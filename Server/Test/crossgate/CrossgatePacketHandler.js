/*
魔力协议格式
*/

var AbstractPacketHandler = require("../../Core/AbstractPacketHandler")
var GamePacket = require("../../Core/GamePacket");
var GamePacketHander = require("../../Core/GamePacketHander");

// 不用实现Udp,魔力不是UDP
class CrossgatePacketHandler extends AbstractPacketHandler
{
    constructor(netMgr, recvBufSize)
    {
        super(netMgr, recvBufSize);
    }

    OnPacketRead(data, clientSocket)
    {
        if (this.m_RecvBuffer == null || data == null ||  !Buffer.isBuffer(data))
            return;

        var recvsize = this.GetReadData(data);
        if (recvsize > 0)
        {
            this.m_RecvSize += recvsize;
            var recvBufSz = this.m_RecvSize;
            var i = 0;
            var headerSize = 2;
            while (recvBufSz - i >= headerSize)
            {
                var dataSize = this.m_RecvBuffer.readUInt16LE(i);
                if ((recvBufSz - i) < (dataSize + headerSize))
                    break;
                var opCode = this.m_RecvBuffer.readUInt16LE(i + headerSize);
                var header = new GamePacketHander(null, 0, 0);
                header.header = opCode;
                var packet = new GamePacket(header, null);
                var bufSize = dataSize - 2;
                if (bufSize <= 0)
                {
                    packet.data = null;
                } else
                {
                    var buffer = Buffer.allocUnsafe(bufSize);
                    var opCopyStart = i + headerSize + 2;
                    this.m_RecvBuffer.copy(buffer, 0, opCopyStart, opCopyStart + bufSize);
                    packet.data = buffer;
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

        var hasBufData = bufLen > 0;
        bufLen += 2;
        var sendBuf = Buffer.allocUnsafe(bufLen + 2);
        var offset = 0;
        sendBuf.writeUInt16LE(bufLen, offset);
        offset += 2;
        sendBuf.writeUInt16LE(packetHandle, offset);
        offset += 2;

        if (hasBufData)
        {
            if (bufOffset == null && sendSize == null)
            {
                if (isArrayBuffer)
                {
                    buf = Buffer.from(buf);
                }
                
                var dataOffset = offset;
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

                var dataOffset = offset;
                if (isArrayBuffer)
                {
                    buf = Buffer.from(buf);
                }
                buf.copy(sendBuf, dataOffset, bufOffset, bufOffset + sendSize);
            }
        }
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

module.exports = CrossgatePacketHandler;