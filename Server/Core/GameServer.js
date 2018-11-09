/*
游戏逻辑服务器
*/
var NetManager = require("./NetManager");
var RegisterGameMessage = require("./RegisterGameMessage");
var DefaultPacketHandler = require("./DefaultPacketHandler");
var TcpClient = require("./TcpClient");
var MessageConsts = require("./MessageConsts");
var GS_DB_Command = require("./Messages/Server/GS_DB_Command");

class GameServer extends NetManager
{
    constructor(id, port, dbIp, dbPort)
    {
        super();
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

    OnConnectedEvent(socket)
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

        if (super.OnConnectedEvent != null)
            super.OnConnectedEvent(socket);
    }

    // 是否是可以的
    IsEnabled()
    {
        return this.m_Enabled;
    }

    // 是否是可见的
    IsVisible()
    {
        return this.m_Visible;
    }

    // 连接需要连接的服务器
    ConnectDB(callBack)
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

    OnAbortEvent(tcpClient)
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

    OnConnectEvent(sucess, tcpClient)
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
    KickAllGates()
    {
        this.CloseAllClientSocket();
    }

    SendToDBServer(commandId, clientId, args)
    {
        if (this.m_DBClient == null)
            return false;
        // 发送给DB SERVER
        this.m_DBClient.SendMessage(MessageConsts.ToDBMessage.DB_COMMAND, new GS_DB_Command(commandId, clientId, args));
        return true;
    }
}

GameServer.Create = function (id, port, dbIp, dbPort)
{
    if (id == null || port == null)
        return null;

    var server = new GameServer(id, port, dbIp, dbPort);
    server.SetPacketHandlerClass(DefaultPacketHandler);

    // 注冊消息
    new RegisterGameMessage(server);

    if (!server.ConnectDB())
    {
        console.error("\n DB服务器连接失败");
    }

    server.Listen(port);
    return server;
}

module.exports = GameServer;