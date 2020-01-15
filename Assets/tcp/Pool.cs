using System;
using System.Collections.Generic;

namespace Utils
{

    public interface IPoolNode<T> {
        LinkedListNode<IPoolNode<T>> PPoolNode {
            get;
        }

        void Dispose();
    }


    public class PoolNode<T>: IPoolNode<T> where T : IPoolNode<T>, new() {
		private LinkedListNode<IPoolNode<T>> m_PoolNode = null;

		protected virtual void OnFree()
		{
		}

		public void Dispose() {
			if (IsDisposed)
				return;
			OnFree ();
			AbstractPool<T>._DestroyNode (this);
		}

		private bool IsDisposed
		{
			get {
				if (m_PoolNode == null)
					return false;
				// 这里不需要锁
				return AbstractPool<T>.IsInNodePool (m_PoolNode);
			}
		}

		public LinkedListNode<IPoolNode<T>> PPoolNode {
			get {
				if (m_PoolNode == null)
					m_PoolNode = new LinkedListNode<IPoolNode<T>>(this);
				return m_PoolNode;
			}
		}
	}

	public sealed class AbstractPool<T> where T: IPoolNode<T>, new()
	{
		private static LinkedList<IPoolNode<T>> m_NodePool = new LinkedList<IPoolNode<T>>();

		internal static bool IsInNodePool(LinkedListNode<IPoolNode<T>> node) {
			// 这里不需要锁
			//lock (m_NodePool) {
				return (node != null) && (m_NodePool == node.List);
			//}
		}

		internal static void _DestroyNode(IPoolNode<T> node)
		{
			if (node != null) {
				var n = node.PPoolNode;
				if (n.List != m_NodePool) {
					lock (m_NodePool) {
						var list = n.List;
						if (list != m_NodePool) {
							if (list != null)
								list.Remove (n);
							m_NodePool.AddLast (n);
						}
					}
				}
			}
		}

		public static IPoolNode<T> GetNode()
		{
            IPoolNode<T> ret = null;
			lock (m_NodePool) {
				LinkedListNode<IPoolNode<T>> n = m_NodePool.First;
				if (n != null) {
					m_NodePool.Remove (n);
					ret = n.Value;
				}
			}
			if (ret != null)
				return ret;

			ret = new T();
			return ret;
		}
	}
}
