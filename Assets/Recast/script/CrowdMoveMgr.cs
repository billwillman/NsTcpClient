using UnityEngine;
using System.Collections.Generic;
using org.critterai.nav;
using org.critterai.u3d;

namespace Recast
{

    public enum CrowdQuality
    {
        High = 0,
        Good,
        Mid,
        Low
    }

    // 群跟随管理器
    public class CrowdMoveMgr: MonoBehaviour
    {
        private CrowdManager m_Mgr = null;

        private static int m_GlobalId = -1;

        public int m_MaxAgent = 100;
        public float m_MaxAgentRadius = 45f;
        private Navmesh m_NavMesh = null;
        private bool m_IsAppQuit = false;
        private LinkedList<CrowdAgent> m_AgentList = new LinkedList<CrowdAgent>();

        // 质量
        public CrowdQuality m_DefaultQuality = CrowdQuality.High;
        private Dictionary<int, int> m_QualiyHash = new Dictionary<int,int>();

        // 创建Agent ID
        internal static int NewGlobalId()
        {
            return ++m_GlobalId;
        }

        void OnApplicationQuit()
        {
            m_IsAppQuit = true;
        }

        internal bool AddAgent(CrowdMoveAgent agent)
        {
            if (m_IsAppQuit || agent == null || m_Mgr == null)
                return false;
            var param = agent.GetParams();
            var pt = agent.transform.position;
            NavmeshPoint pp;
            Vector3 extends = agent.Extends;
            if (!NavMeshMap.GetNavmeshPoint(pt, extends, out pp, agent.GetFilter()))
                return false;
            agent.transform.position = pp.point;
            CrowdAgent a = m_Mgr.AddAgent(pp.point, param);
            agent._SetAgent(a);
            agent._SetId(NewGlobalId());
            var node = agent.ListNode;
            var list = node.List;
            if (list != m_AgentList)
            {
                if (list != null)
                    list.Remove(node);
                m_AgentList.AddLast(node);
            }
            return true;
        }

        internal void RemoveAgent(CrowdMoveAgent agent)
        {
            if (m_IsAppQuit || agent == null)
                return;

            if (agent == null)
                return;

            if (agent.HasListNode)
            {
                var node = agent.ListNode;
                m_AgentList.Remove(node);
            }

            if (m_Mgr != null)
            {
                m_Mgr.RemoveAgent(agent.Agent);
            }
        }

        private void _Init(Navmesh navMesh)
        {
            m_NavMesh = navMesh;
            Init();
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

        public int AddQuailty(CrowdQuality q)
        {
            return this.AddQuailty(q, true);
        }

        private int AddQuailty(CrowdQuality q, bool isCheckExist)
        {
            if (m_Mgr == null)
                return -1;
            int key = (int)q;
            int value;
            if (isCheckExist)
            {
                if (m_QualiyHash.TryGetValue(key, out value))
                    return value;
            }
            CrowdAvoidanceParams param;
            switch (q)
            {
                case CrowdQuality.High:
                    param = CrowdAvoidanceParams.CreateStandardHigh();
                    break;
                case CrowdQuality.Good:
                    param = CrowdAvoidanceParams.CreateStandardGood();
                    break;
                case CrowdQuality.Mid:
                    param = CrowdAvoidanceParams.CreateStandardMedium();
                    break;
                case CrowdQuality.Low:
                    param = CrowdAvoidanceParams.CreateStandardLow();
                    break;
                default:
                    return -1;
            }

            int index = m_QualiyHash.Count;
            if (!m_Mgr.SetAvoidanceConfig(index, param))
            {
#if UNITY_EDITOR
                Debug.LogError("SetAvoidanceConfig error~!");
#endif
                return -1;
            }

            if (!m_QualiyHash.ContainsKey(key))
                m_QualiyHash.Add(key, index);
            return index;
        }

        public void Init()
        {
            Clear();
            m_Mgr = CrowdManager.Create(m_MaxAgent, m_MaxAgentRadius, m_NavMesh);
            AddQuailty(m_DefaultQuality, false);
        }

        private void Clear()
        {
            if (m_Mgr != null)
            {
                var node = m_AgentList.First;
                while (node != null)
                {
                    var next = node.Next;
                    if (node.Value != null)
                    {
                        m_Mgr.RemoveAgent(node.Value);
                    }
                    node = next;
                }

                m_Mgr.RequestDisposal();
                m_Mgr = null;
            }

            m_AgentList.Clear();
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
            } else
            {
                if (NavMeshMap.IsVaild)
                {
                    var mesh = NavMeshMap.GetNavMesh();
                    if (mesh != null)
                        this._Init(mesh);
                }
            }
        }
    }
}
