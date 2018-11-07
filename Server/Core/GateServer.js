/*
GATE设计
*/
var NetManager = require("./NetManager");
var TcpClient = require("./TcpClient");
var DefaultPacketHandler = require("./DefaultPacketHandler");
var RegisterGateMessage = require("./RegisterGateMessage");

function GateServer(id, port)
{
    this.Init();
    this.m_Id = id;
    // 连接GS或者LS
    this.m_Client = null;
}

GateServer.prototype = NetManager.prototype;
GateServer.prototype.constructor = GateServer;

GateServer.prototype.GetId =
    function ()
    {
        return this.m_Id;
    }

// 分发到上层服务器
GateServer.prototype.DispatchToServer =
    function (headerId, packet, clientId)
    {
        if (this.m_Client == null || packet == null)
            return false;
        
        if (headerId == null)
            headerId = packet.header.header;

        // 转发
        return this.m_Client.SendBuf(headerId, packet.data, [clientId]);
    }

/*-------------------------------------------业务逻辑--------------------------------------*/



/*----------------------------------------------------------------------------------------*/

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

        server.Listen(port);
        return server;
    }

module.exports = GateServer;