using System;
using System.Net;
using System.Threading;
using System.Collections.Generic;
using NsTcpClient;

namespace NsUdpClient
{
    public class UdpClient : IDisposable
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

        public UdpClient(bool isIpv6 = false, int bindPort = 0)
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

        private void CreateSocket()
        {
            if (m_Udp != null)
                return;
            CloseThread();

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

        private void Execute()
        {
           
        }

        private void DoSend()
        {
            
        }

        private void DoRead()
        {

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

        public bool Send(string ip, int port, byte[] pData, int bufSize = -1)
        {
            if (string.IsNullOrEmpty(ip) || port <= 0 || pData == null || pData.Length <= 0 || bufSize == 0)
                return false;

            CreateSocket();
            if (m_Udp == null)
                return false;

            if (bufSize < 0)
                bufSize = pData.Length;
            AddSendReq(pData, bufSize);
            return true;
        }

        private void AddSendReq(byte[] pData, int bufSize)
        {
            tReqSend pReq = new tReqSend(pData, bufSize);
            lock (m_Mutex)
            {
                LinkedListNode<tReqHead> node = new LinkedListNode<tReqHead>(pReq);
                m_QueueReq.AddLast(node);
            }
        }

        ~UdpClient()
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

        protected void Dispose(bool Diposing)
        {
            if (!m_IsDispose)
            {
                CloseThread();

                FreeSendQueue();
                if (Diposing)
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

                    m_Thread = null;
                    m_Mutex = null;
                    m_QueueReq = null;
                }
                m_IsDispose = true;
            }
        }
    }
}
