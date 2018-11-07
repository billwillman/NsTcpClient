var TcpServer = require ("./TcpServer");
var UserSession = require("./UserSession");
var AbstractPacketHandler = require ("./AbstractPacketHandler");
var ITcpServerListener = require("./ITcpServerListener");
var AbstractMessageMgr = require("./AbstractMessageMgr");

function NetManager() {
    this.Init();
}

// 继承
NetManager.prototype = ITcpServerListener.prototype;
NetManager.prototype.constructor = NetManager;

NetManager.prototype.Init =
    function ()
    {
        this.m_TcpServer = null;
        this.m_SessionMap = null;
        this.m_PacketHandlerClass = null;
        this.m_ServerListener = null;
        this.m_DefaultServerMsgListener = null;
    }

NetManager.prototype.RegisterDefaultServerMsgListener =
    function (listener)
    {
        if (listener == null)
            listener = new AbstractMessageMgr();
        this.m_DefaultServerMsgListener = listener;
    }

NetManager.prototype.RegisterDefaultSrvAbstractMsg =
    function (headerId, abstractMsg)
    {
        if (headerId == null || abstractMsg == null)
            return;
        if (this.m_DefaultServerMsgListener == null)
            this.m_DefaultServerMsgListener = new AbstractMessageMgr();
        this.m_DefaultServerMsgListener.RegisterSrvMsg.call(this.m_DefaultServerMsgListener, headerId, abstractMsg);
    }
    
NetManager.prototype._OnDefaultMessageHandle =
    function (packet, clientSocket)
    {
        if (this.m_DefaultServerMsgListener == null || this.m_DefaultServerMsgListener.OnMessage == null)
            return;
        this.m_DefaultServerMsgListener.OnMessage.call(this.m_DefaultServerMsgListener, packet, clientSocket, this);
    }

NetManager.prototype._SendPacketRead =
    function (packet, clientSocket)
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

NetManager.prototype.RegisterServerMessage = 
    function (headerId, serverMsgListener)
    {
        if (headerId == null || serverMsgListener == null)
            return;
        if (this.m_ServerListener == null)
            this.m_ServerListener = {};
        this.m_ServerListener[headerId] = serverMsgListener;
    }

// heartTimeout心跳包
NetManager.prototype.Listen =
    function(bindPort, heartTimeout)
    {
        if (bindPort == null)
            return false;

        this.Close();
        this.m_TcpServer = new TcpServer(bindPort);

        this.m_TcpServer.SetListener.call(this.m_TcpServer, this);

        return this.m_TcpServer.Accept(heartTimeout);
    }

NetManager.prototype.Close =
    function ()
    {
        if (this.m_TcpServer != null)
        {
            this.m_TcpServer.Close();
            this.m_TcpServer = null;
        }

        this.m_SessionMap = null;
    }

// 关闭所有客户端， 但服务器不停
NetManager.prototype.CloseAllClientSocket =
    function ()
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

NetManager.prototype.OnTimeOut =
    function (clientSocket)
    {
        this.CloseClientSocket(clientSocket);
    }

    // 主动断开客户端连接
NetManager.prototype.CloseClientSocket = 
    function (clientSocket)
    {
        this._RemoveSession(clientSocket);
    }

NetManager.prototype.SetPacketHandlerClass =
    function (handleClass) {
        this.m_PacketHandlerClass = handleClass;
    }

NetManager.prototype._RemoveSession =
    function(clientSocket)
    {
        if (clientSocket == null)
            return;
        if (this.m_SessionMap != null)
        {
            var userSession = this.m_SessionMap[clientSocket];
            if (userSession != null)
            {
                userSession.Close();
                this.m_SessionMap[clientSocket] = null;
                delete this.m_SessionMap[clientSocket];
            }
        }
    }

NetManager.prototype.OnPacketRead =
    function (data, socket)
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

NetManager.prototype._GetPacketHandlerClass =
    function ()
    {
        if (this.m_PacketHandlerClass == null)
            this.m_PacketHandlerClass = AbstractPacketHandler;
        return this.m_PacketHandlerClass;
    }

    NetManager.prototype.OnEndEvent =
        function (clientSocket)
        {
            if (clientSocket == null)
                return;
            this._RemoveSession(clientSocket);
        }


NetManager.prototype.OnConnectedEvent =
    function (clientSocket)
    {
        if (clientSocket == null)
            return;
        this._RemoveSession(clientSocket);
        if (this.m_SessionMap == null)
            this.m_SessionMap = {};
        var handlerClass = this._GetPacketHandlerClass();
        var newHandler = new handlerClass(this);
        this.m_SessionMap[clientSocket] = new UserSession(clientSocket, newHandler);
    }

// 发送给客户端
NetManager.prototype.SendBuf = 
    function (clientSocket, packetHandle, buf)
    {
        if (clientSocket == null || packetHandle == null)
            return false;
        if (this.m_SessionMap == null)
            return false;
        var session = this.m_SessionMap[clientSocket];
        if (session == null)
            return false;
        return session.SendBuf.call(session, packetHandle, buf);
    }

NetManager.prototype.SendMessage =
    function (clientSocket, packetHandle, message)
    {
        if (clientSocket == null || packetHandle == null)
            return false;
        var buf = null;
        if (message != null)
        {
            message.DoSend();
            buf = message.m_Buf;
        }
        return this.SendBuf(clientSocket, packetHandle, buf);
    }

module.exports = NetManager;