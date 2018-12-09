using UnityEngine;
using org.critterai.nav;
using org.critterai.u3d;

namespace Recast
{
    // 群跟随管理器
    public class CrowdMgr: MonoBehaviour
    {
        private CrowdManager m_Mgr = null;

        public int m_MaxAgent = 100;
        public float m_MaxAgentRadius = 45f;
        public Navmesh m_NavMesh = null;

        public void Init(Navmesh navMesh, int maxAgent = 100, float maxAgentRaidus = 45f)
        {
            m_MaxAgent = maxAgent;
            m_MaxAgentRadius = maxAgentRaidus;
            m_NavMesh = navMesh;
            Init();
        }

        public static CrowdMgr Create(Navmesh navMesh, int maxAgent = 100, float maxAgentRaidus = 45f)
        {
            GameObject gameObj = new GameObject("CrowdMgr", typeof(CrowdMgr));
            CrowdMgr mgr = gameObj.GetComponent<CrowdMgr>();
            mgr.Init(navMesh, maxAgent, maxAgentRaidus);
            return mgr;
        }

        public void Init()
        {
            Clear();
            m_Mgr = CrowdManager.Create(m_MaxAgent, m_MaxAgentRadius, m_NavMesh);
        }

        private void Clear()
        {
            if (m_Mgr != null)
            {
                m_Mgr.RequestDisposal();
                m_Mgr = null;
            }
        }

        void OnDestroy()
        {
            Clear();
        }

        void Update()
        {
            if (m_Mgr != null)
            {
                float delta = Time.unscaledDeltaTime;
                m_Mgr.Update(delta);
            }
        }
    }
}
