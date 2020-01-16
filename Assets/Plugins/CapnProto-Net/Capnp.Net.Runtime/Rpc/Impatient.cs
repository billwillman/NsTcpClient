﻿using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Capnp.Rpc
{
    /// <summary>
    /// Provides support for promise pipelining.
    /// </summary>
    public static class Impatient
    {
        static readonly ConditionalWeakTable<Task, IPromisedAnswer> _taskTable = new ConditionalWeakTable<Task, IPromisedAnswer>();
        static readonly ThreadLocal<IRpcEndpoint> _askingEndpoint = new ThreadLocal<IRpcEndpoint>();

        /// <summary>
        /// Attaches a continuation to the given promise and registers the resulting task for pipelining.
        /// </summary>
        /// <typeparam name="T">Task result type</typeparam>
        /// <param name="promise">The promise</param>
        /// <param name="then">The continuation</param>
        /// <returns>Task representing the future answer</returns>
        /// <exception cref="ArgumentNullException"><paramref name="promise"/> or <paramref name="then"/> is null.</exception>
        /// <exception cref="ArgumentException">The pomise was already registered.</exception>
        public static Task<T> MakePipelineAware<T>(IPromisedAnswer promise, Func<DeserializerState, T> then)
        {
            async Task<T> AwaitAnswer()
            {
                return then(await promise.WhenReturned);
            }

            var rtask = AwaitAnswer();

            try
            {
                // Really weird: We'd expect AwaitAnswer() to initialize a new Task instance upon each invocation.
                // However, this does not seem to be always true (as indicated by CI test suite). An explanation might be
                // that the underlying implementation recycles Task instances (um, really? doesn't make sense. But the
                // observation doesn't make sense, either).

                _taskTable.Add(rtask, promise);
            }
            catch (ArgumentException)
            {
                if (rtask.IsCompleted)
                {
                    // Force .NET to create a new Task instance
                    if (rtask.IsCanceled)
                    {
                        rtask = Task.FromCanceled<T>(new CancellationToken(true));
                    }
                    else if (rtask.IsFaulted)
                    {
                        rtask = Task.FromException<T>(rtask.Exception.InnerException);
                    }
                    else
                    {
                        rtask = Task.FromResult<T>(rtask.Result);
                    }
                    
                    _taskTable.Add(rtask, promise);
                }
                else
                {
                    throw new InvalidOperationException("What the heck is wrong with Task?");
                }
            }

            return rtask;
        }

        /// <summary>
        /// Looks up the underlying promise which was previously registered for the given Task using MakePipelineAware.
        /// </summary>
        /// <param name="task"></param>
        /// <returns>The underlying promise</returns>
        /// <exception cref="ArgumentNullException"><paramref name="task"/> is null.</exception>
        /// <exception cref="ArgumentException">The task was not registered using MakePipelineAware.</exception>
        public static IPromisedAnswer GetAnswer(Task task)
        {
            if (!_taskTable.TryGetValue(task, out var answer))
            {
                throw new ArgumentException("Unknown task");
            }

            return answer;
        }

        internal static IPromisedAnswer TryGetAnswer(Task task)
        {
            _taskTable.TryGetValue(task, out var answer);
            return answer;
        }

        static async Task<Proxy> AwaitProxy<T>(Task<T> task) where T: class
        {
            var item = await task;

            switch (item)
            {
                case Proxy proxy:
                    return proxy;

                case null:
                    return null;
            }

            var skel = Skeleton.GetOrCreateSkeleton(item, false);
            var localCap = LocalCapability.Create(skel);
            return CapabilityReflection.CreateProxy<T>(localCap);
        }

        /// <summary>
        /// Returns a local "lazy" proxy for a given Task. 
        /// This is not real promise pipelining and will probably be removed.
        /// </summary>
        /// <typeparam name="TInterface">Capability interface type</typeparam>
        /// <param name="task">The task</param>
        /// <param name="memberName">debugging aid</param>
        /// <param name="sourceFilePath">debugging aid</param>
        /// <param name="sourceLineNumber">debugging aid</param>
        /// <returns>A proxy for the given task.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="task"/> is null.</exception>
        /// <exception cref="InvalidCapabilityInterfaceException"><typeparamref name="TInterface"/> did not
        /// quality as capability interface.</exception>
        [Obsolete("Call Eager<TInterface>(task, true) instead")]
        public static TInterface PseudoEager<TInterface>(this Task<TInterface> task,
            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
            where TInterface : class
        {
            var lazyCap = new LazyCapability(AwaitProxy(task));
            return CapabilityReflection.CreateProxy<TInterface>(lazyCap, memberName, sourceFilePath, sourceLineNumber) as TInterface;
        }

        static readonly MemberAccessPath Path_OneAndOnly = new MemberAccessPath(0U);

        /// <summary>
        /// Returns a promise-pipelined Proxy for a remote method invocation Task.
        /// </summary>
        /// <typeparam name="TInterface">Capability interface type</typeparam>
        /// <param name="task">Task returning an interface</param>
        /// <param name="allowNoPipeliningFallback">If this flag is 'false', the <paramref name="task"/> MUST have been returned from a remote
        /// method invocation on a generated Proxy interface. Since this is the prerequisite for promise pipelining to work, the method throws an
        /// exception if the requirement is not met (i.e. the passed some Task instance was constructed "somewhere else"). Setting this flag to 'true'
        /// prevents such an exception. The method falls back to a local "lazy" proxy for the given Task. It is fully usable, but does not perform
        /// any promise pipelining (as specified for Cap'n Proto).</param>
        /// <returns>A proxy for the given future.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="task"/> is null.</exception>
        /// <exception cref="InvalidCapabilityInterfaceException"><typeparamref name="TInterface"/> did not qualify as capability interface.</exception>
        /// <exception cref="ArgumentException">The task was not returned from a remote method invocation. Promise pipelining won't work.
        /// Setting <paramref name="allowNoPipeliningFallback"/>> to 'true' prevents this exception. 
        /// OR: Mismatch between generic type arguments (if capability interface is generic).</exception>
        /// <exception cref="InvalidOperationException">Mismatch between generic type arguments (if capability interface is generic).</exception>
        /// <exception cref="System.Reflection.TargetInvocationException">Problem with instatiating the Proxy (constructor threw exception).</exception>
        /// <exception cref="MemberAccessException">Caller does not have permission to invoke the Proxy constructor.</exception>
        /// <exception cref="TypeLoadException">Problem with building the Proxy type, or problem with loading some dependent class.</exception>
        public static TInterface Eager<TInterface>(this Task<TInterface> task, bool allowNoPipeliningFallback = false)
            where TInterface : class
        {
            var answer = TryGetAnswer(task);
            if (answer == null)
            {
                if (!allowNoPipeliningFallback)
                {
                    throw new ArgumentException("The task was not returned from a remote method invocation. See documentation for details.");
                }

                var lazyCap = new LazyCapability(AwaitProxy(task));
                return CapabilityReflection.CreateProxy<TInterface>(lazyCap) as TInterface;
            }
            else
            {
                return CapabilityReflection.CreateProxy<TInterface>(answer.Access(Path_OneAndOnly)) as TInterface;
            }
        }

        internal static IRpcEndpoint AskingEndpoint
        {
            get => _askingEndpoint.Value;
            set { _askingEndpoint.Value = value; }
        }

        /// <summary>
        /// Checks whether a given task belongs to a pending RPC and requests a tail call if applicable.
        /// </summary>
        /// <typeparam name="T">Task result type</typeparam>
        /// <param name="task">Task to request</param>
        /// <param name="func">Converts the task's result to a SerializerState</param>
        /// <returns>Tail-call aware task</returns>
        public static async Task<AnswerOrCounterquestion> MaybeTailCall<T>(Task<T> task, Func<T, SerializerState> func)
        {
            if (TryGetAnswer(task) is PendingQuestion pendingQuestion &&
                pendingQuestion.RpcEndpoint == AskingEndpoint)
            {
                pendingQuestion.IsTailCall = true;
                return pendingQuestion;
            }
            else
            {
                return func(await task);
            }
        }

        /// <summary>
        /// Overload for tuple-typed tasks
        /// </summary>
        public static Task<AnswerOrCounterquestion> MaybeTailCall<T1, T2>(Task<(T1, T2)> task, Func<T1, T2, SerializerState> func)
        {
            return MaybeTailCall(task, (ValueTuple<T1, T2> t) => func(t.Item1, t.Item2));
        }

        /// <summary>
        /// Overload for tuple-typed tasks
        /// </summary>
        public static Task<AnswerOrCounterquestion> MaybeTailCall<T1, T2, T3>(Task<(T1, T2, T3)> task, Func<T1, T2, T3, SerializerState> func)
        {
            return MaybeTailCall(task, (ValueTuple<T1, T2, T3> t) => func(t.Item1, t.Item2, t.Item3));
        }

        /// <summary>
        /// Overload for tuple-typed tasks
        /// </summary>
        public static Task<AnswerOrCounterquestion> MaybeTailCall<T1, T2, T3, T4>(Task<(T1, T2, T3, T4)> task, Func<T1, T2, T3, T4, SerializerState> func)
        {
            return MaybeTailCall(task, (ValueTuple<T1, T2, T3, T4> t) => func(t.Item1, t.Item2, t.Item3, t.Item4));
        }

        /// <summary>
        /// Overload for tuple-typed tasks
        /// </summary>
        public static Task<AnswerOrCounterquestion> MaybeTailCall<T1, T2, T3, T4, T5>(Task<(T1, T2, T3, T4, T5)> task, Func<T1, T2, T3, T4, T5, SerializerState> func)
        {
            return MaybeTailCall(task, (ValueTuple<T1, T2, T3, T4, T5> t) => func(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5));
        }

        /// <summary>
        /// Overload for tuple-typed tasks
        /// </summary>
        public static Task<AnswerOrCounterquestion> MaybeTailCall<T1, T2, T3, T4, T5, T6>(Task<(T1, T2, T3, T4, T5, T6)> task, Func<T1, T2, T3, T4, T5, T6, SerializerState> func)
        {
            return MaybeTailCall(task, (ValueTuple<T1, T2, T3, T4, T5, T6> t) => func(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6));
        }

        /// <summary>
        /// Overload for tuple-typed tasks
        /// </summary>
        public static Task<AnswerOrCounterquestion> MaybeTailCall<T1, T2, T3, T4, T5, T6, T7>(Task<(T1, T2, T3, T4, T5, T6, T7)> task, Func<T1, T2, T3, T4, T5, T6, T7, SerializerState> func)
        {
            return MaybeTailCall(task, (ValueTuple<T1, T2, T3, T4, T5, T6, T7> t) => func(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, t.Item7));
        }
    }
}
