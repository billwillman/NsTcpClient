using System;
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
		public eReqType uReqType;
        public virtual void Dispose() {

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

	public class tReqSend: tReqHead
	{
		public ByteBufferNode pSendData;
		public tReqSend(byte[] pData, int bufSize)
		{
			uReqType = eReqType.eREQ_TYPE_SEND;
			if ((pData != null) && (bufSize > 0)) {
                pSendData = NetByteArrayPool.GetByteBufferNode(bufSize);
				Buffer.BlockCopy(pData, 0, pSendData.GetBuffer(), 0, bufSize);
			} else {
				pSendData = null;
			}
		}

        public override void Dispose() {
            if (pSendData != null) {
                pSendData.Dispose();
                pSendData = null;
            }
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