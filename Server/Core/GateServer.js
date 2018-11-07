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
}

GateServer.prototype = NetManager.prototype;
GateServer.prototype.constructor = GateServer;

GateServer.prototype.GetId =
    function ()
    {
        return this.m_Id;
    }

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
        new RegisterGateMessage(this, isGS);

        server.Listen(port);
        return server;
    }

module.exports = GateServer;