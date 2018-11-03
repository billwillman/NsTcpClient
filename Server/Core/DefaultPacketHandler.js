
var AbstractPacketHandler = require("./AbstractPacketHandler")
var GamePacketHander = require("./GamePacketHander");
var GamePacket = require("./GamePacket");
var NetManager = require("./NetManager")

function DefaultPacketHandler(netMgr)
{
    this.m_NetMgr = netMgr;
}

// 继承
DefaultPacketHandler.prototype = AbstractPacketHandler.prototype;

DefaultPacketHandler.prototype.constructor = DefaultPacketHandler;
DefaultPacketHandler.prototype.OnPacketRead =
    function (data, clientSocket)
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
                    var sourceStart = i + headerSize;
                    this.m_RecvBuffer.copy(packet.data, 0, sourceStart, sourceStart + packet.header.dataSize);
                }

                //--------------- 進入隊列
                if (m_NetMgr != null)
                    m_NetMgr._SendPacketRead.call(m_NetMgr, packet, clientSocket);
                //-----------------------
                i += headerSize + header.dataSize;
            }

            recvBufSz -= i;
            this.m_RecvSize = recvBufSz;
            if (this.m_RecvSize > 0)
                this.MoveMySelf(recvBufSz, i);
        } else if (recvsize == -2 || recvsize == -1)
        {
            // 缓冲区满了, 关闭SOCKET
            if (clientSocket != null)
            {
                if (m_NetMgr != null)
                    m_NetMgr.CloseClientSocket.call(m_NetMgr, clientSocket);
            }
        }
    }

DefaultPacketHandler.prototype.SendBuf =
    function (clientSocket, packetHandle, buf)
    {
        if (clientSocket == null || packetHandle == null)
            return false;

        if (buf != null && !Buffer.isBuffer(buf))
            return false;
        
        var hasData = buf != null && buf.length > 0;
        var packetHead = new GamePacketHander(null, 0, 0);
        packetHead.header = packetHandle;
        if (hasData)
            packetHead.dataSize = buf.length;

        var sendBufSize = GamePacketHander.Size + packetHead.dataSize;
        var sendBuf = Buffer.allocUnsafe(sendBufSize);
        if (!packetHead.ToBuf(sendBuf))
            return false;

        if (hasData)
        {
            var dataOffset = GamePacketHander.Size;
            buf.copy(sendBuf, dataOffset);zf
        }

        // 发送过去
        if (!clientSocket.write(sendBuf))
        {
            if (m_NetMgr != null)
                m_NetMgr.CloseClientSocket(clientSocket);
            return false;
        }
        
        return true;
    }


module.exports = DefaultPacketHandler