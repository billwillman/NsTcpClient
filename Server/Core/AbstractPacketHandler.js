/*
    协议处理
* */

function AbstractPacketHandler()
{
}

AbstractPacketHandler.prototype.constructor = AbstractPacketHandler;

AbstractPacketHandler.prototype.m_RecvSize = 0;

AbstractPacketHandler.prototype.m_RecvBuffer = Buffer.allocUnsafe(64 * 1024);

AbstractPacketHandler.prototype.GetReadData =
    function (data)
    {
        if (data == null || !Buffer.isBuffer(data))
            return -1;
        if (data.length <= 0)
            return 0;
        var bufSize = this.m_RecvBuffer.length;
        if (bufSize <= 0)
            return 0;
        var ret = bufSize - this.m_RecvSize;
        if (ret <= 0)
        {
            ret = 0;
            return ret;
        }

        data.copy(this.m_RecvBuffer, this.m_RecvSize, 0, ret);
        return ret;
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

AbstractPacketHandler.prototype.SendBuffer =
    function (buf)
    {
        console.log("SendBuffer");
    }

AbstractPacketHandler.prototype.MoveMySelf =
    function (recvBufSz)
    {
        if (recvBufSz == null || recvBufSz <= 0)
            return;
        this.m_RecvBuffer.copy(this.m_RecvBuffer, 0, i, recvBufSz);
    }

module.exports = AbstractPacketHandler
