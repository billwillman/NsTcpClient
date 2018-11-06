/*
服务器协议
*/
var IServerMessagListener = require("./IServerMessagListener");

function AbsractServerMessage()
{}

AbsractServerMessage.prototype = IServerMessagListener.prototype;
AbsractServerMessage.prototype.constructor = AbsractServerMessage;

AbsractServerMessage.prototype.OnMessage = 
    function (packet, clientSocket, netMgr)
    {
        this.m_HeaderId = packet.header.header;
        this.m_Buf = packet.data;
        this.m_BufOffset = 0;
        this.m_NetMgr = netMgr;

        // 接收处理
        this.DoRecv();
    }

// 是否是读到头了
AbsractServerMessage.prototype.IsBufEnd =
    function (readLen)
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

AbsractServerMessage.prototype.GetHeaderId =
    function ()
    {
        return this.m_HeaderId;
    }

// 接收继承方法
AbsractServerMessage.prototype.DoRecv =
    function ()
    {}

AbsractServerMessage.prototype.ReadInt =
    function ()
    {
        if (this.IsBufEnd(4))
            return null;
        var ret = this.m_Buf.readInt32LE(this.m_BufOffset);
        this.m_BufOffset += 4;
        return ret;
    }

AbsractServerMessage.prototype.ReadUInt =
    function ()
    {
        if (this.IsBufEnd(4))
            return null;
        var ret = this.m_Buf.readUInt32LE(this.m_BufOffset);
        this.m_BufOffset += 4;
        return ret;
    }

AbsractServerMessage.prototype.ReadInt64 =
    function ()
    {
        if (this.IsBufEnd(6))
            return null;
        var ret = this.m_Buf.readIntLE(this.m_BufOffset, 6);
        this.m_BufOffset += 6;
        return ret;
    }

AbsractServerMessage.prototype.ReadUInt64 =
    function ()
    {
        if (this.IsBufEnd(6))
            return null;
        var ret = this.m_Buf.readUIntLE(this.m_BufOffset, 6);
        this.m_BufOffset += 6;
        return ret;
    }

AbsractServerMessage.prototype.ReadString =
    function ()
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

AbsractServerMessage.prototype.ReadByte =
    function ()
    {
        if (this.IsBufEnd(1))
            return null;
        var ret = this.m_Buf.readUInt8(this.m_BufOffset);
        this.m_BufOffset += 1;
        return ret;
    }

AbsractServerMessage.prototype.ReadBool =
    function ()
    {
        var byte = this.ReadByte();
        if (byte == null)
            return null;
        var ret = byte != 0;
        return ret;
    }

AbsractServerMessage.prototype.ReadInt16 =
    function ()
    {
        if (this.IsBufEnd(2))
            return null;
        var ret = this.m_Buf.readInt16LE(this.m_BufOffset);
        this.m_BufOffset += 2;
        return ret;
    }

AbsractServerMessage.prototype.ReadUInt16 =
    function ()
    {
        if (this.IsBufEnd(2))
            return null;
        var ret = this.m_Buf.readUInt16LE(this.m_BufOffset);
        this.m_BufOffset += 2;
        return ret;
    }

AbsractServerMessage.prototype.ReadFloat =
    function ()
    {
        if (this.IsBufEnd(4))
            return null;
        var ret = this.m_Buf.readFloatLE(this.m_BufOffset);
        this.m_BufOffset += 4;
        return ret;
    }

AbsractServerMessage.prototype.ReadDouble =
    function ()
    {
        if (this.IsBufEnd(8))
            return null;
        var ret = this.m_Buf.readDoubleLE(this.m_BufOffset);
        this.m_BufOffset += 8;
        return ret;
    }

module.exports = AbsractServerMessage;