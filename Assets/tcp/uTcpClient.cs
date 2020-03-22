//#define USE_NETORDER
#define USE_PROTOBUF_NET
#define USE_CapnProto

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Runtime.CompilerServices;
#if USE_NETORDER
using System.Net;
#endif
#if USE_CapnProto
using CapnProto;
#endif
using Utils;
using System.IO;

namespace NsTcpClient
{
	[StructLayoutAttribute(LayoutKind.Sequential, Pack=1)]
	public struct GamePackHeader
	{
		public int dataSize;
		public uint headerCrc32;
		public uint dataCrc32;
		public int header;
	}
	
	public enum GamePacketStatus {
        GPNone = 0,
        GPProcessing,
        GPDone
    }

	// 1. data is ProtoBuf
	// 2. data is json
	public class GamePacket: PoolNode<GamePacket>
	{
        public GamePackHeader header;
        //public byte[] data = null;
		public ByteBufferNode data = null;

        private LinkedListNode<GamePacket> m_LinkedNode = null;

        public LinkedListNode<GamePacket> LinkedNode {
           get {
                if (m_LinkedNode == null)
                    m_LinkedNode = new LinkedListNode<GamePacket>(this);
                return m_LinkedNode;
            }
                
        }

        private void UnInit() {
            status = GamePacketStatus.GPNone;
            if (data != null) {
                data.Dispose();
                data = null;
            }
        }

        private void Init() {
            UnInit();
        }

        public static GamePacket CreateFromPool() {
			GamePacket ret = (GamePacket)AbstractPool<GamePacket>.GetNode ();
            ret.Init();
            return ret;
        }

		protected override void OnFree() {
			UnInit();
        }

        public bool hasData()
		{
			if ((header.dataSize <= 0) || (data == null))
				return false;
			return (data.DataSize > 0);
		}

		public string dataToString()
		{
			if (data == null)
				return string.Empty;
			return data.ToString ();
		}
		
		 // 是否正在处理
        public GamePacketStatus status {
            get;
            private set;
        }

        public void DoDone() {
            status = GamePacketStatus.GPDone;
        }

        public void DoProcessing() {
            status = GamePacketStatus.GPProcessing;
        }

#if USE_PROTOBUF_NET
		// Data为ProtoBuf，转成对象
        public T ProtoBufToObject<T>() where T : class, Google.Protobuf.IMessage<T>
		{
            
            
			if (data == null)
				return null;
            // 老版本ProtoBuf 2.0
            /*
			System.IO.MemoryStream stream = new System.IO.MemoryStream ();
			stream.Write (data, 0, data.Length);
			stream.Seek (0, System.IO.SeekOrigin.Begin);
			T ret = ProtoBuf.Serializer.Deserialize<T> (stream);

			stream.Close ();
			stream.Dispose ();

			return ret;
            */
            // 更新为3.0 ProtoBuf

            // 统一接口
            var buf = data.GetBuffer();
            if (buf == null)
                return null;
            T ret = ProtoMessageMgr.GetInstance().Parser<T>(buf, header.dataSize) as T;
            return ret;
		}
#endif
	}

	public delegate void OnPacketRead(GamePacket packet);

	public abstract class ClientPackageListener
	{
		public ClientPackageListener(ClientSocket socket)
		{
			mSocket = socket;
		}

		public void AddPacketListener(int header, OnPacketRead callBack)
		{
			if (mSocket != null)
				mSocket.AddPacketListener (header, callBack);
		}

		public void RemovePacketListener(int header)
		{
			if (mSocket != null)
				mSocket.RemovePacketListener (header);
		}

		private ClientSocket mSocket = null;
	}

	public class ClientSocket
	{
		static public readonly int cSocketConnWaitTime = 5000;

        public delegate void OnSocketStateEvent(eClientState state);

		public ClientSocket(bool isUseTimer = false)
		{

			if (isUseTimer)
				mTimer = new System.Timers.Timer (1.0f);

			Stop ();
			if (mTimer != null) {
				mTimer.Elapsed += new System.Timers.ElapsedEventHandler (delegate {
					Execute ();
					Thread.Sleep(1);
				});
			}
		}

        public void AddStateEvent(OnSocketStateEvent evt) 
        {
            if (evt == null)
                return;

            mStateEvents += evt;
        }

        public void RemoveStateEvent(OnSocketStateEvent evt)
        {
            if (evt == null)
                return;

            mStateEvents -= evt;
        }

		public eClientState GetState()
		{
			if (mTcpClient == null)
				return eClientState.eCLIENT_STATE_NONE;
			if (mConnecting)
				return eClientState.eClient_STATE_CONNECTING;
			return mTcpClient.GetState ();
		}

        /*
		private void CalcHeaderCrc(ref GamePackHeader header, byte[] dst) {
			int headerCrcSize = Marshal.SizeOf (header.headerCrc32);
			int sz = Marshal.SizeOf (header) - headerCrcSize;
			mCrc.Crc (dst, headerCrcSize, sz);
			header.headerCrc32 = (uint)mCrc.Value;
			byte[] crc = BitConverter.GetBytes (header.headerCrc32);
			Buffer.BlockCopy (crc, 0, dst, 4, crc.Length);
		}*/

		// 支持发送buf为null
		unsafe public void Send (byte[] buf, int packetHandle, int bufSize = -1)
		{
			if (mConnecting || (mTcpClient == null))
				return;

			if (mTcpClient.GetState () != eClientState.eClient_STATE_CONNECTED)
				return;

            if (bufSize < 0 && buf != null)
                bufSize = buf.Length;


            bool hasBufData = (buf != null) && (bufSize > 0);

			GamePackHeader header = new GamePackHeader ();

			if (hasBufData)
				header.dataSize = bufSize;
			else
				header.dataSize = 0;
			
			header.header = packetHandle;

			if (hasBufData)
			{
				mCrc.Crc (buf, bufSize);
				header.dataCrc32 = (uint)mCrc.Value;
			} else
				header.dataCrc32 = 0;

			int headerSize = Marshal.SizeOf (header);
			int dstSize = headerSize;
			if (hasBufData)
				dstSize += bufSize;

            // 下面注释是未优化代码
            //byte[] dstBuffer = new byte[dstSize];
            // 此处已优化
			var dstStream = NetByteArrayPool.GetByteBufferNode(dstSize);
            try {
                byte[] dstBuffer = dstStream.GetBuffer();

                // 此处可优化，可以考虑后续使用RINGBUF优化，RINGBUF用完可以自动关闭掉连接
              //  IntPtr pStruct = Marshal.AllocHGlobal(headerSize);
                try {
                    //   Marshal.StructureToPtr(header, pStruct, false);
                    //    Marshal.Copy(pStruct, dstBuffer, 0, headerSize);

                    // 此处是上面优化代码
                    byte* pHeader = (byte*)&header;
                    Marshal.Copy((IntPtr)pHeader, dstBuffer, 0, headerSize);

#if USE_NETORDER
				// Calc header Crc
				CalcHeaderCrc (ref header, dstBuffer);

				// used net
				header.headerCrc32 = (uint)IPAddress.HostToNetworkOrder(header.headerCrc32);
				header.dataCrc32 = (uint)IPAddress.HostToNetworkOrder(header.dataCrc32);
				header.header = IPAddress.HostToNetworkOrder(header.header);
				header.dataSize = IPAddress.HostToNetworkOrder(header.dataSize);

				Marshal.StructureToPtr(header, pStruct, false);
				Marshal.Copy(pStruct, dstBuffer, 0, headerSize);
#endif
                } finally {
                //    Marshal.FreeHGlobal(pStruct);
                }

#if USE_NETORDER
#else
                // Calc header Crc
              //  CalcHeaderCrc(ref header, dstBuffer);
#endif
                if (hasBufData)
                    Buffer.BlockCopy(buf, 0, dstBuffer, headerSize, bufSize);
                mTcpClient.Send(dstBuffer, dstSize);
            } finally {
                if (dstStream != null) {
                    dstStream.Dispose();
                    dstStream = null;
                }
            }
		}

		public void SendString(string str, int packetHandle)
		{
			if (string.IsNullOrEmpty (str))
				return;
			byte[] buf = System.Text.Encoding.UTF8.GetBytes (str);
			Send (buf, packetHandle);
		}

        public bool Connect(string ip, int port)
		{
			DisConnect ();
			mTcpClient = new TcpClient ();
            mTcpClient.OnThreadBufferProcess = OnThreadBufferProcess;
            bool ret = mTcpClient.Connect(ip, port, cSocketConnWaitTime);
			if (ret) {
				mConnecting = true;
				Start();
			} else {
				mTcpClient.Release ();
				mTcpClient = null;
			}

			return ret;
		}

		public void DisConnect()
		{
			Stop ();
			if (mTcpClient != null) {
				mTcpClient.Release ();
				mTcpClient = null;
			}
            OnClearData();
			mConnecting = false;
			mAbort = false;
		}
		
		private void OnClearData() {
            ClearAllProcessPackets();
            mRecvSize = 0;
        }

		private void Start()
		{
			if ((mTimer != null) && (!mTimer.Enabled)) {
				mTimer.Enabled = true;
				mTimer.Start ();
			}
		}

		private void Stop()
		{
			if ((mTimer != null) && mTimer.Enabled) {
				mTimer.Enabled = false;
				mTimer.Stop ();
			}
				
		}

		// 声明为同步函数
		//[MethodImplAttribute(MethodImplOptions.Synchronized)]
		public bool Execute()
		{
		//	string threadId = Thread.CurrentThread.ManagedThreadId.ToString ();
		//	Console.WriteLine (threadId);

			if (mTcpClient == null) {
				Stop ();
				return false;
			}

			if (mConnecting) {
				eClientState state = mTcpClient.GetState ();
				if ((state == eClientState.eClient_STATE_CONNECTING) || 
					(state == eClientState.eCLIENT_STATE_NONE))
					return true; 

				if ((state == eClientState.eClient_STATE_CONNECT_FAIL) ||
				    (state == eClientState.eClient_STATE_ABORT)) {
					mConnecting = false;
					mAbort = false;
					mTcpClient.Release ();
					mTcpClient = null;
					OnClearData();

					// Call Event Error
                    if (mStateEvents != null)
                    {
                        mStateEvents(state);
                    }

					return false;
				} else if (state == eClientState.eClient_STATE_CONNECTED) {
					mConnecting = false;
					mAbort = false;

					// Call Event Success
                    if (mStateEvents != null)
                    {
                        mStateEvents(state);
                    }

					return true;
				}

				mConnecting = false;
			}

            ProcessPackets ();

            if (mTcpClient != null && !mTcpClient.HasReadData ()) {
                if (!mAbort) {
                    eClientState state = mTcpClient.GetState ();
                    if (state == eClientState.eClient_STATE_ABORT) {
                        mAbort = true;
                        OnClearData();
                        // Call Event Abort
                        if (mStateEvents != null)
                        {
                            mStateEvents(state);
                        }

                        return false;
                    }
                }
            }
            return true;
                   
		}

        // 子线程调用
        private unsafe void OnThreadBufferProcess(TcpClient tcp)
        {
            if (tcp == null)
                return;

            int recvsize = mTcpClient.GetReadDataNoLock(mRecvBuffer, mRecvSize);
            if (recvsize > 0) {
                mRecvSize += recvsize;
                int recvBufSz = mRecvSize;
                int i = 0;
                GamePackHeader header = new GamePackHeader();

                int headerSize = Marshal.SizeOf(header);

                // 优化掉了
              //  IntPtr headerBuffer = Marshal.AllocHGlobal(headerSize);
                try {
                    while (recvBufSz - i >= headerSize) {
                        byte* headerBuffer = (byte*)&header;
                        Marshal.Copy(mRecvBuffer, i, (IntPtr)headerBuffer, headerSize);
                       // 优化掉了
                       // header = (GamePackHeader)Marshal.PtrToStructure(headerBuffer, typeof(GamePackHeader));
                        #if USE_NETORDER
                        // used Net
                        header.headerCrc32 = (uint)IPAddress.NetworkToHostOrder(header.headerCrc32);
                        header.dataCrc32 = (uint)IPAddress.NetworkToHostOrder(header.dataCrc32);
                        header.header = IPAddress.NetworkToHostOrder(header.header);
                        header.dataSize = IPAddress.NetworkToHostOrder(header.dataSize);
                        #endif
                        if ((recvBufSz - i) < (header.dataSize + headerSize))
                            break;
                        // GamePacket packet = new GamePacket();
                        GamePacket packet = GamePacket.CreateFromPool();
                        packet.header = header;
                        if (packet.header.dataSize <= 0) {
                            packet.header.dataSize = 0;
                            packet.data = null;
                        } else {
                            // packet.data = new byte[packet.header.dataSize];
							packet.data = NetByteArrayPool.GetByteBufferNode(packet.header.dataSize);
                            var buf = packet.data.GetBuffer();
                            Buffer.BlockCopy(mRecvBuffer, i + headerSize, buf, 0, packet.header.dataSize);
							/*
							 * 优化：后面可以考虑把Proto的序列化生成放到子线程。。。
							*/
                        }

                        // 优化掉
                        // LinkedListNode<GamePacket> node = new LinkedListNode<GamePacket>(packet);
                        var node = packet.LinkedNode;
                        lock (this)
                        {
                            mPacketList.AddLast(node);
                        }

                        i += headerSize + header.dataSize;
                    }
                } finally {
                    // 优化掉了
                 //   Marshal.FreeHGlobal(headerBuffer);
                }

                recvBufSz -= i;
                mRecvSize = recvBufSz;
                if (recvBufSz > 0)
                    Buffer.BlockCopy(mRecvBuffer, i, mRecvBuffer, 0, recvBufSz);
            }
        }

		private void ClearAllProcessPackets()
		{
            if (mPacketList != null) {
                lock (this) {
                    mPacketList.Clear();
                }
            }
		}

		private void ProcessPacket(GamePacket packet)
		{
			packet.DoDone();
			OnPacketRead onRead;
			if (mPacketListenerMap.TryGetValue (packet.header.header, out onRead)) {
				if (onRead != null) {
					try
					{
						onRead(packet);
					} catch
					{
					}
				}
			} else
            {
                try
                {
                    // 再找AbstractServer
                    if (mPacketAbstractServerMessageMap != null)
                    {
                        System.Type abstractMessageClass;
                        if (mPacketAbstractServerMessageMap.TryGetValue(packet.header.header, out abstractMessageClass)
                            && abstractMessageClass != null)
                        {
                            byte[] buffer = null;
                            if (packet.data != null)
                                buffer = packet.data.GetBuffer();
                            var obj = Activator.CreateInstance(abstractMessageClass, buffer, packet.header.dataSize);
                            if (obj != null)
                            {
                                AbstractServerMessage message = obj as AbstractServerMessage;
                                if (message != null)
                                {
                                    try
                                    {
                                        message.DoRecv();
                                    }
                                    finally
                                    {
                                        message.Dispose();
                                    }
                                }
                            }
                        }
                    }
                } catch
                {

                }
            }
		}

		private void ProcessPackets()
		{
            while (true) {
                LinkedListNode<GamePacket> node;
                lock (this) {
                    node = mPacketList.First;
                    if (node != null)
                        mPacketList.RemoveFirst();
                }

                if (node == null)
                    break;
                GamePacket packet = node.Value;
                if (packet != null) {
                    if (packet.status == GamePacketStatus.GPNone) {
                        ProcessPacket(packet);
                        // 如果为标记为正在处理
                        if (packet.status == GamePacketStatus.GPProcessing) {
                            lock (this) {
                                mPacketList.AddFirst(node);
                            }
							break;
                        } else {
                            packet.Dispose();
                        }
                    } else if (packet.status == GamePacketStatus.GPProcessing) {
                        lock (this) {
                            mPacketList.AddFirst(node);
                        }
                        break;
                    } else {
                        packet.Dispose();
                    }
                }
            }
		}

        public void RegisterServerMessageClass(int header, System.Type messageClass)
        {
            if (messageClass == null)
                return;
            if (mPacketAbstractServerMessageMap == null)
            {
                mPacketAbstractServerMessageMap = new Dictionary<int, Type>();
                mPacketAbstractServerMessageMap.Add(header, messageClass);
            }
            else
            {
                if (mPacketAbstractServerMessageMap.ContainsKey(header))
                    throw (new Exception());
                else
                    mPacketAbstractServerMessageMap.Add(header, messageClass);
            }
        }

        public void RemoveServerMessageClass(int header)
        {
            if (mPacketAbstractServerMessageMap == null)
                return;
            if (mPacketAbstractServerMessageMap.ContainsKey(header))
                mPacketAbstractServerMessageMap.Remove(header);
        }

		public void AddPacketListener(int header, OnPacketRead callBack)
		{
			if (mPacketListenerMap.ContainsKey (header)) {
				throw (new Exception());
			} else {
				mPacketListenerMap.Add(header, callBack);
			}
		}

		public void RemovePacketListener(int header)
		{
			if (mPacketListenerMap.ContainsKey (header)) {
				mPacketListenerMap.Remove(header);
			}
		}

        private TcpClient mTcpClient = null;
		private LinkedList<GamePacket> mPacketList = new LinkedList<GamePacket>();
		private Dictionary<int, OnPacketRead> mPacketListenerMap = new Dictionary<int, OnPacketRead>();
        private Dictionary<int, System.Type> mPacketAbstractServerMessageMap = null;
        private byte[] mRecvBuffer = new byte[TcpClient.MAX_TCP_CLIENT_BUFF_SIZE];
		private int mRecvSize = 0;
		private bool mConnecting = false;
		private bool mAbort = false;
		private System.Timers.Timer mTimer = null;
		private ICRC mCrc = new Crc32();
        private event OnSocketStateEvent mStateEvents;
	}
}
