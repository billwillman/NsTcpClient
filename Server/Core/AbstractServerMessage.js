/*
服务器协议
*/
var IServerMessagListener = require("./IServerMessagListener");

function AbsractServerMessage()
{}

AbsractServerMessage.prototype = IServerMessagListener.prototype;
AbsractServerMessage.prototype.constructor = AbsractServerMessage;

AbsractServerMessage.prototype.OnMessage = 
    function (packet, clientSocket, netMgr)
    {
        this.m_HeaderId = packet.header.header;
        this.m_Buf = packet.data;
        this.m_NetMgr = netMgr;
    }

AbsractServerMessage.prototype.GetHeaderId =
    function ()
    {
        return this.m_HeaderId;
    }

// 接收继承方法
AbsractServerMessage.prototype.DoRecv =
    function ()
    {
        
    }


module.exports = AbsractServerMessage;