using System;

namespace NsTcpClient
{
	internal enum eReqType
	{
		eREQ_TYPE_CONNECT = 0,
		eREQ_TYPE_SEND,
	}

	internal class tReqHead
	{
		public eReqType uReqType;
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

	internal class tReqSend: tReqHead
	{
		public byte[] pSendData;
		public tReqSend(byte[] pData, int bufSize)
		{
			uReqType = eReqType.eREQ_TYPE_SEND;
			if ((pData != null) && (bufSize > 0)) {
				pSendData = new byte[bufSize];
				Buffer.BlockCopy(pData, 0, pSendData, 0, bufSize);
			} else {
				pSendData = null;
			}
		}

		public int SendSize {
			get {
				if (pSendData != null)
				{
					return pSendData.Length;
				} else {
					return 0;
				}
			}
		}
	}

    internal class UdpReqSend : tReqSend
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