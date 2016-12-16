using System;
using System.Collections;
using System.Collections.Generic;

namespace NsTcpClient
{
	public class NetManager: Singleton<NetManager>
	{

		public string Ip
		{
			get
			{
				return m_Ip;
			}
		}

		public int Port
		{
			get
			{
				return m_Port;
			}
		}

		public bool ConnectLastServer()
		{
			return ConnectServer(m_Ip, m_Port);
		}

		public bool ConnectServer(string ip, int port)
		{
			if (string.IsNullOrEmpty(ip) || port <= 0)
				return false;

			if (string.Compare(m_Ip, ip) == 0 && m_Port == port)
			{
				if (m_Client != null)
				{
					var status = m_Client.GetState();
					if (status == eClientState.eClient_STATE_CONNECTED || status == eClientState.eClient_STATE_CONNECTING)
						return true;
				}
			} else
			{
				if (m_Client != null)
					m_Client.DisConnect();
			}

			ClearTempList();

			if (m_Client == null)
			{
				m_Client = new ClientSocket(true);
				m_Client.AddStateEvent(OnSocketStateEvent);
			}

			bool ret = m_Client.Connect(ip, port);

			if (ret)
			{
				m_Ip = ip;
				m_Port = port;
			}

			return ret;
		}

		public void Disconnect()
		{
			if (m_Client != null)
				m_Client.DisConnect();
			ClearTempList();
		}

		void OnSocketStateEvent(eClientState status)
		{
			switch (status)
			{
			case eClientState.eClient_STATE_ABORT:
				ClearTempList();
				if (OnSocketAbort != null)
					OnSocketAbort();
				break;
			case eClientState.eClient_STATE_CONNECT_FAIL:
				ClearTempList();
				if (OnConnectResult != null)
					OnConnectResult(false);
				break;
			case eClientState.eClient_STATE_CONNECTED:
				SendTempList();
				if (OnConnectResult != null)
					OnConnectResult(true);
				break;
			}
		}
			
		// Socket异常连接
		public Action OnSocketAbort
		{
			get;
			set;
		}

		// 连接返回
		public Action<bool> OnConnectResult
		{
			get;
			set;
		}

		public void Send (byte[] buf, int packetHandle)
		{
			if (m_Client == null)
				return;
			var status = m_Client.GetState();
			if (status == eClientState.eClient_STATE_CONNECTED)
			{
				m_Client.Send(buf, packetHandle);
			} else if (status == eClientState.eClient_STATE_CONNECTING)
			{
				TempPacket temp = new TempPacket(buf, packetHandle);
				m_TempList.AddLast(temp);
			}
		}

		public void Send (int packetHandle)
		{
			if (m_Client == null)
				return;
			Send(null, packetHandle);
		}

		public void SendStr (string data, int packetHandle)
		{
			if (m_Client == null)
				return;
			byte[] src;
			if (string.IsNullOrEmpty(data))
				src = null;
			else
				src = System.Text.Encoding.ASCII.GetBytes(data);
			Send(src, packetHandle);
		}

		private void ClearTempList()
		{
			m_TempList.Clear();
		}

		private void SendTempList()
		{
			LinkedListNode<TempPacket> node = m_TempList.First;
			while (node != null)
			{
				m_Client.Send(node.Value.data, node.Value.handle);
				node = node.Next;
			}

			ClearTempList();
		}

		private struct TempPacket
		{
			public byte[] data;
			public int handle;

			public TempPacket(byte[] srcData, int packetHandle)
			{
				handle = packetHandle;
				if (srcData == null || srcData.Length <= 0)
					data = null;
				else 
				{
					data = new byte[srcData.Length];
					Buffer.BlockCopy(srcData, 0, data, 0, srcData.Length);
				}
			}
		}

		private LinkedList<TempPacket> m_TempList = new LinkedList<TempPacket>();
		private string m_Ip = string.Empty;
		private int m_Port = 0;
		private ClientSocket m_Client = null;
	}
}
