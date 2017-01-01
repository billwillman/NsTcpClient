// ------------------------------------------------------------------------------
// NsTcpClient tcp Client Library
//      by zengyi
// ------------------------------------------------------------------------------

// IOS not use Thread.abort
//#define _USE_ABORT

using System;
using System.Collections;
using System.Collections.Generic;

using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace NsTcpClient {

    public class TcpClient : ITcpClient, IDisposable {
        static public readonly int MAX_TCP_CLIENT_BUFF_SIZE = (64 * 1024);

        // public function
        public TcpClient() {
            m_Thread = new Thread(ThreadProc);
            // Thread start run
            m_Thread.Start();
            #if !_USE_ABORT
            LocalThreadState = m_Thread.ThreadState;
            #endif
            //UnityEngine.Debug.LogFormat ("Thread Status: {0:D}", (int)m_ThreadStatus);
        }

        ~TcpClient() {
            Dispose(false);
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Release() {
            Dispose();
        }

        protected void Dispose(bool Diposing) {
            if (!m_IsDispose) {
                // 优先Abort
                #if _USE_ABORT
                if (m_Thread != null) {
                    m_Thread.Abort();
                    m_Thread.Join();
                }
                #else
                // 模拟abort操作

                LocalThreadState = ThreadState.AbortRequested;
                m_Thread.Join();

                #endif

                if (m_Waiting != null) {
                    m_Waiting.Set();
                }

                FreeSendQueue();

                if (Diposing) {

                    // 释放托管对象资源

                    CloseSocket();

                    m_WaitSendSize = 0;
                    m_HasReadSize = 0;
                    m_Waiting = null;
                    m_Thread = null;
                    m_Mutex = null;
                    m_SendBuffer = null;
                    m_ReadBuffer = null;
                    m_QueueReq = null;
                }

                // 释放非托管对象资源

                m_IsDispose = true;
            }
        }

        public bool Connect(string pRemoteIp, int uRemotePort, int mTimeOut = -1) {
            eClientState state = GetState();
            if ((state == eClientState.eClient_STATE_CONNECTING) || (state == eClientState.eClient_STATE_CONNECTED))
                return false;

            AddConnectReq(pRemoteIp, uRemotePort, mTimeOut);
            return true;
        }

        public eClientState GetState() {
            lock (m_Mutex) {
                return m_State;
            }
        }

        public bool Send(byte[] pData) {
            if ((pData == null) || (pData.Length <= 0))
                return false;

            eClientState state = GetState();
            if (state != eClientState.eClient_STATE_CONNECTED)
                return false;

            AddSendReq(pData);
            return true;
        }

        public bool HasReadData() {
            lock (m_Mutex) {
                return (m_HasReadSize > 0);
            }
        }

        public int GetReadData(byte[] pBuffer, int offset) {
            if (pBuffer == null)
                return 0;

            int bufSize = pBuffer.Length;
            if (bufSize <= 0)
                return 0;

            lock (m_Mutex) {
                int ret = bufSize - offset;

                if (ret <= 0) {
                    // mei you chu li wan
                    ret = 0;
                    return ret;
                }

                if (ret > m_HasReadSize)
                    ret = m_HasReadSize;

                Buffer.BlockCopy(m_ReadBuffer, 0, pBuffer, offset, ret);
                int uLast = m_HasReadSize - ret;

                Buffer.BlockCopy(m_ReadBuffer, ret, m_ReadBuffer, 0, uLast);
                m_HasReadSize = uLast;

                return ret;
            }
        }

        // private function
        private void SetClientState(eClientState uState) {
            lock (m_Mutex) {
                m_State = uState;
            }
        }

        private void AddConnectReq(string pRemoteIp, int uRemotePort, int mTimeOut) {
            tReqConnect pReq = new tReqConnect(pRemoteIp, uRemotePort, mTimeOut);
            lock (m_Mutex) {
                m_State = eClientState.eClient_STATE_CONNECTING;
                LinkedListNode<tReqHead> node = new LinkedListNode<tReqHead>(pReq);
                m_QueueReq.AddLast(node);
            }
        }

        private void AddSendReq(byte[] pData) {
            tReqSend pReq = new tReqSend(pData);
            lock (m_Mutex) {
                LinkedListNode<tReqHead> node = new LinkedListNode<tReqHead>(pReq);
                m_QueueReq.AddLast(node);
            }
        }

        private tReqHead GetFirstReq() {
            tReqHead pReq = null;

            lock (m_Mutex) {
                if (m_QueueReq.Count > 0) {
                    LinkedListNode<tReqHead> node = m_QueueReq.First;
                    if (node != null)
                        pReq = node.Value;
                }
            }

            return pReq;
        }

        private void RemoteFirstReq() {
            lock (m_Mutex) {
                m_QueueReq.RemoveFirst();
            }
        }

        private void HandleConnect(tReqHead pReq) {
            if (pReq == null)
                return;
            tReqConnect pConnect = (tReqConnect)pReq;

            m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            if (m_Socket == null) {
                // not used
                SetClientState(eClientState.eClient_STATE_CONNECT_FAIL);
                return;
            }

            // 使用非延迟发送
            m_Socket.NoDelay = true;

            m_Socket.SendTimeout = 0;
            m_Socket.ReceiveTimeout = 0;

            bool non = pConnect.timeOut >= 0;

            if (non) {
                // set waiting = true
                m_Waiting.Reset();

                AsyncCallback callBack = new AsyncCallback(OnConnectCallBack);
                m_Socket.BeginConnect(pConnect.szRemoteIp, pConnect.uRemotePort, callBack, m_Socket);


                if (m_Waiting.WaitOne(pConnect.timeOut)) {
                    if (m_Socket.Connected && m_Socket.Poll(0, SelectMode.SelectWrite)) {
                        SetClientState(eClientState.eClient_STATE_CONNECTED);
                    }
                    else {
                        CloseSocket();
                        SetClientState(eClientState.eClient_STATE_CONNECT_FAIL);
                    }
                }
                else {
                    if (m_Socket.Connected && m_Socket.Poll(0, SelectMode.SelectWrite)) {
                        SetClientState(eClientState.eClient_STATE_CONNECTED);
                    }
                    else {
                        CloseSocket();
                        SetClientState(eClientState.eClient_STATE_CONNECT_FAIL);
                    }
                }
            }
            else {
                // yi zhi waiting...
                try {
                    m_Socket.Connect(pConnect.szRemoteIp, pConnect.uRemotePort);
                    if (m_Socket.Connected && m_Socket.Poll(0, SelectMode.SelectWrite)) {
                        SetClientState(eClientState.eClient_STATE_CONNECTED);
                    }
                    else {
                        CloseSocket();
                        SetClientState(eClientState.eClient_STATE_CONNECT_FAIL);
                    }
                }
                catch (Exception e) {
                    ProcessException(e, eClientState.eClient_STATE_CONNECT_FAIL);
                }
            }

        }

        private void ProcessException(Exception e, eClientState state) {
            if (e == null)
                return;

            CloseSocket();

            if (!(e is ThreadAbortException)) {
                SetClientState(state);
            }
        }

        private void OnConnectCallBack(IAsyncResult result) {
            try {
                Socket socket = (Socket)result.AsyncState;
                if (socket != null) {
                    if (socket.Connected && socket.Poll(0, SelectMode.SelectWrite))
                        socket.EndConnect(result);
                }

            }
            finally {
                if (m_Waiting != null)
                    m_Waiting.Set();
            }

        }

        private bool HandleSendReq(tReqHead pReq) {
            if (pReq == null)
                return false;

            tReqSend pSend = (tReqSend)pReq;

            if (pSend.pSendData == null)
                return true;
            if (pSend.pSendData.Length <= 0)
                return true;

            int uFreeSize = m_SendBuffer.Length - m_WaitSendSize;
            int uSendSize = pSend.pSendData.Length;
            if (uFreeSize > uSendSize) {
                Buffer.BlockCopy(pSend.pSendData, 0, m_SendBuffer, m_WaitSendSize, uSendSize);
                m_WaitSendSize += uSendSize;
                return true;
            }

            return false;
        }

        private void DoSend() {
            if (m_State != eClientState.eClient_STATE_CONNECTED)
                return;

            if (m_WaitSendSize > 0) {
                try {
                    int nRet = m_Socket.Send(m_SendBuffer, m_WaitSendSize, SocketFlags.None);
                    if (nRet < 0) {
                        CloseSocket();
                        SetClientState(eClientState.eClient_STATE_ABORT);
                    }
                    else {
                        lock (m_Mutex) {
                            m_WaitSendSize -= nRet;
                            if (m_WaitSendSize > 0) {
                                Buffer.BlockCopy(m_SendBuffer, nRet, m_SendBuffer, 0, m_WaitSendSize);
                            }
                        }
                    }
                }
                catch (Exception e) {
                    ProcessException(e, eClientState.eClient_STATE_ABORT);
                }
            }
        }

        private void DoRead() {
            if (m_State != eClientState.eClient_STATE_CONNECTED) {
                return;
            }

            try {
                // 读取数据
                if (m_Socket.Poll(0, SelectMode.SelectRead)) {
                    lock (m_Mutex) {
                        int nRet = m_Socket.Receive(m_ReadBuffer, m_HasReadSize, m_ReadBuffer.Length - m_HasReadSize, SocketFlags.None);
                        if (nRet <= 0) {
                            CloseSocket();
                            m_State = eClientState.eClient_STATE_ABORT;
                        }
                        else {
                            m_HasReadSize += nRet;
                        }
                    }
                }
            }
            catch (Exception e) {
                ProcessException(e, eClientState.eClient_STATE_ABORT);
            }
        }

        private void FreeSendQueue() {
            lock (m_Mutex) {
                if (m_QueueReq != null)
                    m_QueueReq.Clear();
            }
        }

        #if !_USE_ABORT
        // 用于模拟abort
        private ThreadState LocalThreadState {
            get {
                lock (m_Mutex) {
                    return m_ThreadStatus;
                }
            }

            set {
                lock (m_Mutex) {
                    m_ThreadStatus = value;
                }
            }
        }
        #endif

        // 线程是否在运行状态
        private bool IsThreadRuning {
            get {
                return m_Thread != null && m_Thread.ThreadState == ThreadState.Running 
                    #if !_USE_ABORT
                    && LocalThreadState == ThreadState.Running 
                    #endif
                    ;
            }
        }

        // Thread Runing
        private void ThreadProc() {
            while (IsThreadRuning) {
                Execute();
            }

            #if !_USE_ABORT
            LocalThreadState = ThreadState.Aborted;
            #endif
        }

        private void Execute() {
            // 没有在连接状态
            try {
                if ((m_State != eClientState.eClient_STATE_CONNECTED) && (m_State != eClientState.eClient_STATE_CONNECTING)) {
                    Thread.Sleep(1);
                    return;
                }

                tReqHead pHead = GetFirstReq();
                if (pHead != null) {
                    if (pHead.uReqType == eReqType.eREQ_TYPE_CONNECT) {
                        HandleConnect(pHead);
                        RemoteFirstReq();
                    }
                    else
                    if (pHead.uReqType == eReqType.eREQ_TYPE_SEND) {
                        if (HandleSendReq(pHead))
                            RemoteFirstReq();
                    }
                }

                DoSend();
                DoRead();

                Thread.Sleep(1);
            } catch (ThreadAbortException ex) {
                // 不做处理
            }
        }

        private void CloseSocket() {
            if (m_Socket == null)
                return;
            if (m_Socket.Connected)
                m_Socket.Shutdown(SocketShutdown.Both);
            m_Socket.Close();
            m_Socket = null;
        }

        // private
        private object m_Mutex = new object();
        private Socket m_Socket = null;
        private eClientState m_State = eClientState.eCLIENT_STATE_NONE;
        private int m_WaitSendSize = 0;
        private int m_HasReadSize = 0;
        private ManualResetEvent m_Waiting = new ManualResetEvent(false);
        private Thread m_Thread = null;
        // 线程状态
        private ThreadState m_ThreadStatus = ThreadState.Unstarted;

        private byte[] m_SendBuffer = new byte[MAX_TCP_CLIENT_BUFF_SIZE];
        private byte[] m_ReadBuffer = new byte[MAX_TCP_CLIENT_BUFF_SIZE];
        private LinkedList<tReqHead> m_QueueReq = new LinkedList<tReqHead>();
        private bool m_IsDispose = false;
    }

}