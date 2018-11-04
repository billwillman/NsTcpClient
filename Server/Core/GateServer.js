/*
GATE设计
*/
var NetManager = require("./NetManager");
var TcpClient = require("./TcpClient");

function GateServer()
{
    this.Init();
}

GateServer.prototype = NetManager.prototype;
GateServer.prototype.constructor = GateServer;

GateServer.Create =
    function (ip, port)
    {
        
    }

module.exports = GateServer;