using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.IO;

namespace NsTcpClient {
    
    // 网路数据池
    public static class NetByteArrayPool {
        private static RecyclableMemoryStreamManager m_Mgr = null;
        private static string _cRcyclableTag = "NetByteArrayPool";
        private static int _cSmallBufferSize = 1 * 1024;
        private static int _cLargeBufSize = 64 * 1024;
        public static void InitMgr() {
            if (m_Mgr != null)
                return;
            m_Mgr = new RecyclableMemoryStreamManager(_cSmallBufferSize, _cLargeBufSize, _cLargeBufSize);
        }

        public static MemoryStream GetBuffer(int bufSize) {
            if (bufSize <= 0)
                return null;
            InitMgr();
            return m_Mgr.GetStream(_cRcyclableTag, bufSize);
        }

        public static void FreeBuffer(MemoryStream stream) {
            if (stream == null || stream.Length <= 0)
                return;
            InitMgr();
            stream.Dispose();
            stream = null;
        }
    }
}
