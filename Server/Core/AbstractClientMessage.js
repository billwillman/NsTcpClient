/*
 抽象客户端发送消息
*/

function AbstractClientMessage(headerId)
{
    this.m_headerId = headerId;
    this.m_Buf = null;
    this.m_BufOffset = 0;
}

AbstractClientMessage.prototype.constructor = AbstractClientMessage;

AbstractClientMessage.prototype._InitBuf =
    function ()
    {
        if (this.m_Buf == null)
        {
            this.m_Buf = Buffer.allocUnsafe(0);
            this.m_BufOffset = 0;
        }
    }

AbstractClientMessage.prototype.DoSend =
    function (buf)
    {}

AbstractClientMessage.prototype.WriteByte =
    function (byte)
    {
        _InitBuf();
        this.m_Buf.fill(byte, this.m_BufOffset, 1);
        this.m_BufOffset++;
    }

AbstractClientMessage.prototype.WriteBool =
    function (bool)
    {
        var byte = bool ? 1: 0;
        this.WriteByte(byte);
    }

AbstractClientMessage.prototype.WriteInt =
    function (int)
    {
        _InitBuf();
        this.m_Buf.writeInt32LE(int, this.m_BufOffset);
        this.m_BufOffset += 4;
    }

AbstractClientMessage.prototype.WriteUInt =
    funciton (uint)
    {
        _InitBuf();
        this.m_Buf.writeUInt32LE(uint, this.m_BufOffset);
        this.m_BufOffset += 4;
    }

AbstractClientMessage.prototype.WriteInt64 =
    function (int64)
    {
        _InitBuf();
        this.m_Buf.writeIntLE(int64, this.m_BufOffset, 6);
        this.m_BufOffset += 6;
    }

// 写入字符串
AbstractClientMessage.prototype.WriteString =
    function (str)
    {
        _InitBuf();
        // 字符串长度
        this.WriteUInt(str.byteLength);
        this.m_Buf.write(str, this.m_BufOffset, str.length, "utf-8");
        this.m_BufOffset += str.byteLength;
    }


module.exports = AbstractClientMessage;