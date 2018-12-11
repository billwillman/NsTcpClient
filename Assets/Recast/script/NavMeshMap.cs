using System;
using System.IO;
using UnityEngine;
using Utils;
using org.critterai.nav;
using org.critterai.u3d;
using org.critterai.geom;
using org.critterai.interop;

namespace Recast
{

    public class PathBuffer
    {
        private uint[] m_Buffer = null;
        private int m_DataCount = 0;
        public void Init(uint[] buffer)
        {
            m_Buffer = buffer;
            m_DataCount = 0;
        }

        public uint[] Buffer
        {
            get
            {
                return m_Buffer;
            }
        }

        public void SetDataCount(int cnt)
        {
            m_DataCount = cnt;
        }

        public bool IsVaild()
        {
            return (m_Buffer != null) && (m_Buffer.Length > 0) && (m_DataCount > 0) && 
                    (m_DataCount <= m_Buffer.Length);
        }

        public int DataCount
        {
            get
            {
                return m_DataCount;
            }
        }
    }

    public class NavMeshMap : MonoBehaviour
    {
        private Navmesh m_NavMesh = null;
        private NavmeshQuery m_Query = null;
        private NavmeshQueryFilter m_DefaultFilter = null;
        private static NavMeshMap m_Map = null;
        private ObjectPool<PathBuffer> m_BufferMgr = null;

        private readonly static int m_MaxQueryNodeCount = 100;
        private readonly static int m_MaxPathBufferCount = 100;

        public static readonly Vector3 _cDefaultExtends = new Vector3(1f, 1f, 1f);

        public TextAsset m_MapAsset = null;

        void Awake()
        {
            if (m_Map == null)
                m_Map = this;
        }

        void Start()
        {
            if (m_MapAsset != null)
            {
                bool ret = LoadMapFromBuffer(m_MapAsset.bytes);
                if (!ret)
                    Debug.LogError("加載地图导航网格失败~!");
            }
        }

        public static NavmeshQueryFilter GetDefaultQueryFilter()
        {
            if (m_Map == null)
                return null;
            return m_Map.DefaultQueryFilter;
        }

        private PathBuffer CreateBuffer()
        {
            PathBuffer buffer = new PathBuffer();
            uint[] buf = new uint[m_MaxPathBufferCount];
            buffer.Init(buf);
            return buffer;
        }

        private void ResetBuffer(PathBuffer buffer)
        {
            if (buffer != null)
                buffer.SetDataCount(0);
        }

        private void InitBufferMgr()
        {
            if (m_BufferMgr == null)
            {
                m_BufferMgr = new ObjectPool<PathBuffer>();
                m_BufferMgr.Init(0, CreateBuffer, ResetBuffer);
            }
        }

        protected PathBuffer GetPathBuffer()
        {
            InitBufferMgr();
            PathBuffer ret = m_BufferMgr.GetObject();
            return ret;
        }

        public static void StopPathBuffer(PathBuffer buffer)
        {
            if (m_Map == null)
                return;
            m_Map.StorePathBuffer(buffer);
        }

        protected void StorePathBuffer(PathBuffer buffer)
        {
            if (buffer == null)
                return;
            InitBufferMgr();
            m_BufferMgr.Store(buffer);
        }
        

        protected NavmeshQueryFilter DefaultQueryFilter
        {
            get
            {
                if (m_DefaultFilter == null)
                    m_DefaultFilter = new NavmeshQueryFilter();
                return m_DefaultFilter;
            }
        }

        void OnDestroy()
        {
            if (m_DefaultFilter != null)
            {
                m_DefaultFilter.RequestDisposal();
                m_DefaultFilter = null;
            }
            Clear();
            m_Map = null;
        }

        public static PathCorridor CreateAgent(int maxPathSize = 10, int maxCorners = 10, NavmeshQueryFilter filter = null)
        {
            if (m_Map == null)
                return null;
            if (filter == null)
                filter = m_Map.DefaultQueryFilter;
            PathCorridor ret = new PathCorridor(maxPathSize, maxCorners, m_Map.Query, filter);
            return ret;
        }

        public static bool GetGroudPt(Vector3 source, Vector3 extends, out Vector3 result, NavmeshQueryFilter filter = null)
        {
            result = source;
            if (m_Map == null)
                return false;
            return m_Map._GetGroudPt(source, extends, out result, filter);
        }

        public static bool GetNavmeshPoint(Vector3 source, Vector3 extends, out NavmeshPoint point, NavmeshQueryFilter filter = null)
        {
            point = new NavmeshPoint();
            if (m_Map == null)
                return false;
            var query = m_Map.Query;
            if (query == null)
                return false;
            if (filter == null)
                filter = m_Map.DefaultQueryFilter;
            var status = query.GetNearestPoint(source, extends, filter, out point);
            if (NavUtil.Failed(status))
                return false;
            return true;
        }

        protected bool _GetGroudPt(Vector3 source, Vector3 extends, out Vector3 result, NavmeshQueryFilter filter = null)
        {
            result = source;
            var query = this.Query;
            if (query == null)
                return false;
            NavmeshPoint navPoint;
            if (filter == null)
                filter = this.DefaultQueryFilter;
            var status = query.GetNearestPoint(source, extends, filter, out navPoint);
            if (NavUtil.Failed(status))
                return false;
            result = navPoint.point;
            return true;
        }

        public static PathBuffer FindPath(Vector3 start, Vector3 end, Vector3 extends, NavmeshQueryFilter filter = null)
        {
            if (m_Map == null)
                return null;
            return m_Map._FindPath(start, end, extends, filter);
        }

        public PathBuffer _FindPath(Vector3 start, Vector3 end, Vector3 extends, NavmeshQueryFilter filter = null)
        {
            var query = this.Query;
            if (query == null)
                return null;
            if (filter == null)
                filter = this.DefaultQueryFilter;
            NavmeshPoint startPoint;
            var status = query.GetNearestPoint(start, extends, filter, out startPoint);
            if (NavUtil.Failed(status))
                return null;
            NavmeshPoint endPoint;
            status = query.GetNearestPoint(end, extends, filter, out endPoint);
            if (NavUtil.Failed(status))
                return null;

            PathBuffer pathBuf = GetPathBuffer();
            if (pathBuf == null || pathBuf.Buffer == null)
                return null;
            try
            {
                int dataCount;
                status = query.FindPath(ref startPoint, ref endPoint, extends, filter, pathBuf.Buffer, out dataCount);
                if (NavUtil.Failed(status))
                    return null;
                
                pathBuf.SetDataCount(dataCount);

                return pathBuf;
            } catch
            {
                StorePathBuffer(pathBuf);
            }
            return null;
        }

        public Navmesh GetNavmesh()
        {
            return m_NavMesh;
        }

        public NavmeshQuery Query
        {
            get
            {
                if (m_NavMesh == null || m_MaxQueryNodeCount <= 0)
                    return null;
                if (m_Query == null)
                {
                    var state = NavmeshQuery.Create(m_NavMesh, m_MaxQueryNodeCount, out m_Query);
                    if (!NavUtil.Succeeded(state))
                    {
                        if (m_Query != null)
                        {
                            m_Query.RequestDisposal();
                            m_Query = null;
                        }
                    }
                }
                return m_Query;
            }
        }

        public static bool LoadMap(byte[] buffer)
        {
            NavMeshMap map = GetInstance();
            return map.LoadMapFromBuffer(buffer);
        }

        public static NavMeshMap GetInstance()
        {
            if (m_Map == null)
            {
                GameObject gameObj = new GameObject("NavMeshMap", typeof(NavMeshMap));
                m_Map = gameObj.GetComponent<NavMeshMap>();
            }
            return m_Map;
        }

        private void Clear()
        {
            if (m_Query != null)
            {
                m_Query.RequestDisposal();
                m_Query = null;
            }
            if (m_NavMesh != null)
            {
                m_NavMesh.RequestDisposal();
                m_NavMesh = null;
            }
        }

        private bool LoadMapFromBuffer(byte[] buffer)
        {
            Clear();
            if (buffer == null || buffer.Length <= 0)
                return false;

            try
            {
                var status = Navmesh.Create(buffer, out m_NavMesh);
                var ret = NavUtil.Succeeded(status) && m_NavMesh != null;
                if (!ret)
                    Clear();
                return ret;
            } catch
            {
                return false;
            }
        }
    }
}
