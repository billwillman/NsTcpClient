#define USE_PROTOBUF_NET

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

#if USE_PROTOBUF_NET

namespace NsTcpClient
{
    public class ProtoMessageMgr: Singleton<ProtoMessageMgr>
    {
        private Dictionary<System.Type, Google.Protobuf.MessageParser> m_MessageParserMap = new Dictionary<Type, Google.Protobuf.MessageParser>();

        public Google.Protobuf.IMessage Parser(System.Type messageType, byte[] buf)
        {
            if (messageType == null || buf == null || buf.Length <= 0)
                return null;
            Google.Protobuf.MessageParser parser;
            if (m_MessageParserMap.TryGetValue(messageType, out parser) && parser != null)
            {
                // 此处代码可以优化
                Google.Protobuf.IMessage ret = parser.ParseFrom(buf);
                return ret;
            }

            return null;
        }

        public Google.Protobuf.IMessage Parser<T>(byte[] buf) where T: class, Google.Protobuf.IMessage<T>
        {
            if (buf == null || buf.Length <= 0)
                return null;
            System.Type tt = typeof(T);
            return Parser(tt, buf);
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
            Google.Protobuf.CodedOutputStream output = new Google.Protobuf.CodedOutputStream(stream);
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
    }
}

#endif
