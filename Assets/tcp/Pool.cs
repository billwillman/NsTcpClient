using System;
using System.Collections.Generic;

namespace Utils
{

	public class PoolNode<T> where T : PoolNode<T>, new() {
		private LinkedListNode<PoolNode<T>> m_PoolNode = null;

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

		public LinkedListNode<PoolNode<T>> PPoolNode {
			get {
				if (m_PoolNode == null)
					m_PoolNode = new LinkedListNode<PoolNode<T>>(this);
				return m_PoolNode;
			}
		}
	}

	public sealed class AbstractPool<T> where T: PoolNode<T>, new()
	{
		private static LinkedList<PoolNode<T>> m_NodePool = new LinkedList<PoolNode<T>>();

		internal static bool IsInNodePool(LinkedListNode<PoolNode<T>> node) {
			// 这里不需要锁
			//lock (m_NodePool) {
				return (node != null) && (m_NodePool == node.List);
			//}
		}

		internal static void _DestroyNode(PoolNode<T> node)
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

		public static PoolNode<T> GetNode()
		{
			PoolNode<T> ret = null;
			lock (m_NodePool) {
				LinkedListNode<PoolNode<T>> n = m_NodePool.First;
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
