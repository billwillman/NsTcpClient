
var AbstractPacketHandler = require("./AbstractPacketHandler")
//var GamePacketHander = require("./GamePacketHander");
var GamePacketHander = require("./GamePacketHander");
var GamePacket = require("./GamePacket");
var NetManager = require("./NetManager")

function DefaultPacketHandler()
{
}

// 继承
DefaultPacketHandler.prototype = AbstractPacketHandler.prototype;

DefaultPacketHandler.prototype.constructor = DefaultPacketHandler;
DefaultPacketHandler.prototype.OnPacketRead =
    function (data)
    {
        if (data == null ||  !Buffer.isBuffer(data))
            return;

        // 粘包处理
        var recvsize = this.GetReadData(data);
        if (recvsize > 0)
        {
            this.m_RecvSize += recvsize;
            var recvBufSz = this.m_RecvSize;
            var i = 0;
            var headerSize = GamePacketHander.Size;
            var orgOffset = this.m_RecvBuffer.byteOffset;
            while (recvBufSz - i >= headerSize) {
                this.m_RecvBuffer.byteOffset = orgOffset + i;
                var header = new GamePacketHander(this.m_RecvBuffer, recvsize);
                if (recvBufSz - i < header.dataSize + headerSize)
                    break;
                var packet = new GamePacket(header, null);
                if (packet.header.dataSize <= 0)
                {
                    packet.header.dataSize = 0;
                } else
                {
                    packet.data = Buffer.allocUnsafe(packet.header.dataSize);
                    this.m_RecvBuffer.copy(packet.data, 0, i + headerSize, packet.header.dataSize);
                }

                //--------------- 進入隊列
                NetManager.GetInstance()._SendPacketRead(packet);
                //-----------------------
                i += headerSize + header.dataSize;
            }

            recvBufSz -= i;
            this.m_RecvSize = recvBufSz;
            if (this.m_RecvSize > 0)
                this.MoveMySelf(recvBufSz, i);
        }
    }

DefaultPacketHandler.prototype.SendBuffer =
    function (buf)
    {

        return true;
    }


module.exports = DefaultPacketHandler