using System.Collections;

namespace NsTcpClient
{

	public enum eClientState
	{
		eCLIENT_STATE_NONE = 0,			
		eClient_STATE_CONNECTING,	
		eClient_STATE_CONNECTED,	
		eClient_STATE_CONNECT_FAIL,	
		eClient_STATE_ABORT,		
		eClient_STATE_DISCONNECT,
	};

	public interface ITcpClient {
		void Release();
		bool Connect(string remoteIp, int remotePort, int timeOut = -1);
		eClientState GetState();
		bool Send(byte[] pData);
		bool HasReadData();
		int GetReadData(byte[] pBuffer, int offset);
	}
}
