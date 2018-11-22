
var AbstractPacketHandler = require("./AbstractPacketHandler")
var GamePacketHander = require("./GamePacketHander");
var GamePacket = require("./GamePacket");
var NetManager = require("./NetManager");

class DefaultPacketHandler extends AbstractPacketHandler
{
    constructor(netMgr, recvBufSize)
    {
        super(netMgr, recvBufSize);
    }

    UdpBufToPacket(data)
    {
        if (data == null ||  !Buffer.isBuffer(data))
            return null;
        var headerSize = GamePacketHander.Size;
        var header = new GamePacketHander(data, data.length, 0);
        if (header.dataSize + headerSize > data.length)
            return null;
        var packet = new GamePacket(header, null);
        if (packet.header.dataSize <= 0)
        {
            packet.header.dataSize = 0;
        } else
        {
            packet.data = Buffer.allocUnsafe(packet.header.dataSize);
            var sourceStart = headerSize;
            data.copy(packet.data, 0, sourceStart, sourceStart + packet.header.dataSize);
        }
        return packet;
    }

    OnPacketRead(data, clientSocket)
    {
        if (this.m_RecvBuffer == null || data == null ||  !Buffer.isBuffer(data))
            return;

        // 粘包处理
        var recvsize = this.GetReadData(data);
        if (recvsize > 0)
        {
            this.m_RecvSize += recvsize;
            var recvBufSz = this.m_RecvSize;
            var i = 0;
            var headerSize = GamePacketHander.Size;
            //var orgOffset = this.m_RecvBuffer.byteOffset;
            var orgOffset = 0;
            while (recvBufSz - i >= headerSize) {
              //  this.m_RecvBuffer.byteOffset = orgOffset + i;
                var offset = orgOffset + i;
                var header = new GamePacketHander(this.m_RecvBuffer, recvsize, offset);
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
                if (this.m_NetMgr != null && this.m_NetMgr._SendPacketRead != null)
                    this.m_NetMgr._SendPacketRead.call(this.m_NetMgr, packet, clientSocket);
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
                if (this.m_NetMgr != null && this.m_NetMgr.CloseClientSocket != null)
                    this.m_NetMgr.CloseClientSocket.call(this.m_NetMgr, clientSocket, recvsize);
            }
        }
    }

    GeneratorSendBuf(packetHandle, buf, bufOffset, sendSize, args)
    {
        // 支持ArrayBuffer, 因为ProtoBuf使用了ArrayBuffer
        if (buf != null && !Buffer.isBuffer(buf) && !(buf instanceof ArrayBuffer))
            return null;
        var bufLen = 0;
        var isArrayBuffer = false;
        if (buf != null)
        {
            isArrayBuffer = buf instanceof ArrayBuffer;
            if (isArrayBuffer)
                bufLen = buf.byteLength;
            else
                bufLen = buf.length;
        }
        var hasData = buf != null && bufLen > 0;
        var packetHead = new GamePacketHander(null, 0, 0);
        packetHead.header = packetHandle;
        if (hasData)
            packetHead.dataSize = bufLen;
        if (args != null && args instanceof Array && args[0] != null)
        {
            packetHead.headerCrc32 = args[0];
        }
    
        var sendBufSize = GamePacketHander.Size + packetHead.dataSize;
        var sendBuf = Buffer.allocUnsafe(sendBufSize);
        if (!packetHead.ToBuf(sendBuf))
            return null;
    
        if (hasData)
        {
            if (bufOffset == null && sendSize == null)
            {
                if (isArrayBuffer)
                {
                    buf = Buffer.from(buf);
                }
                
                var dataOffset = GamePacketHander.Size;
                buf.copy(sendBuf, dataOffset);
            } else
            {
                if (bufOffset == null)
                    bufOffset = 0;
                if (bufOffset >= bufLen)
                    return null;
                var maxSendSize = bufLen - bufOffset;
                if (sendSize == null || sendSize > maxSendSize)
                {
                    sendSize = maxSendSize;
                }

                var dataOffset = GamePacketHander.Size;
                if (isArrayBuffer)
                {
                    buf = Buffer.from(buf);
                }
                buf.copy(sendBuf, dataOffset, bufOffset, bufOffset + sendSize);
            }
        }
        return sendBuf;
    }

    SendBuf(clientSocket, packetHandle, buf, args)
    {
        if (clientSocket == null || packetHandle == null)
            return false;

        var sendBuf = this.GeneratorSendBuf(packetHandle, buf, null, null, args);
        if (sendBuf == null)
            return false;
        
        // 发送过去
        if (!clientSocket.write(sendBuf))
        {
            if (this.m_NetMgr != null && this.m_NetMgr.CloseClientSocket != null)
                this.m_NetMgr.CloseClientSocket.call(this.m_NetMgr, clientSocket);
            return false;
        }
        
        return true;
    }
}


module.exports = DefaultPacketHandler