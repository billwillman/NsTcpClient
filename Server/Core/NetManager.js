var TcpServer = require ("./TcpServer");
var UserSession = require("./UserSession");
var AbstractPacketHandler = require ("./AbstractPacketHandler");
var ITcpServerListener = require("./ITcpServerListener");
var AbstractMessageMgr = require("./AbstractMessageMgr");

class NetManager extends ITcpServerListener
{
    constructor()
    {
        super();
        this.Init();
    }

    Init()
    {
        this.m_TcpServer = null;
        this.m_SessionMap = null;
        this.m_PacketHandlerClass = null;
        this.m_ServerListener = null;
        this.m_DefaultServerMsgListener = null;
    }

    RegisterDefaultServerMsgListener(listener)
    {
        if (listener == null)
            listener = new AbstractMessageMgr();
        this.m_DefaultServerMsgListener = listener;
    }

    RegisterDefaultSrvAbstractMsg(headerId, abstractMsg)
    {
        if (headerId == null || abstractMsg == null)
            return;
        if (this.m_DefaultServerMsgListener == null)
            this.m_DefaultServerMsgListener = new AbstractMessageMgr();
        this.m_DefaultServerMsgListener.RegisterSrvMsg.call(this.m_DefaultServerMsgListener, headerId, abstractMsg);
    }

    _OnDefaultMessageHandle(packet, clientSocket)
    {
        if (this.m_DefaultServerMsgListener == null || this.m_DefaultServerMsgListener.OnMessage == null)
            return;
        this.m_DefaultServerMsgListener.OnMessage.call(this.m_DefaultServerMsgListener, packet, clientSocket, this);
    }

    _SendPacketRead(packet, clientSocket)
    {
        if (packet == null)
            return;

        // 根据协议号分发
        if (this.m_ServerListener != null && packet.header != null)
        {
            var headerId = packet.header.header;
            var serverMsgListener = this.m_ServerListener[headerId];
            if (serverMsgListener != null && serverMsgListener.OnMessage != null)
            {
                serverMsgListener.OnMessage.call(serverMsgListener, packet, clientSocket, this);
                return;
            }
        }

        this._OnDefaultMessageHandle(packet, clientSocket);
    }

    RegisterServerMessage(headerId, serverMsgListener)
    {
        if (headerId == null || serverMsgListener == null)
            return;
        if (this.m_ServerListener == null)
            this.m_ServerListener = {};
        this.m_ServerListener[headerId] = serverMsgListener;
    }

    // heartTimeout心跳包
    Listen(bindPort, heartTimeout)
    {
        if (bindPort == null)
            return false;

        this.Close();
        this.m_TcpServer = new TcpServer(bindPort);

        this.m_TcpServer.SetListener.call(this.m_TcpServer, this);

        return this.m_TcpServer.Accept(heartTimeout);
    }

    Close()
    {   
        if (this.m_TcpServer != null)
        {
            this.m_TcpServer.Close();
            this.m_TcpServer = null;
        }

        this.m_SessionMap = null;
    }

    // 关闭所有客户端， 但服务器不停
    CloseAllClientSocket()
    {
        if (this.m_SessionMap != null)
        {
            for (clientSocket in this.m_SessionMap)
            {
                this.CloseClientSocket(clientSocket);
            }
        }
        this.m_SessionMap = null;
    }

    OnTimeOut(clientSocket)
    {
        this.CloseClientSocket(clientSocket);
    }

    // 主动断开客户端连接
    CloseClientSocket(clientSocket)
    {
        this._RemoveSession(clientSocket);
    }

    SetPacketHandlerClass(handleClass)
    {
        this.m_PacketHandlerClass = handleClass;
    }

    _RemoveSession(clientSocket)
    {
        if (clientSocket == null)
            return;
        if (this.m_SessionMap != null)
        {
            var userSession = this.m_SessionMap[clientSocket];
            if (userSession != null)
            {
                this.OnRemoveSessionEvent(userSession);
                userSession.Close();
                this.m_SessionMap[clientSocket] = null;
                delete this.m_SessionMap[clientSocket];
            }
        }
    }

    OnRemoveSessionEvent(session)
    {}

    OnAddSessionEvent(session)
    {}

    OnPacketRead(data, socket)
    {
        if (data == null || socket == null)
            return;
        if (this.m_SessionMap == null)
            return;
        var userSession = this.m_SessionMap[socket];
        if (userSession == null)
            return;
        userSession.HandleMessage.call(userSession, data);
    }

    _GetPacketHandlerClass()
    {
        if (this.m_PacketHandlerClass == null)
            this.m_PacketHandlerClass = AbstractPacketHandler;
        return this.m_PacketHandlerClass;
    }

    OnEndEvent(clientSocket)
    {
        if (clientSocket == null)
                return;
        this._RemoveSession(clientSocket);
    }

    OnConnectedEvent(clientSocket)
    {
        if (clientSocket == null)
            return;
        this._RemoveSession(clientSocket);
        if (this.m_SessionMap == null)
            this.m_SessionMap = {};
        var handlerClass = this._GetPacketHandlerClass();
        var newHandler = new handlerClass(this);
        var session = new UserSession(clientSocket, newHandler);
        this.m_SessionMap[clientSocket] = session;
        this.OnAddSessionEvent(session);
    }

    // 发送给客户端
    SendBuf(clientSocket, packetHandle, buf, args)
    {
        if (clientSocket == null || packetHandle == null)
            return false;
        if (this.m_SessionMap == null)
            return false;
        var session = this.m_SessionMap[clientSocket];
        if (session == null)
            return false;
        return session.SendBuf.call(session, packetHandle, buf, args);
    }

    SendMessage(packetHandle, message, args, targetSocket)
    {
        if (targetSocket == null || packetHandle == null)
            return false;
        var buf = null;
        if (message != null)
        {
            message.DoSend();
            buf = message.m_Buf;
        }
        return this.SendBuf(targetSocket, packetHandle, buf, args);
    }
}

module.exports = NetManager;