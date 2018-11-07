/*
虚消息管理器
*/

var IServerMessagListener = require("./IServerMessageListener");

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
            return false;
        var headerId = packet.header.header;
        if (headerId == null)
            return false;
        var msgClass = this.m_AbstractSrvMapClass[headerId];
        if (msgClass == null)
            return false;
        var msg = new msgClass();
        if (msg.OnMessage != null)
        {
            msg.OnMessage.call(msg, packet, clientSocket, netMgr);
            return true;
        }
        return false;
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