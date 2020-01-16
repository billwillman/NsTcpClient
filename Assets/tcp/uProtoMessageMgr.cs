#define USE_PROTOBUF_NET
#define USE_CapnProto

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
#if USE_CapnProto
using CapnProto;
#endif

namespace NsTcpClient
{

#if USE_CapnProto
    public struct CapnProtoMsg {
        public ByteBufferNode allocator;
        public Message msg;

        public long MessageSizeLong {
            get {
                if (msg != null)
                    return (msg.WordCount << 3);
                return 0;
            }
        }

        public int MessageSize {
            get {
                if (msg != null)
                    return (int)(msg.WordCount << 3);
                return 0;
            }
        }

        public Pointer Root {
            get {
                if (msg == null)
                    return default(Pointer);
                return msg.Root;
            }
        }

        public byte[] GetBuffer() {
            if (allocator != null)
                return allocator.GetBuffer();
            return null;
        }

        public byte[] Buffer {
            get {
                if (allocator != null)
                    return allocator.Buffer;
                return null;
            }
        }

        public void Dispose() {
            if (msg != null) {
                msg.Dispose();
                msg = null;
            }
            if (allocator != null) {
                allocator.Dispose();
                allocator = null;
            }
        }
    }
#endif

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
        public static byte[] ToBuffer<T>(Google.Protobuf.IMessage<T> message) where T: class, Google.Protobuf.IMessage<T>
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
			output.Dispose ();
            return buf;
        }

        // 使用池
        public static ByteBufferNode ToBufferNode<T>(Google.Protobuf.IMessage<T> message, out int outSize) where T : class, Google.Protobuf.IMessage<T> {
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
            var stream = NetByteArrayPool.GetByteBufferNode(bufSize);
            var buffer = stream.GetBuffer();
            Google.Protobuf.CodedOutputStream output = new Google.Protobuf.CodedOutputStream(buffer, bufSize);
            message.WriteTo(output);
            output.CheckNoSpaceLeft();
			output.Dispose ();
            return stream;
        }


        // 使用池
        private static MemoryStream ToStream<T>(Google.Protobuf.IMessage<T> message, out int outSize) where T: class, Google.Protobuf.IMessage<T> {
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
			output.Dispose ();
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

        public static bool Parser<T>(byte[] buffer, out T data, int bufSize = -1) where T: struct, CapnProto.IPointer {
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
                 
                //data = msg.Allocate<T>();
                if (!msg.ReadNext())
                    throw new EndOfStreamException();
                IPointer ptr = msg.Root;
                data = (T)ptr;
                return true;
            } finally {
                msg.Dispose();
                msg = null;
            }
        }

        public static bool Parser<T>(CapnProtoMsg msg, out T data, int dataSize = -1) where T : struct, CapnProto.IPointer {
            return Parser<T>(msg.GetBuffer(), out data, dataSize);
        }

        public static CapnProtoMsg CreateCapnProtoMsg() {

            ByteBufferNode byteNode = NetByteArrayPool.GetByteBufferNode(NetByteArrayPool._cSmallBufferSize);
            byte[] buffer = byteNode.Buffer;
           

            CapnProtoMsg ret = new CapnProtoMsg();
            CapnProto.Message allocator = CapnProto.Message.Load(buffer, 0, buffer.Length);
            ret.allocator = byteNode;
            ret.msg = allocator;
            return ret;
        }

        public static CapnProto.Text CreateText(CapnProto.Pointer owner, string value) {
            CapnProto.Text ret = CapnProto.Text.Create(owner, value);
            return ret;
        }

        public static CapnProto.Text CreateText(CapnProtoMsg msg, string value) {
            return CreateText(msg.Root, value);
        }

        public static CapnProto.FixedSizeList<T> CreateList<T>(CapnProto.Pointer owner, IList<T> list) {
            CapnProto.FixedSizeList<T> ret = CapnProto.FixedSizeList<T>.Create(owner, list);
            return ret;
        }

        public static CapnProto.FixedSizeList<T> CreateList<T>(CapnProtoMsg msg, IList<T> list) {

            return CreateList<T>(msg.Root, list);
        }

        public static bool SaveToStream<T>(Stream stream, T data, int messageSize) where T: struct, CapnProto.IPointer {
            if (stream == null || messageSize <= 0)
                return false;
            var node = NetByteArrayPool.GetByteBufferNode(NetByteArrayPool._cSmallBufferSize);
            data.CopyTo<T>(node.GetBuffer());
            stream.Write(node.GetBuffer(), 0, messageSize);
            node.Dispose();
            return true;
        }
#endif
    }
}


