var TcpServer = require ("./TcpServer");
var UserSession = require("./UserSession");
var AbstractPacketHandler = require ("./AbstractPacketHandler");
var ITcpServerListener = require("./ITcpServerListener");

function NetManager() {
    this.m_TcpServer = null;
    this.m_SessionMap = null;
    this.m_PacketHandlerClass = null;
    this.m_ServerListener = null;
}

// 继承
NetManager.prototype = ITcpServerListener.prototype;
NetManager.prototype.constructor = NetManager;

NetManager.GetInstance =
    function()
    {
        if (NetManager.m_Instance == null)
        {
            NetManager.m_Instance = new NetManager();
        }
        return NetManager.m_Instance;
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
                serverMsgListener.OnMessage.call(serverMsgListener, packet, clientSocket)
        }
    }

NetManager.prototype.RegisterServerMessage = 
    function (headerId, serverMsgListener)
    {
        if (serverMsgListener == null)
            return;
        if (this.m_ServerListener == null)
            this.m_ServerListener = {};
        this.m_ServerListener[headerId] = serverMsgListener;
    }

NetManager.prototype.Listen =
    function(bindPort)
    {
        if (bindPort == null)
            return false;

        this.Close();
        this.m_TcpServer = new TcpServer(bindPort);

        this.m_TcpServer.SetListener.call(this.m_TcpServer, this);

        return this.m_TcpServer.Accept();
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
        var newHandler = new handlerClass();
        this.m_SessionMap[clientSocket] = new UserSession(clientSocket, newHandler);
    }

module.exports = NetManager;