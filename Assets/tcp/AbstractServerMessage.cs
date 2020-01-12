using System.IO;
using Utils;

namespace NsTcpClient
{
    public abstract class AbstractServerMessage : DisposeObject
    {
        protected MemoryStream m_Buf = null;
        protected long m_DataSize = 0;
        public AbstractServerMessage(byte[] buffer, long dataSize): base()
        {
            if ((buffer != null) && (buffer.Length > 0)) {
                m_Buf = new MemoryStream(buffer);
                if ((dataSize <= 0) || (dataSize > buffer.Length))
                    dataSize = buffer.Length;
            } else
                dataSize = 0;
            m_DataSize = dataSize;
        }

        public bool IsBufEnd(uint size)
        {
            if (m_Buf == null)
                return true;
            long pos = m_Buf.Position;
            long newPos = pos + size;
            if ((newPos > m_Buf.Length) || (newPos > m_DataSize))
                return true;
            return false;
        }

        internal abstract void DoRecv();

        /*-----------------------------读取函数--------------------------------*/

        protected bool ReadInt(out int value)
        {
            value = 0;
            if (IsBufEnd(4))
                return false;

            value = FilePathMgr.Instance.ReadInt(m_Buf);

            return true;
        }

        protected bool ReadUInt(out uint value)
        {
            value = 0;
            int v;
            if (!ReadInt(out v))
                return false;
            value = (uint)v;
            return true;
        }

        protected bool ReadString(out string value)
        {
            try
            {
                value = FilePathMgr.Instance.ReadString(m_Buf);
            }
            catch
            {
                value = null;
                return false;
            }

            return true;
        }

        protected bool ReadLong(out long value)
        {
            value = 0;
            if (IsBufEnd(8))
                return false;
            value = FilePathMgr.Instance.ReadLong(m_Buf);
            return true;
        }

        protected bool ReadULong(out ulong value)
        {
            value = 0;
            long v;
            if (!ReadLong(out v))
                return false;
            value = (ulong)v;
            return true;
        }

        protected bool ReadByte(out byte value)
        {
            value = 0;
            if (IsBufEnd(1))
                return false;
            int v = m_Buf.ReadByte();
            value = (byte)v;
            return true;
        }

        protected bool ReadBool()
        {
            byte v;
            if (!ReadByte(out v))
                return false;
            return v != 0;
        }

        protected bool ReadSingle(out float value)
        {
            value = 0;
            if (IsBufEnd(4))
                return false;
            value = FilePathMgr.Instance.ReadSingle(m_Buf);
            return true;
        }

        protected bool ReadDouble(out double value)
        {
            value = 0;
            if (IsBufEnd(8))
                return false;
            value = FilePathMgr.Instance.ReadDouble(m_Buf);
            return true;
        }
    }
}