/*
GATE设计
*/
var NetManager = require("./NetManager");
var TcpClient = require("./TcpClient");

function GateServer()
{
    
}

GateServer.prototype = NetManager.prototype;
GateServer.prototype.constructor = GateServer;

module.exports = GateServer;