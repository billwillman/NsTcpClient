/*
    协议处理
* */

function AbstractPacketHandler(netMgr, recvBufSize)
{
    this.m_NetMgr = netMgr;
    this.InitRecvBuffer(recvBufSize);
}

AbstractPacketHandler.prototype.InitRecvBuffer =
    function (allocSize)
    {
        this.m_RecvSize = 0;
        // 20K, 一个用户20K数据
        if (allocSize == null)
            allocSize = 20 * 1024;
        this.m_RecvBuffer = Buffer.allocUnsafe(allocSize); 
    }

AbstractPacketHandler.prototype.constructor = AbstractPacketHandler;

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
            // 表明没有接收空间了
            ret = -2;
            return ret;
        }

        var hasReadSize = data.length;
        if (ret > hasReadSize)
            ret = hasReadSize;

        data.copy(this.m_RecvBuffer, this.m_RecvSize, 0, ret);
        return ret;
    }

AbstractPacketHandler.prototype.OnPacketRead =
    function (data, clientSocket)
    {
        console.log("OnPakcetRead");
    }

AbstractPacketHandler.prototype.SendBuf =
    function (clientSocket, packetHandle, buf)
    {
        console.log("SendBuffer");
        return false;
    }

AbstractPacketHandler.prototype.MoveMySelf =
    function (recvBufSz, i)
    {
        if (recvBufSz == null || recvBufSz <= 0)
            return;
        this.m_RecvBuffer.copy(this.m_RecvBuffer, 0, i, i + recvBufSz);
    }

module.exports = AbstractPacketHandler
