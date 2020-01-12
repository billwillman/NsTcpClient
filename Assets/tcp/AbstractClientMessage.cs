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
        {}

        private void InitBuf()
        {
            if (m_Buf == null)
            {
                m_Buf = NetByteArrayPool.GetBuffer(1);
                m_DataSize = 0;
            }
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
            if (m_Buf != null) {
                m_Buf.Dispose();
                m_Buf = null;
            }
        }

        internal abstract void DoSend();

        protected void WriteInt(int value)
        {
            InitBuf();
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
            InitBuf();
            long oldPos = m_Buf.Position;
            m_Buf.WriteByte(value);
            m_DataSize += m_Buf.Position - oldPos;
        }

        protected void WriteBool(bool value)
        {
            InitBuf();
            long oldPos = m_Buf.Position;
            FilePathMgr.Instance.WriteBool(m_Buf, value);
            m_DataSize += m_Buf.Position - oldPos;
        }

        protected void WriteString(string value)
        {
            InitBuf();
            long oldPos = m_Buf.Position;
            FilePathMgr.Instance.WriteString(m_Buf, value);
            m_DataSize += m_Buf.Position - oldPos;
        }

        protected void WriteShort(short value)
        {
            InitBuf();
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
            InitBuf();
            long oldPos = m_Buf.Position;
            FilePathMgr.Instance.WriteSingle(m_Buf, value);
            m_DataSize += m_Buf.Position - oldPos;
        }

        protected void WriteDouble(double value)
        {
            InitBuf();
            long oldPos = m_Buf.Position;
            FilePathMgr.Instance.WriteDouble(m_Buf, value);
            m_DataSize += m_Buf.Position - oldPos;
        }

        protected void WriteLong(long value)
        {
            InitBuf();
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
