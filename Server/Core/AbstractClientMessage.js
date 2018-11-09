/*
 抽象客户端发送消息
*/

class AbstractClientMessage
{
    constructor()
    {
        this.m_Buf = null;
        this.m_BufOffset = 0;
    }

    _InitBuf(requireSize)
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

    DoSend()
    {}

    WriteByte(byte)
    {
        this._InitBuf(1);
        this.m_Buf.writeUInt8(byte, this.m_BufOffset);
        this.m_BufOffset++;
    }

    WriteBool(bool)
    {
        var byte = bool ? 1: 0;
        this.WriteByte(byte);
    }

    WriteInt(int)
    {
        this._InitBuf(4);
        this.m_Buf.writeInt32LE(int, this.m_BufOffset);
        this.m_BufOffset += 4;
    }

    WriteUInt(uint)
    {
        this._InitBuf(4);
        this.m_Buf.writeUInt32LE(uint, this.m_BufOffset);
        this.m_BufOffset += 4;
    }

    WriteInt64(int64)
    {
        this._InitBuf(6);
        this.m_Buf.writeIntLE(int64, this.m_BufOffset, 6);
        this.m_BufOffset += 6;
    }

    WriteUInt64(uint64)
    {
        this._InitBuf(6);
        this.m_Buf.writeUIntLE(uint64, this.m_BufOffset, 6);
        this.m_BufOffset += 6;
    }

    WriteString(str)
    {
        this._InitBuf(4 + str.length);
        // 字符串长度
        this.WriteUInt(str.length);
        this.m_Buf.write(str, this.m_BufOffset, str.length, "utf-8");
        this.m_BufOffset += str.length;
    }

    WriteUInt16(uint16)
    {
        this._InitBuf(2);
        this.m_Buf.writeUInt16LE(uint16, this.m_BufOffset);
        this.m_BufOffset += 2;
    }

    WriteInt16(int16)
    {
        this._InitBuf(2);
        this.m_Buf.writeInt16LE(int16, this.m_BufOffset);
        this.m_BufOffset += 2;
    }

    WriteFloat(float)
    {
        this._InitBuf(4);
        this.m_Buf.writeFloatLE(float, this.m_BufOffset);
        this.m_BufOffset += 4;
    }

    WriteDouble(double)
    {
        this._InitBuf(8);
        this.m_Buf.writeDoubleLE(double, this.m_BufOffset);
        this.m_BufOffset += 8;
    }
}


module.exports = AbstractClientMessage;