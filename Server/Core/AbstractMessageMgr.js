/*
虚消息管理器
*/

var IServerMessagListener = require("./IServerMessagListener");

function AbstractMessageMgr()
{
    this.m_AbstractSrvMapClass = null;
}

AbstractMessageMgr.prototype = IServerMessagListener.prototype;
AbstractMessageMgr.prototype.construcotr = AbstractMessageMgr;

AbstractMessageMgr.prototype.OnMessage =
    function (packet, clientSocket, netMgr)
    {
        // 分发
        if (this.m_AbstractSrvMapClass == null)
            return;
        var headerId = packet.header.header;
        if (headerId == null)
            return;
        var msgClass = this.m_AbstractSrvMapClass[headerId];
        if (msgClass == null)
            return;
        var msg = new msgClass();
        if (msg.OnMessage != null)
        {
            msg.OnMessage.call(msg, packet, clientSocket, netMgr);
        }
    }

AbstractMessageMgr.prototype.RegisterSrvMsg =
    function (headerId, abstractMessageClass)
    {
        if (headerId == null || abstractMessageClass == null)
            return;
        if (this.m_AbstractSrvMapClass == null)
            this.m_AbstractSrvMapClass = {};
        this.m_AbstractSrvMapClass[headerId] = abstractMessageClass;
    }



module.exports = AbstractMessageMgr;