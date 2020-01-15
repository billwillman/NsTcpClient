using System;
using System.Collections.Generic;
using Utils;

namespace NsTcpClient
{
	public enum eReqType
	{
		eREQ_TYPE_CONNECT = 0,
		eREQ_TYPE_SEND,
	}

	public class tReqHead
	{
        private LinkedListNode<tReqHead> m_ListNode = null;
		public eReqType uReqType;
        public virtual void Dispose() {

        }

        public LinkedListNode<tReqHead> ListNode {
            get {
                if (m_ListNode == null)
                    m_ListNode = new LinkedListNode<tReqHead>(this);
                return m_ListNode;
            }
        }
    }

	internal class tReqConnect: tReqHead
	{
		public int uRemotePort;
		public string szRemoteIp;
		public int timeOut;

		public tReqConnect(string pRemoteIp, int uPort, int timeout)
		{
			uReqType = eReqType.eREQ_TYPE_CONNECT;
			uRemotePort = uPort;
			timeOut = timeout;
			szRemoteIp = pRemoteIp;
		}
	}

	public class tReqSend: tReqHead, IPoolNode<tReqSend>
	{
		public ByteBufferNode pSendData;
		public tReqSend(byte[] pData, int bufSize)
		{
			uReqType = eReqType.eREQ_TYPE_SEND;
            Init(pData, bufSize);
		}

        public tReqSend() {
            uReqType = eReqType.eREQ_TYPE_SEND;
            pSendData = null;
        }
        public void Init(byte[] pData, int bufSize) {
            if ((pData != null) && (bufSize > 0)) {
                if (pSendData != null)
                    pSendData.Dispose();
                pSendData = NetByteArrayPool.GetByteBufferNode(bufSize);
                Buffer.BlockCopy(pData, 0, pSendData.GetBuffer(), 0, bufSize);
            } else {
                pSendData = null;
            }
        }

        private LinkedListNode<IPoolNode<tReqSend>> m_PoolNode = null;
        public LinkedListNode<IPoolNode<tReqSend>> PPoolNode {
            get {
                if (m_PoolNode == null)
                    m_PoolNode = new LinkedListNode<IPoolNode<tReqSend>>(this);
                return m_PoolNode;
            }
        }

        public static tReqSend CreateFromPool(byte[] pData, int bufSize) {
            tReqSend ret = (tReqSend)AbstractPool<tReqSend>.GetNode();
            ret.Init(pData, bufSize);
            return ret;
        }

        public override void Dispose() {
            if (pSendData != null) {
                pSendData.Dispose();
                pSendData = null;
            }

            AbstractPool<tReqSend>._DestroyNode(this);
        }

		public int SendSize {
			get {
				if (pSendData != null)
				{
					return pSendData.DataSize;
				} else {
					return 0;
				}
			}
		}
	}

    public class UdpReqSend : tReqSend
    {
        public string ip;
        public int port;
        public UdpReqSend(byte[] pData, int bufSize, string ip, int port): base(pData, bufSize)
        {
            this.ip = ip;
            this.port = port;
        }
    }
}