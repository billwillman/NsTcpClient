using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.IO;

namespace NsTcpClient {

    public sealed class ByteBufferNode {
        private LinkedListNode<ByteBufferNode> m_LinkedListNode = null;
        private byte[] m_Buffer = new byte[NetByteArrayPool._cSmallBufferSize];
        private int m_DataSize = NetByteArrayPool._cSmallBufferSize;

        public ByteBufferNode(int dataSize) {
            _InitDataSize(dataSize);
        }

        public int DataSize {
            get {
                return m_DataSize;
            }
        }

        internal void _InitDataSize(int dataSize) {
            m_DataSize = dataSize;
            if (dataSize > NetByteArrayPool._cSmallBufferSize)
                throw (new Exception());
        }

        public LinkedListNode<ByteBufferNode> LinkedListNode {
            get {
                if (m_LinkedListNode == null)
                    m_LinkedListNode = new LinkedListNode<ByteBufferNode>(this);
                return m_LinkedListNode;
            }
        }

        public byte[] Buffer {
            get {
                return m_Buffer;
            }
        }

		public byte[] GetBuffer()
		{
			return m_Buffer;
		}

        public int Length {
            get {
                if (m_Buffer != null)
                    return m_Buffer.Length;
                return 0;
            }
        }

        public void Dispose() {
            NetByteArrayPool._DestroyBuffer(this);
        }

    }
    
    // 网路数据池
    public static class NetByteArrayPool {
        private static RecyclableMemoryStreamManager m_Mgr = null;
        private static string _cRcyclableTag = "NetByteArrayPool";
        public static readonly int _cSmallBufferSize = 4 * 1024;
        private static int _cLargeBufSize = 64 * 1024;
        public static void InitMgr() {
            if (m_Mgr != null)
                return;
            m_Mgr = new RecyclableMemoryStreamManager(_cSmallBufferSize, _cLargeBufSize, _cLargeBufSize);
        }

        // 自己的池
        private static LinkedList<ByteBufferNode> m_ByteNodePool = new LinkedList<ByteBufferNode>();

        public static ByteBufferNode GetByteBufferNode(int dataSize = -1) {
            if (dataSize <= 0)
                dataSize = NetByteArrayPool._cSmallBufferSize;
			
			ByteBufferNode ret = null;
			lock (m_ByteNodePool) {
				LinkedListNode<ByteBufferNode> n = m_ByteNodePool.First;
				if (n != null) {
					m_ByteNodePool.Remove (n);
					ret = n.Value;
				}
			}
			if (ret != null) {
				ret._InitDataSize (dataSize);
				return ret;
			}

            ret = new ByteBufferNode(dataSize);
            return ret;
        }

        internal static void _DestroyBuffer(ByteBufferNode node) {
            if (node != null) {
                var n = node.LinkedListNode;
				if (n.List != m_ByteNodePool) {
					lock (m_ByteNodePool) {
						var list = n.List;
						if (list != m_ByteNodePool) {
							if (list != null)
								list.Remove (n);
							m_ByteNodePool.AddLast (n);
						}
                    }
                }
            }
        }

        public static MemoryStream GetBuffer(int bufSize) {
            if (bufSize <= 0)
                return null;
            InitMgr();
            return m_Mgr.GetStream(_cRcyclableTag, bufSize);
        }

        /*
        public static void FreeBuffer(MemoryStream stream) {
            if (stream == null || stream.Length <= 0)
                return;
            InitMgr();
            stream.Dispose();
            stream = null;
        }
        */
    }
}
