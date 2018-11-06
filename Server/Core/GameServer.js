/*
游戏逻辑服务器
*/
var NetManager = require("./NetManager");
var RegisterGameMessage = require("./RegisterGameMessage");
var DefaultPacketHandler = require("./DefaultPacketHandler");
var TcpClient = require("./TcpClient");

function GameServer(id, port, dbIp, dbPort)
{
    this.Init();
    this.m_Id = id;
    this.m_Port = port;
    this.m_DBIp = dbIp;
    this.m_DBPort = dbPort;
    // 只有当所有都连接好了，才允许其他人进去
    this.m_Enabled = false;
    // 是否是可见的
    this.m_Visible = true;
    // 连接DB的
    this.m_DBClient = null;
}

GameServer.prototype = NetManager.prototype;
GameServer.prototype.constructor = GameServer;

var superConnectedEvent = GameServer.prototype.OnConnectedEvent;
GameServer.prototype.OnConnectedEvent =
    function (socket)
    {
        if (!this.m_Enabled)
        {
            if (socket != null)
            {
                socket.destroy();
                socket = null;
            }
            return;
        }

        if (superConnectedEvent != null)
            superConnectedEvent(socket);   
    }

// 是否是可以的
GameServer.prototype.IsEnabled = 
    function ()
    {
        return this.m_Enabled;
    }

// 是否是可见的
GameServer.prototype.IsVisible =
    function ()
    {
        return this.m_Visible;
    }

// 连接需要连接的服务器
GameServer.prototype.ConnectDB =
    function (callBack)
    {
        if (this.m_DBClient != null)
        {
            this.m_DBClient.DisConnect();
        }

        if (this.m_DBIp == null || this.m_DBPort == null)
            return false;
        
        this.m_DBClient = new TcpClient(DefaultPacketHandler, this);

        var ret = this.m_DBClient.ConnectServer(this.m_DBIp, this.m_DBPort, 0);
        return ret;
    }

    GameServer.prototype.OnAbortEvent =
        function (tcpClient)
        {
            if (this.m_DBClient == tcpClient)
            {
                console.log("\n DB服务器异常断开");

                this.m_Enabled = false;
                this.KickAllGates();
                // 重新连接DB
                this.ConnectDB();
            }
        }

    GameServer.prototype.OnConnectEvent =
        function (sucess, tcpClient)
        {
            if (this.m_DBClient == tcpClient)
            {
                this.m_Enabled = sucess;
                if (sucess)
                {
                    console.error("\n DB服务器连接成功");
                } else
                {
                    console.error("\n DB服务器连接失败");
                    this.KickAllGates();
                }
            }
        }

// 踢掉所有Gate连接
GameServer.prototype.KickAllGates =
        function ()
        {
            this.CloseAllClientSocket();
        }

GameServer.Create = function (id, port, dbIp, dbPort)
{
    if (id == null || port == null)
        return null;

    var server = new GameServer(id, port, dbIp, dbPort);
    server.SetPacketHandlerClass(DefaultPacketHandler);

    // 注冊消息
    new RegisterGameMessage(this);

    if (!server.ConnectDB())
    {
        console.error("\n DB服务器连接失败");
    }

    server.Listen(port);
    return server;
}

module.exports = GameServer;