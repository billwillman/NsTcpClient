/*
GATE设计
*/
var NetManager = require("./NetManager");
var TcpClient = require("./TcpClient");

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
    function (id, port)
    {
        if (id == null || port == null)
            return null;
    }

module.exports = GateServer;