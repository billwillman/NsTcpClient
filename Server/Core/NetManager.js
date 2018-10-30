var TcpServer = require ("./TcpServer");
var UserSession = require("./UserSession");
var AbstractPacketHandler = require ("./AbstractPacketHandler");
var ITcpServerListener = require("./ITcpServerListener");

function NetManager() {
    this.m_TcpServer = null;
    this.m_SessionMap = null;
    this.m_PacketHandler = null;
    this.m_OnPacketRead = null;
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

NetManager.prototype.SendPacketRead =
    function (packet)
    {
        if (packet == null)
            return;
        if (this.m_OnPacketRead == null)
            return;
        this.m_OnPacketRead(packet);
    }

NetManager.prototype.SetPacketReadEvent =
    function (evt)
    {
        this.m_OnPacketRead = evt;
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

NetManager.prototype.SetPacketHandler =
    function (handle) {
        this.m_PacketHandler = handle;
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
    function (data)
    {
        if (data == null)
            return;
        if (this.m_PacketHandler != null)
            this.m_PacketHandler.OnPacketRead.call(this.m_PacketHandler, data);
    }

NetManager.prototype._GetPacketHandler =
    function ()
    {
        if (this.m_PacketHandler == null)
            this.m_PacketHandler = new AbstractPacketHandler();
        return this.m_PacketHandler;
    }

NetManager.prototype.SendStr =
    function (str)
    {
        if (str == null)
            return false;
        var handle = this._GetPacketHandler();
        var buffer = Buffer.from(str, "utf8");
        handle.SendBuffer(buffer);

        return true;
    }

    NetManager.prototype.OnSocketEndEvent =
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
        this.m_SessionMap[clientSocket] = new UserSession(clientSocket);
    }

module.exports = NetManager;