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
        this._InitBuf();
        this.m_Buf.writeUInt8(byte, this.m_BufOffset);
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
        this._InitBuf();
        this.m_Buf.writeInt32LE(int, this.m_BufOffset);
        this.m_BufOffset += 4;
    }

AbstractClientMessage.prototype.WriteUInt =
    funciton (uint)
    {
        this._InitBuf();
        this.m_Buf.writeUInt32LE(uint, this.m_BufOffset);
        this.m_BufOffset += 4;
    }

AbstractClientMessage.prototype.WriteInt64 =
    function (int64)
    {
        this._InitBuf();
        this.m_Buf.writeIntLE(int64, this.m_BufOffset, 6);
        this.m_BufOffset += 6;
    }

// 写入字符串
AbstractClientMessage.prototype.WriteString =
    function (str)
    {
        this._InitBuf();
        // 字符串长度
        this.WriteUInt(str.byteLength);
        this.m_Buf.write(str, this.m_BufOffset, str.length, "utf-8");
        this.m_BufOffset += str.byteLength;
    }

AbstractClientMessage.prototype.WriteUInt16 =
    function (uint16)
    {
        this._InitBuf();
        this.m_Buf.writeUInt16LE(uint16, this.m_BufOffset);
        this.m_BufOffset += 2;
    }

AbstractClientMessage.prototype.WriteInt16 =
    function (int16)
    {
        this._InitBuf();
        this.m_Buf.writeInt16LE(int16, this.m_BufOffset);
        this.m_BufOffset += 2;
    }

AbstractClientMessage.prototype.WriteFloat =
    function (float)
    {
        this._InitBuf();
        this.m_Buf.writeFloatLE(float, this.m_BufOffset);
        this.m_BufOffset += 4;
    }

AbstractClientMessage.prototype.WriteDouble =
    function (double)
    {
        this._InitBuf();
        this.m_Buf.writeDoubleLE(double, this.m_BufOffset);
        this.m_BufOffset += 8;
    }


module.exports = AbstractClientMessage;