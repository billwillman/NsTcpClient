/*
    协议处理
* */

function AbstractPacketHandler()
{
}

AbstractPacketHandler.prototype.GetRecvBuffer =
    function ()
    {
        if (this.m_RecvBuffer == null)
            this.m_RecvBuffer = Buffer.allocUnsafe(64 * 1024);
        return this.m_RecvBuffer;
    }

AbstractPacketHandler.prototype.GetSendBuffer =
    function ()
    {
        if (this.m_SendBuffer == null)
            this.m_SendBuffer = Buffer.allocUnsafe(64 * 1024);
        return this.m_SendBuffer;
    }

AbstractPacketHandler.prototype.OnPacketRead =
    function (data)
    {
        console.log("OnPakcetRead");
    }

AbstractPacketHandler.prototype.constructor = AbstractPacketHandler;

AbstractPacketHandler.prototype.SendBuffer =
    function (buf)
    {
        console.log("SendBuffer");
    }

module.exports = AbstractPacketHandler
