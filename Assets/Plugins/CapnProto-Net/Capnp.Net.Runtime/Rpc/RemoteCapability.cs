﻿using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Capnp.Rpc
{

    abstract class RemoteCapability : RefCountingCapability
    {
        protected readonly IRpcEndpoint _ep;

        protected RemoteCapability(IRpcEndpoint ep)
        {
            _ep = ep;
        }

        internal override IPromisedAnswer DoCall(ulong interfaceId, ushort methodId, DynamicSerializerState args)
        {
            var call = SetupMessage(args, interfaceId, methodId);
            Debug.Assert(call.Target.which != MessageTarget.WHICH.undefined);
            return _ep.BeginQuestion(this, args);
        }

        protected virtual Call.WRITER SetupMessage(DynamicSerializerState args, ulong interfaceId, ushort methodId)
        {
            var callMsg = args.MsgBuilder.BuildRoot<Message.WRITER>();

            callMsg.which = Message.WHICH.Call;

            var call = callMsg.Call;
            call.AllowThirdPartyTailCall = false;
            call.InterfaceId = interfaceId;
            call.MethodId = methodId;
            call.Params.Content = args;

            return call;
        }
    }
}
