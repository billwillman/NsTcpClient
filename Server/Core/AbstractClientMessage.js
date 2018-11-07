/*
 抽象客户端发送消息
*/

function AbstractClientMessage()
{
    this.m_Buf = null;
    this.m_BufOffset = 0;
}

AbstractClientMessage.prototype.constructor = AbstractClientMessage;

AbstractClientMessage.prototype._InitBuf =
    function (requireSize)
    {
        if (requireSize == null)
            requireSize = 0;
        if (this.m_Buf == null)
        {
            this.m_Buf = Buffer.allocUnsafe(requireSize);
            this.m_BufOffset = 0;
        } else
        {
            var requireLen = requireSize + this.m_BufOffset;
            if (requireLen > this.m_Buf.byteLength)
            {
                // 重新分配
                var newBuf = Buffer.allocUnsafe(requireLen);
                this.m_Buf.copy(newBuf);
                this.m_Buf = newBuf;
            }
        }
    }

AbstractClientMessage.prototype.DoSend =
    function ()
    {}

AbstractClientMessage.prototype.WriteByte =
    function (byte)
    {
        this._InitBuf(1);
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
        this._InitBuf(4);
        this.m_Buf.writeInt32LE(int, this.m_BufOffset);
        this.m_BufOffset += 4;
    }

AbstractClientMessage.prototype.WriteUInt =
    function (uint)
    {
        this._InitBuf(4);
        this.m_Buf.writeUInt32LE(uint, this.m_BufOffset);
        this.m_BufOffset += 4;
    }

AbstractClientMessage.prototype.WriteInt64 =
    function (int64)
    {
        this._InitBuf(6);
        this.m_Buf.writeIntLE(int64, this.m_BufOffset, 6);
        this.m_BufOffset += 6;
    }

// 写入字符串
AbstractClientMessage.prototype.WriteString =
    function (str)
    {
        this._InitBuf(4 + str.length);
        // 字符串长度
        this.WriteUInt(str.length);
        this.m_Buf.write(str, this.m_BufOffset, str.length, "utf-8");
        this.m_BufOffset += str.length;
    }

AbstractClientMessage.prototype.WriteUInt16 =
    function (uint16)
    {
        this._InitBuf(2);
        this.m_Buf.writeUInt16LE(uint16, this.m_BufOffset);
        this.m_BufOffset += 2;
    }

AbstractClientMessage.prototype.WriteInt16 =
    function (int16)
    {
        this._InitBuf(2);
        this.m_Buf.writeInt16LE(int16, this.m_BufOffset);
        this.m_BufOffset += 2;
    }

AbstractClientMessage.prototype.WriteFloat =
    function (float)
    {
        this._InitBuf(4);
        this.m_Buf.writeFloatLE(float, this.m_BufOffset);
        this.m_BufOffset += 4;
    }

AbstractClientMessage.prototype.WriteDouble =
    function (double)
    {
        this._InitBuf(8);
        this.m_Buf.writeDoubleLE(double, this.m_BufOffset);
        this.m_BufOffset += 8;
    }


module.exports = AbstractClientMessage;