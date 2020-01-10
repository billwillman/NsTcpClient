#define USE_PROTOBUF_NET
#define USE_CapnProto

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
#if USE_CapnProto
using CapnProto;
#endif

namespace NsTcpClient
{
    public class ProtoMessageMgr: Singleton<ProtoMessageMgr>
    {
#if USE_PROTOBUF_NET
        private Dictionary<System.Type, Google.Protobuf.MessageParser> m_MessageParserMap = new Dictionary<Type, Google.Protobuf.MessageParser>();

        public Google.Protobuf.IMessage Parser(System.Type messageType, byte[] buf, int bufSize = -1)
        {
            if (messageType == null || buf == null || buf.Length <= 0)
                return null;
            if (bufSize <= 0 || bufSize > buf.Length)
                bufSize = buf.Length;

            Google.Protobuf.MessageParser parser;
            if (m_MessageParserMap.TryGetValue(messageType, out parser) && parser != null)
            {
                // 此处代码可以优化
                Google.Protobuf.IMessage ret = parser.ParseFrom(buf, 0, bufSize);
                return ret;
            }

            return null;
        }

        public Google.Protobuf.IMessage Parser<T>(byte[] buf, int bufSize = -1) where T: class, Google.Protobuf.IMessage<T>
        {
            if (buf == null || buf.Length <= 0)
                return null;
            System.Type tt = typeof(T);
            return Parser(tt, buf, bufSize);
        }

        
        // 无使用池
        public byte[] ToBuffer<T>(Google.Protobuf.IMessage<T> message) where T: class, Google.Protobuf.IMessage<T>
        {
            if (message == null)
                return null;
            // 检查
            Google.Protobuf.ProtoPreconditions.CheckNotNull(message, "message");
            int bufSize = message.CalculateSize();
            if (bufSize <= 0)
                return null;
            // 此处代码可以优化
            byte[] buf = new byte[bufSize]; 
            Google.Protobuf.CodedOutputStream output = new Google.Protobuf.CodedOutputStream(buf);
            message.WriteTo(output);
            output.CheckNoSpaceLeft();
            return buf;
        }
        

        // 使用池
        public MemoryStream ToStream<T>(Google.Protobuf.IMessage<T> message, out int outSize) where T: class, Google.Protobuf.IMessage<T> {
            outSize = 0;
            if (message == null)
                return null;
            // 检查
            Google.Protobuf.ProtoPreconditions.CheckNotNull(message, "message");
            int bufSize = message.CalculateSize();
            if (bufSize <= 0)
                return null;
            // 代码已优化
            outSize = bufSize;
            var stream = NetByteArrayPool.GetBuffer(bufSize);
            var buffer = stream.GetBuffer();
            Google.Protobuf.CodedOutputStream output = new Google.Protobuf.CodedOutputStream(buffer, bufSize);
            message.WriteTo(output);
            output.CheckNoSpaceLeft();
            return stream;
        }

        internal bool Register(System.Type messageType, Google.Protobuf.MessageParser parser)
        {
            if (messageType == null || parser == null)
                return false;
            m_MessageParserMap[messageType] = parser;
            return true;
        }

        internal bool Register<T>(Google.Protobuf.MessageParser parser) where T: class, Google.Protobuf.IMessage<T>
        {
            if (parser == null)
                return false;
            System.Type tt = typeof(T);
            return Register(tt, parser);
        }
#endif

#if USE_CapnProto
        public bool Parser<T>(byte[] buffer, out T data, int bufSize = -1) where T: struct, CapnProto.IPointer {
            if (buffer == null || buffer.Length <= 0) {
                data = default(T);
                return false;
            }
            if (bufSize < 0 || bufSize > buffer.Length)
                bufSize = buffer.Length;
            Message msg = Message.Load(buffer, 0, bufSize);
            if (msg == null) {
                data = default(T);
                return false;
            }
            try {
                data = msg.Allocate<T>();
                return true;
            } finally {
                msg.Dispose();
                msg = null;
            }
        }

        // 使用池的版本
        public MemoryStream ToStream<T>(T message, out int outSize) where T : struct, CapnProto.IPointer {
            int byteLen = message.ByteLength();
            if (byteLen <= 0) {
                outSize = 0;
                return null;
            }
            MemoryStream stream = NetByteArrayPool.GetBuffer(byteLen);
            byte[] buffer = stream.GetBuffer();
            message.CopyTo(buffer);
            outSize = byteLen;
            return stream;
        }
#endif
        }
}


