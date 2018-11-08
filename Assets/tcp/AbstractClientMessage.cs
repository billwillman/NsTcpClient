using System;
using System.IO;
using Utils;

namespace NsTcpClient
{
    public abstract class AbstractClientMessage: DisposeObject
    {
        private MemoryStream m_Buf = null;
        private long m_DataSize = 0;

        public AbstractClientMessage(): base()
        {
            m_Buf = NetByteArrayPool.GetBuffer(1);
        }

        public byte[] GetBuffer(out long dataSize)
        {
            dataSize = 0;
            if (m_Buf == null)
                return null;
            dataSize = m_DataSize;
            return m_Buf.GetBuffer();
        }

        protected override void OnFree(bool isManual)
        {
            NetByteArrayPool.FreeBuffer(m_Buf);
            m_Buf = null;
        }

        internal abstract void DoSend();

        protected void WriteInt(int value)
        {
            if (m_Buf == null)
                return;
            long oldPos = m_Buf.Position;
            FilePathMgr.Instance.WriteInt(m_Buf, value);
            m_DataSize += m_Buf.Position - oldPos;
        }

        protected void WriteUInt(uint value)
        {
            WriteInt((int)value);
        }

        protected void WriteByte(byte value)
        {
            if (m_Buf == null)
                return;
            long oldPos = m_Buf.Position;
            m_Buf.WriteByte(value);
            m_DataSize += m_Buf.Position - oldPos;
        }

        protected void WriteBool(bool value)
        {
            if (m_Buf == null)
                return;
            long oldPos = m_Buf.Position;
            FilePathMgr.Instance.WriteBool(m_Buf, value);
            m_DataSize += m_Buf.Position - oldPos;
        }

        protected void WriteString(string value)
        {
            if (m_Buf == null)
                return;
            long oldPos = m_Buf.Position;
            FilePathMgr.Instance.WriteString(m_Buf, value);
            m_DataSize += m_Buf.Position - oldPos;
        }

        protected void WriteShort(short value)
        {
            if (m_Buf == null)
                return;
            long oldPos = m_Buf.Position;
            FilePathMgr.Instance.WriteShort(m_Buf, value);
            m_DataSize += m_Buf.Position - oldPos;
        }

        protected void WriteUShort(ushort value)
        {
            WriteShort((short)value);
        }

        protected void WriteSingle(float value)
        {
            if (m_Buf == null)
                return;
            long oldPos = m_Buf.Position;
            FilePathMgr.Instance.WriteSingle(m_Buf, value);
            m_DataSize += m_Buf.Position - oldPos;
        }

        protected void WriteDouble(double value)
        {
            if (m_Buf == null)
                return;
            long oldPos = m_Buf.Position;
            FilePathMgr.Instance.WriteDouble(m_Buf, value);
            m_DataSize += m_Buf.Position - oldPos;
        }

        protected void WriteLong(long value)
        {
            if (m_Buf == null)
                return;
            long oldPos = m_Buf.Position;
            FilePathMgr.Instance.WriteLong(m_Buf, value);
            m_DataSize += m_Buf.Position - oldPos;
        }

        protected void WriteULong(ulong value)
        {
            WriteLong((long)value);
        }
    }
}
