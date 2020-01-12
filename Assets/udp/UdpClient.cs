using System;
using System.Net;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using NsTcpClient;


namespace NsUdpClient
{
    public class KcpClient : IDisposable
    {
        private System.Net.Sockets.UdpClient m_Udp = null;
        private bool m_IsIpv6 = false;
        private Thread m_Thread = null;
        private int m_BindPort = 0;
        // 线程状态
        private ThreadState m_ThreadStatus = ThreadState.Unstarted;
        private object m_Mutex = new object();
        private LinkedList<tReqHead> m_QueueReq = new LinkedList<tReqHead>();
        private bool m_IsDispose = false;
        private Dictionary<int, System.Type> mPacketAbstractServerMessageMap = null;
        private LinkedList<GamePacket> mPacketList = new LinkedList<GamePacket>();
        private ICRC mCrc = new Crc32();
        private Dictionary<int, OnPacketRead> mPacketListenerMap = new Dictionary<int, OnPacketRead>();
        private IPEndPoint m_RecvEndPoint = new IPEndPoint(IPAddress.Any, 0);
        private KCP m_Kcp = null;

        private static readonly DateTime utc_time = new DateTime(1970, 1, 1);
        public static UInt32 iclock()
        {
            return (UInt32)(Convert.ToInt64(DateTime.UtcNow.Subtract(utc_time).TotalMilliseconds) & 0xffffffff);
        }

        public KcpClient(bool isIpv6 = false, int bindPort = 0)
        {
            m_IsIpv6 = isIpv6;
            if (bindPort < 0)
                bindPort = 0;
            m_BindPort = bindPort;
        }

        public int BindPort
        {
            get
            {
                return m_BindPort;
            }
        }

        public bool IsIpv6
        {
            get
            {
                return m_IsIpv6;
            }
        }

        // 更新
        public void Update(float delta)
        {
            // 处理包
            ProcessPackets();
        }

        private void CreateSocket()
        {
            if (m_Udp != null)
                return;
            //CloseThread();

            System.Net.Sockets.AddressFamily family = m_IsIpv6 ? System.Net.Sockets.AddressFamily.InterNetworkV6 : System.Net.Sockets.AddressFamily.InterNetwork;
            m_Udp = new System.Net.Sockets.UdpClient(m_BindPort, family);
            // 创建线程
            CreateThread();
        }

        private bool IsThreadRuning
        {
            get
            {
                return m_Thread != null && m_Thread.ThreadState == ThreadState.Running
#if !_USE_ABORT
 && LocalThreadState == ThreadState.Running
#endif
;
            }
        }

        private tReqHead GetFirstReq()
        {
            tReqHead pReq = null;

            lock (m_Mutex)
            {
                if (m_QueueReq.Count > 0)
                {
                    LinkedListNode<tReqHead> node = m_QueueReq.First;
                    if (node != null)
                        pReq = node.Value;
                }
            }

            return pReq;
        }

        private void RemoteFirstReq()
        {
            lock (m_Mutex)
            {
                m_QueueReq.RemoveFirst();
            }
        }

        private void Execute()
        {
            if (m_Udp == null)
                return;
            
            tReqHead pHead = GetFirstReq();
            if (pHead != null)
            {
                if (pHead.uReqType == eReqType.eREQ_TYPE_SEND)
                {
                    RemoteFirstReq();
                    UdpReqSend sndReq = pHead as UdpReqSend;
                    if (sndReq != null)
                    {
                        DoSend(sndReq);
                    }
                }
            }

            DoRead();
        }

        protected virtual void DoSend(UdpReqSend req)
        {
            if (req == null || m_Udp == null || req.pSendData == null || req.pSendData.Length <= 0 || req.SendSize <= 0 || 
                string.IsNullOrEmpty(req.ip) || req.port <= 0)
                return;
            try
            {
                m_Udp.Send(req.pSendData, req.SendSize, req.ip, req.port);
            }
            catch (Exception e)
            {
#if DEBUG
                UnityEngine.Debug.LogError(e.ToString());
#endif
            }
        }

        protected virtual void DoRead()
        {
            try
            {
                if (m_Udp == null)
                    return;
                byte[] recvBuf = m_Udp.Receive(ref m_RecvEndPoint);
                if (recvBuf == null)
                    return;
                OnThreadBufferProcess(recvBuf, recvBuf.Length);
            }
            catch (Exception e)
            {
#if DEBUG
                UnityEngine.Debug.LogError(e.ToString());
#endif
            }
        }

        private void ThreadProc()
        {
            while (IsThreadRuning)
            {
                Execute();
            }
#if !_USE_ABORT
            LocalThreadState = ThreadState.Aborted;
#endif
        }

        private void CreateThread()
        {
            if (m_Thread != null)
                return;
            
            m_Thread = new Thread(ThreadProc);
#if !_USE_ABORT
            LocalThreadState = ThreadState.Running;
#endif
            m_Thread.Start();
        }

#if !_USE_ABORT
        // 用于模拟abort
        private ThreadState LocalThreadState
        {
            get
            {
                lock (m_Mutex)
                {
                    return m_ThreadStatus;
                }
            }

            set
            {
                lock (m_Mutex)
                {
                    m_ThreadStatus = value;
                }
            }
        }
#endif

        private void CloseThread()
        {
            if (m_Thread != null)
            {
                try
                {
#if _USE_ABORT
                m_Thread.Abort();
                m_Thread.Join();
#else
                    // 模拟abort操作
                    LocalThreadState = ThreadState.AbortRequested;
                    m_Thread.Join();
#endif
                } catch
                { }
                m_Thread = null;
            }
        }

        public bool SendMessage(string ip, int port, int packetHandle, AbstractClientMessage message)
        {
            if (string.IsNullOrEmpty(ip) || port <= 0)
                return false;
            if (message != null)
            {
                try
                {
                    message.DoSend();
                    long bufSize;
                    byte[] buffer = message.GetBuffer(out bufSize);
                    if (bufSize > int.MaxValue)
                        return false;
                    return Send(ip, port, buffer, packetHandle, (int)bufSize);
                }
                finally
                {
                    message.Dispose();
                }
            } else
            {
                return Send(ip, port, null, packetHandle);
            }
        }
        

        public bool Send(string ip, int port, byte[] buf, int packetHandle, int bufSize = -1)
        {
            if (string.IsNullOrEmpty(ip) || port <= 0)
                return false;

            if (bufSize < 0 && buf != null)
                bufSize = buf.Length;

            bool hasBufData = (buf != null) && (bufSize > 0);

            GamePackHeader header = new GamePackHeader();

            if (hasBufData)
                header.dataSize = bufSize;
            else
                header.dataSize = 0;

            header.header = packetHandle;

            if (hasBufData)
            {
                mCrc.Crc(buf, bufSize);
                header.dataCrc32 = (uint)mCrc.Value;
            }
            else
                header.dataCrc32 = 0;

            int headerSize = Marshal.SizeOf(header);
            int dstSize = headerSize;
            if (hasBufData)
                dstSize += bufSize;

            // 下面注释是未优化代码
            //byte[] dstBuffer = new byte[dstSize];
            // 此处已优化
			var dstStream = NetByteArrayPool.GetByteBufferNode(dstSize);
            try
            {
                byte[] dstBuffer = dstStream.GetBuffer();

                // 此处可优化，可以考虑后续使用RINGBUF优化，RINGBUF用完可以自动关闭掉连接
                IntPtr pStruct = Marshal.AllocHGlobal(headerSize);
                try
                {
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
                }
                finally
                {
                    Marshal.FreeHGlobal(pStruct);
                }

#if USE_NETORDER
#else
                // Calc header Crc
                //  CalcHeaderCrc(ref header, dstBuffer);
#endif
                if (hasBufData)
                    Buffer.BlockCopy(buf, 0, dstBuffer, headerSize, bufSize);
                return SendBuf(ip, port, dstBuffer, dstSize);
            }
            finally
            {
                if (dstStream != null)
                {
                    dstStream.Dispose();
                    dstStream = null;
                }
            }

        }

        private bool SendBuf(string ip, int port, byte[] pData, int bufSize = -1)
        {
            if (string.IsNullOrEmpty(ip) || port <= 0 || pData == null || pData.Length <= 0 || bufSize == 0)
                return false;

            CreateSocket();
            if (m_Udp == null)
                return false;

            if (bufSize < 0)
                bufSize = pData.Length;

            AddSendReq(pData, bufSize, ip, port);
            return true;
        }

        private void AddSendReq(byte[] pData, int bufSize, string ip, int port)
        {
            UdpReqSend pReq = new UdpReqSend(pData, bufSize, ip, port);
            lock (m_Mutex)
            {
                LinkedListNode<tReqHead> node = new LinkedListNode<tReqHead>(pReq);
                m_QueueReq.AddLast(node);
            }
        }

        ~KcpClient()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Release()
        {
            Dispose();
        }

        private void FreeSendQueue()
        {
            lock (m_Mutex)
            {
                if (m_QueueReq != null)
                    m_QueueReq.Clear();
            }
        }

        private void CloseSocket()
        {
            if (m_Udp != null)
            {
                try
                {
                    m_Udp.Close();
                }
                catch
                { }
                m_Udp = null;
            }

            // 清空KCP
            m_Kcp = null;

            FreeSendQueue();

            // 清理掉所有处理
            ClearAllProcessPackets();
        }

        protected void Dispose(bool Diposing)
        {
            if (!m_IsDispose)
            {
                CloseThread();

                FreeSendQueue();
                if (Diposing)
                {
                    CloseSocket();

                    m_Thread = null;
                    m_Mutex = null;
                    m_QueueReq = null;
                }
                m_IsDispose = true;
            }
        }

        /*注册消息*/
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

        private void ClearAllProcessPackets()
        {
            if (mPacketList != null)
            {
                lock (this)
                {
                    mPacketList.Clear();
                }
            }
        }

        public void AddPacketListener(int header, OnPacketRead callBack)
        {
            if (mPacketListenerMap.ContainsKey(header))
            {
                throw (new Exception());
            }
            else
            {
                mPacketListenerMap.Add(header, callBack);
            }
        }

        public void RemovePacketListener(int header)
        {
            if (mPacketListenerMap.ContainsKey(header))
            {
                mPacketListenerMap.Remove(header);
            }
        }


        private void ProcessPackets()
        {
            while (true)
            {
                LinkedListNode<GamePacket> node;
                lock (this)
                {
                    node = mPacketList.First;
                    if (node != null)
                        mPacketList.RemoveFirst();
                }

                if (node == null)
                    break;
                GamePacket packet = node.Value;
                if (packet != null)
                {
                    if (packet.status == GamePacketStatus.GPNone)
                    {
                        ProcessPacket(packet);
                        // 如果为标记为正在处理
                        if (packet.status == GamePacketStatus.GPProcessing)
                        {
                            lock (this)
                            {
                                mPacketList.AddFirst(node);
                            }
                            break;
                        } else {
                            packet.Dispose();
                        }
                    }
                    else if (packet.status == GamePacketStatus.GPProcessing)
                    {
                        lock (this)
                        {
                            mPacketList.AddFirst(node);
                        }
                        break;
                    } else {
                        packet.Dispose();
                    }
                }
            }
        }

        private void ProcessPacket(GamePacket packet)
        {
            packet.DoDone();
            OnPacketRead onRead;
            if (mPacketListenerMap.TryGetValue(packet.header.header, out onRead))
            {
                if (onRead != null)
                {
                    try
                    {
                        onRead(packet);
                    }
                    catch
                    {
                    }
                }
            }
            else
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
                }
                catch
                {

                }
            }
        }

        // 处理格式
        protected unsafe virtual void OnThreadBufferProcess(byte[] recvBuffer, int recvBufSz)
        {
            if (recvBuffer == null || recvBuffer.Length <= 0 || recvBufSz <= 0)
                return;
            if (recvBufSz > recvBuffer.Length)
                recvBufSz = recvBuffer.Length;

            GamePackHeader header = new GamePackHeader();
            int headerSize = Marshal.SizeOf(header);
        //    IntPtr headerBuffer = Marshal.AllocHGlobal(headerSize);
            try {
                if (recvBufSz >= headerSize)
                {
                    byte* headerBuffer = (byte*)&header;
                    Marshal.Copy(recvBuffer, 0, (IntPtr)headerBuffer, headerSize);
                  //  header = (GamePackHeader)Marshal.PtrToStructure(headerBuffer, typeof(GamePackHeader));
#if USE_NETORDER
                        // used Net
                        header.headerCrc32 = (uint)IPAddress.NetworkToHostOrder(header.headerCrc32);
                        header.dataCrc32 = (uint)IPAddress.NetworkToHostOrder(header.dataCrc32);
                        header.header = IPAddress.NetworkToHostOrder(header.header);
                        header.dataSize = IPAddress.NetworkToHostOrder(header.dataSize);
#endif
                    if (recvBufSz < (header.dataSize + headerSize))
                        return;

                    GamePacket packet = GamePacket.CreateFromPool();
                    packet.header = header;
                    if (packet.header.dataSize <= 0)
                    {
                        packet.header.dataSize = 0;
                        packet.data = null;
                    }
                    else
                    {
						packet.data = NetByteArrayPool.GetByteBufferNode(packet.header.dataSize);
                        var buf = packet.data.GetBuffer();
                        Buffer.BlockCopy(recvBuffer, headerSize, buf, 0, packet.header.dataSize);
                    }

                    //LinkedListNode<GamePacket> node = new LinkedListNode<GamePacket>(packet);
                    var node = packet.LinkedNode;
                    lock (this)
                    {
                        mPacketList.AddLast(node);
                    }

                } 
            } finally
            {
             //   Marshal.FreeHGlobal(headerBuffer);
            }
        }


        //--------------------------- KCP相关 ----------------------------------------------//

        void init_kcp(UInt32 conv)
        {
            if (m_Kcp != null)
                return;
            m_Kcp = new KCP(conv, (byte[] buf, int size) =>
            {
               // m_UdpClient.SendBuf(buf, size);
            });

            // fast mode.
            m_Kcp.NoDelay(1, 10, 2, 1);
            m_Kcp.WndSize(128, 128);
        }


        //---------------------------------------------------------------------------------//

    }
}
