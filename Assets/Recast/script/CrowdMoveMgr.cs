using UnityEngine;
using org.critterai.nav;
using org.critterai.u3d;

namespace Recast
{
    // 群跟随管理器
    public class CrowdMoveMgr: MonoBehaviour
    {
        private CrowdManager m_Mgr = null;

        public int m_MaxAgent = 100;
        public float m_MaxAgentRadius = 45f;
        public Navmesh m_NavMesh = null;
        private bool m_IsAppQuit = false;

        void OnApplicationQuit()
        {
            m_IsAppQuit = true;
        }

        public void RemoveAgent(CrowdMoveAgent agent)
        {
            if (m_IsAppQuit)
                return;

            if (agent == null)
                return;
            if (m_Mgr != null)
            {
                m_Mgr.RemoveAgent(agent.Agent);
            }
        }

        public void Init(Navmesh navMesh, int maxAgent = 100, float maxAgentRaidus = 45f)
        {
            m_MaxAgent = maxAgent;
            m_MaxAgentRadius = maxAgentRaidus;
            m_NavMesh = navMesh;
            Init();
        }

        public static CrowdMoveMgr Create(Navmesh navMesh, int maxAgent = 100, float maxAgentRaidus = 45f)
        {
            GameObject gameObj = new GameObject("CrowdMoveMgr", typeof(CrowdMoveMgr));
            CrowdMoveMgr mgr = gameObj.GetComponent<CrowdMoveMgr>();
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
