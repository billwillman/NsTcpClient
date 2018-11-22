/*
服务器协议
*/
var IServerMessagListener = require("./IServerMessageListener");
var ProtoBufMgr = require("./ProtoBufMgr");

class AbstractServerMessage extends IServerMessagListener
{
    constructor()
    {
        super();
    }

    OnMessage(packet, clientSocket, netMgr)
    {
        this.m_HeaderId = packet.header.header;
        this.m_HeaderCrc32 = packet.header.headerCrc32;
        this.m_Buf = packet.data;
        this.m_BufOffset = 0;
        this.m_NetMgr = netMgr;

        // 接收处理
        this.DoRecv(clientSocket);
    }

    NetManager()
    {
        return this.m_NetMgr;
    }

    IsBufEnd(readLen)
    {
        if (this.m_Buf == null)
            return true;
        if (readLen == null)
            readLen = 1;
        var byteLen = this.m_Buf.bytelength;
        if (this.m_BufOffset + readLen > byteLen)
            return true;
        return false;
    } 

    GetHeaderId()
    {
        return this.m_HeaderId;
    }

    // 接收继承方法
    DoRecv(clientSocket)
    {}

    ReadInt()
    {
        if (this.IsBufEnd(4))
            return null;
        var ret = this.m_Buf.readInt32LE(this.m_BufOffset);
        this.m_BufOffset += 4;
        return ret;
    }

    ReadUInt()
    {
        if (this.IsBufEnd(4))
            return null;
        var ret = this.m_Buf.readUInt32LE(this.m_BufOffset);
        this.m_BufOffset += 4;
        return ret;
    }

    ReadInt64()
    {
        if (this.IsBufEnd(6))
            return null;
        var ret = this.m_Buf.readIntLE(this.m_BufOffset, 6);
        this.m_BufOffset += 6;
        return ret;
    }

    ReadUInt64()
    {
        if (this.IsBufEnd(6))
            return null;
        var ret = this.m_Buf.readUIntLE(this.m_BufOffset, 6);
        this.m_BufOffset += 6;
        return ret;
    }

    ReadString()
    {
        var byteLen = this.ReadUInt();
        if (byteLen == null)
            return null;
        if (this.IsBufEnd(byteLen))
            return null;
        var ret = this.m_Buf.toString("utf-8", this.m_BufOffset, this.m_BufOffset + byteLen);
        this.m_BufOffset += byteLen;
        return ret;
    }

    ReadByte()
    {
        if (this.IsBufEnd(1))
            return null;
        var ret = this.m_Buf.readUInt8(this.m_BufOffset);
        this.m_BufOffset += 1;
        return ret;
    }

    ReadBool()
    {
        var byte = this.ReadByte();
        if (byte == null)
            return null;
        var ret = byte != 0;
        return ret;
    }

    ReadInt16()
    {
        if (this.IsBufEnd(2))
            return null;
        var ret = this.m_Buf.readInt16LE(this.m_BufOffset);
        this.m_BufOffset += 2;
        return ret;
    }

    ReadUInt16()
    {
        if (this.IsBufEnd(2))
            return null;
        var ret = this.m_Buf.readUInt16LE(this.m_BufOffset);
        this.m_BufOffset += 2;
        return ret;
    }

    ReadFloat()
    {
        if (this.IsBufEnd(4))
            return null;
        var ret = this.m_Buf.readFloatLE(this.m_BufOffset);
        this.m_BufOffset += 4;
        return ret;
    }

    ReadDouble()
    {
        if (this.IsBufEnd(8))
            return null;
        var ret = this.m_Buf.readDoubleLE(this.m_BufOffset);
        this.m_BufOffset += 8;
        return ret;
    }

    SendMessage(packetHandle, message, targetSocket, args)
    {
        if (this.m_NetMgr == null || this.m_NetMgr.SendMessage == null)
            return;
        this.m_NetMgr.SendMessage.call(this.m_NetMgr, packetHandle, message, args, targetSocket);
    }

    SendProtoMessage(packetHandle, message, targetSocket, args)
    {
        if (this.m_NetMgr == null || this.m_NetMgr.SendProtoMessage == null)
            return;
        this.m_NetMgr.SendProtoMessage.call(this.m_NetMgr, packetHandle, message, args, targetSocket);
    }

    NewProtoMessageById(packetId)
    {
        if (this.m_NetMgr == null || this.m_NetMgr.NewProtoMessageById == null)
            return null;
        return this.m_NetMgr.NewProtoMessageById.call(this.m_NetMgr, packetId);
    }

    // 获得ProtoMessage对象
    GetProtoMessageObject()
    {
        return ProtoBufMgr.GetInstance().BufToProtoMessageByPacKetId(this.m_HeaderId, this.m_Buf);
    }
}

module.exports = AbstractServerMessage;