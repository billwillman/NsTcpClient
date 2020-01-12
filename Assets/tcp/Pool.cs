using System;
using System.Collections.Generic;

namespace Utils
{

	public class PoolNode<T>
	{
		private LinkedListNode<T> m_PoolNode = null;

		protected virtual void OnFree()
		{
		}

		public void Dispose() {
			if (AbstractPool<T>._DestroyNode (this)) {
				OnFree ();
			}
		}

		public LinkedListNode<T> PPoolNode {
			get {
				if (m_PoolNode == null)
					m_PoolNode = new LinkedListNode<T>(this);
				return m_PoolNode;
			}
		}
	}

	public sealed class AbstractPool<T> where T: new()
	{
		private static LinkedList<T> m_NodePool = new LinkedList<T>();

		internal static bool IsInNodePool(LinkedListNode<T> node) {
			return m_NodePool == node.List;
		}

		internal static bool _DestroyNode(PoolNode<T> node)
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
							return true;
						}
					}
				}
			}
			return false;
		}

		public static PoolNode<T> GetNode()
		{
			PoolNode<T> ret = null;
			lock (m_NodePool) {
				LinkedListNode<T> n = m_NodePool.First;
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
