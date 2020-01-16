#define USE_PROTOBUF_NET
#define USE_CapnProto

using System;
using System.Collections;
using System.Collections.Generic;
#if USE_CapnProto
using CapnProto;
#endif

namespace NsTcpClient
{
	public class NetManager: Singleton<NetManager>
	{

		public NetManager()
		{
			m_Client = new ClientSocket();
			m_Client.AddStateEvent(OnSocketStateEvent);
			m_Timer = TimerMgr.Instance.CreateTimer(0, true);
			m_Timer.AddListener(OnTimerEvent);

            NetByteArrayPool.InitMgr();

        }

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
			if (m_Client == null || string.IsNullOrEmpty(ip) || port <= 0)
				return false;

			if (string.Compare(m_Ip, ip) == 0 && m_Port == port)
			{
				var status = m_Client.GetState();
				if (status == eClientState.eClient_STATE_CONNECTED || status == eClientState.eClient_STATE_CONNECTING)
					return true;
				else
					m_Client.DisConnect();
			} else
			{
				if (m_Client != null)
					m_Client.DisConnect();
			}

			ClearTempList();

			bool ret = m_Client.Connect(ip, port);

			if (ret)
			{
				m_Ip = ip;
				m_Port = port;
			}

			return ret;
		}

        public eClientState ClietnState
        {
            get
            {
                if (m_Client == null)
                    return eClientState.eCLIENT_STATE_NONE;
                return m_Client.GetState();
            }
        }

		public void Disconnect()
		{
			if (m_Client != null)
				m_Client.DisConnect();
			ClearTempList();
		}

		void OnTimerEvent(Timer obj, float timer)
		{
			if (m_Client != null)
			{
				m_Client.Execute();
			}
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

		public void AddPacketListener(int header, OnPacketRead callBack)
		{
			if (m_Client == null)
				return;
			m_Client.AddPacketListener(header, callBack);
		}

        public void AddServerMessageClass(int header, System.Type messageClass)
        {
            if (m_Client == null || messageClass == null)
                return;
            m_Client.RegisterServerMessageClass(header, messageClass);
        }

        public void Send(byte[] buf, int packetHandle, int bufSize = -1)
		{
			if (m_Client == null)
				return;
			var status = m_Client.GetState();
			if (status == eClientState.eClient_STATE_CONNECTED)
			{
                m_Client.Send(buf, packetHandle, bufSize);
			} else if (status == eClientState.eClient_STATE_CONNECTING)
			{
                TempPacket temp = new TempPacket(buf, packetHandle, bufSize);
				m_TempList.AddLast(temp);
			}
		}

		public void Send (int packetHandle)
		{
			if (m_Client == null)
				return;
			Send(null, packetHandle);
		}

#if USE_CapnProto
        public void SendCapnProto(CapnProtoMsg msg, int packetHandle) {
            ByteBufferNode node = NetByteArrayPool.GetByteBufferNode();
            var buffer = node.GetBuffer();
            System.IO.MemoryStream stream = new System.IO.MemoryStream(buffer);
            msg.WriteToStream(stream);
            Send(buffer, packetHandle, (int)stream.Length);
            stream.Dispose();
            node.Dispose();
        }
#endif

#if USE_PROTOBUF_NET
            public void SendProtoBuf<T>(T data, int packetHandle) where T: class, Google.Protobuf.IMessage<T>
        {
            if (data == null)
            {
                return;
            }

            // ProtoBuf 2.0接口
            /*
			System.IO.MemoryStream stream = new System.IO.MemoryStream ();
			ProtoBuf.Serializer.Serialize<T> (stream, data);
			byte[] buf = stream.ToArray ();
			stream.Close ();
			stream.Dispose ();
			Send (buf, packetHandle);
            */

            // protobuf 3.0接口
           //  byte[] buf = ProtoMessageMgr.GetInstance().ToBuffer<T>(data);
           // Send(buf, packetHandle);
            
            // 优化后版本使用byte[]池
            int outSize;
            var stream = ProtoMessageMgr.ToBufferNode<T>(data, out outSize);
            if (stream == null)
                return;
            try
            {
                if (outSize <= 0)
                    return;
                var buf = stream.GetBuffer();
                Send(buf, packetHandle, outSize);
            }
            finally
            {
                if (stream != null)
                {
                    stream.Dispose();
                    stream = null;
                }
            }
            
        }
#endif

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

        // 发送AbstractClientMessage
        public void SendMessage(int packetHandle, AbstractClientMessage message)
        {
            if (m_Client == null)
                return;
            if (message != null)
            {
                try
                {
                    message.DoSend();
                    long bufSize;
                    byte[] buffer = message.GetBuffer(out bufSize);
                    if (bufSize > int.MaxValue)
                        return;
                    Send(buffer, packetHandle, (int)bufSize);
                }
                finally
                {
                    message.Dispose();
                }

            } else
            {
                Send(packetHandle);
            }
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

            public TempPacket(byte[] srcData, int packetHandle, int bufSize = -1)
			{
				handle = packetHandle;
                if (srcData == null || srcData.Length <= 0 || bufSize == 0)
					data = null;
				else 
				{
                    if (bufSize < 0)
                        bufSize = srcData.Length;
                    data = new byte[bufSize];
                    Buffer.BlockCopy(srcData, 0, data, 0, bufSize);
				}
			}
		}

		private LinkedList<TempPacket> m_TempList = new LinkedList<TempPacket>();
		private string m_Ip = string.Empty;
		private int m_Port = 0;
		private ClientSocket m_Client = null;
		private ITimer m_Timer = null;
#if USE_CapnProto
        
       
#endif
    }
}
