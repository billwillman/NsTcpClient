//#define USE_NETORDER
#define USE_PROTOBUF_NET

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Runtime.CompilerServices;
#if USE_NETORDER
using System.Net;
#endif

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
	public class GamePacket
	{
		public GamePackHeader header;
		public byte[] data = null;

		public bool hasData()
		{
			if ((header.dataSize <= 0) || (data == null))
				return false;
			return (data.Length > 0);
		}

		public string dataToString()
		{
			if (data == null)
				return "";
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
            T ret = ProtoMessageMgr.GetInstance().Parser<T>(data) as T;
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

		private void CalcHeaderCrc(ref GamePackHeader header, byte[] dst) {
			int headerCrcSize = Marshal.SizeOf (header.headerCrc32);
			int sz = Marshal.SizeOf (header) - headerCrcSize;
			mCrc.Crc (dst, headerCrcSize, sz);
			header.headerCrc32 = (uint)mCrc.Value;
			byte[] crc = BitConverter.GetBytes (header.headerCrc32);
			Buffer.BlockCopy (crc, 0, dst, 0, crc.Length);
		}

#if USE_PROTOBUF_NET
		// 发送ProtoBuf T来自ProtoBuf类申明
		public void SendProtoBuf<T>(T data, int packetHandle) where T: class, Google.Protobuf.IMessage<T>
		{
			if (data == null) {
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

            // 优化后版本使用byte[]池
            int outSize;
            var stream = ProtoMessageMgr.GetInstance().ToStream<T>(data, out outSize);
            if (stream == null)
                return;
            try {
                if (outSize <= 0)
                    return;
                var buf = stream.GetBuffer();
                Send(buf, packetHandle, outSize);
            }finally {
                if (stream != null) {
                    stream.Dispose();
                    stream = null;
                }
            }
		}
#endif

		// 支持发送buf为null
		public void Send (byte[] buf, int packetHandle, int bufSize = -1)
		{
			if (mConnecting || (mTcpClient == null))
				return;

			if (mTcpClient.GetState () != eClientState.eClient_STATE_CONNECTED)
				return;

            if (bufSize < 0)
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
            var dstStream = NetByteArrayPool.GetBuffer(dstSize);
            try {
                byte[] dstBuffer = dstStream.GetBuffer();

                // 此处可优化，可以考虑后续使用RINGBUF优化，RINGBUF用完可以自动关闭掉连接
                IntPtr pStruct = Marshal.AllocHGlobal(headerSize);
                try {
                    Marshal.StructureToPtr(header, pStruct, false);
                    Marshal.Copy(pStruct, dstBuffer, 0, headerSize);

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
                    Marshal.FreeHGlobal(pStruct);
                }

#if USE_NETORDER
#else
                // Calc header Crc
                CalcHeaderCrc(ref header, dstBuffer);
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
        private void OnThreadBufferProcess(TcpClient tcp)
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
                IntPtr headerBuffer = Marshal.AllocHGlobal(headerSize);
                try {
                    while (recvBufSz - i >= headerSize) {
                        Marshal.Copy(mRecvBuffer, i, headerBuffer, headerSize);
                        header = (GamePackHeader)Marshal.PtrToStructure(headerBuffer, typeof(GamePackHeader));
                        #if USE_NETORDER
                        // used Net
                        header.headerCrc32 = (uint)IPAddress.NetworkToHostOrder(header.headerCrc32);
                        header.dataCrc32 = (uint)IPAddress.NetworkToHostOrder(header.dataCrc32);
                        header.header = IPAddress.NetworkToHostOrder(header.header);
                        header.dataSize = IPAddress.NetworkToHostOrder(header.dataSize);
                        #endif
                        if ((recvBufSz - i) < (header.dataSize + headerSize))
                            break;
                        GamePacket packet = new GamePacket();
                        packet.header = header;
                        if (packet.header.dataSize <= 0) {
                            packet.header.dataSize = 0;
                            packet.data = null;
                        } else {
                            packet.data = new byte[packet.header.dataSize];
                            Buffer.BlockCopy(mRecvBuffer, i + headerSize, packet.data, 0, packet.header.dataSize);
                        }

                        LinkedListNode<GamePacket> node = new LinkedListNode<GamePacket>(packet);
                        lock (this)
                        {
                            mPacketList.AddLast(node);
                        }

                        i += headerSize + header.dataSize;
                    }
                } finally {
                    Marshal.FreeHGlobal(headerBuffer);
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
			}
		}

		private void ProcessPackets()
		{
            while (true) {
                LinkedListNode<GamePacket> node;
                lock (this) {
                    node = mPacketList.First;
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
                        }
                    } else if (packet.status == GamePacketStatus.GPProcessing) {
                        lock (this) {
                            mPacketList.AddFirst(node);
                        }
                        break;
                    }
                }
            }
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
		private byte[] mRecvBuffer = new byte[TcpClient.MAX_TCP_CLIENT_BUFF_SIZE];
		private int mRecvSize = 0;
		private bool mConnecting = false;
		private bool mAbort = false;
		private System.Timers.Timer mTimer = null;
		private ICRC mCrc = new Crc32();
        private event OnSocketStateEvent mStateEvents;
	}
}
