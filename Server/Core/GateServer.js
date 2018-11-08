/*
GATE设计
*/
var NetManager = require("./NetManager");
var TcpClient = require("./TcpClient");
var DefaultPacketHandler = require("./DefaultPacketHandler");
var RegisterGateMessage = require("./RegisterGateMessage");

class GateServer extends NetManager
{
    constructor(id, port)
    {
        super();
        this.m_Id = id;
        // 连接GS或者LS
        this.m_Client = null;
        this.m_ClientId = [0];
    }

    GetId()
    {
        return this.m_Id;
    }

    OnAddSessionEvent(session)
    {
        // 增加新的Session
        var globalId = ++GateServer.GlobalClientId;
        session.clientId = globalId;
    }
    
    // 分发到上层服务器
    DispatchToServer(headerId, packet, clientId)
    {
        if (this.m_Client == null || packet == null)
            return false;
        
        if (headerId == null)
            headerId = packet.header.header;

        // 转发
        this.m_ClientId[0] = clientId;
        return this.m_Client.SendBuf(headerId, packet.data, this.m_ClientId);
    }

    /*-------------------------------------------业务逻辑--------------------------------------*/

    /*----------------------------------------------------------------------------------------*/
}

GateServer.GlobalClientId = 0;

// 创建GATE服务器
GateServer.Create =
    function (id, port, isGS)
    {
        if (id == null || port == null)
            return null;
        var server = new GateServer(id, port);
        server.SetPacketHandlerClass(DefaultPacketHandler);

        // 是否是GS的GATE
        if (isGS == null)
            isGS = false;

        // 注冊消息
        new RegisterGateMessage(server, isGS);

        // 5秒必须收到心跳包，否则断线
        server.Listen(port);
        return server;
    }

module.exports = GateServer;