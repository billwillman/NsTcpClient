using UnityEngine;
using org.critterai.nav;
using org.critterai.u3d;

namespace Recast
{
    public class NavMeshMap : MonoBehaviour
    {
        private Navmesh m_NavMesh = null;
        private static NavMeshMap m_Map = null;

        void OnDestroy()
        {
            Clear();
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
