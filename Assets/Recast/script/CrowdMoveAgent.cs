using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using org.critterai.nav;
using org.critterai.u3d;

namespace Recast
{

    // 跟随对象
    public class CrowdMoveAgent : MonoBehaviour
    {
        private CrowdAgent m_Agent = null;
        private int m_ID = -1;
        private CrowdMoveMgr m_Mgr = null;
        private CrowdAgentParams m_Params;
        private LinkedListNode<CrowdAgent> m_Node = null;
        private NavmeshQueryFilter m_Filter = null;
        private Transform m_CacheTransform = null;

        public NavMeshAgent m_Target = null;

        public Vector3 GetCurrVec()
        {
            if (!IsInited)
                return Vector3.zero;
            return m_Agent.Velocity;
        }

        protected Transform CacheTransform
        {
            get
            {
                if (m_CacheTransform == null)
                    m_CacheTransform = this.transform;
                return m_CacheTransform;
            }
        }

        internal NavmeshQueryFilter GetFilter()
        {
            return m_Filter;
        }

        public float m_MaxSpeed = 10f;
        public float m_Radius = 1f;
        public float m_Height = 1f;
        public float m_MaxAcc = 0f;
        public float m_SeparationWeight = 1f;

        // 自动转向
        public bool m_IsAnticipateTurns = true;
        // 避开障碍物
        public bool m_IsObstacleAvoidance = true;
        // 隔离人群
        public bool m_IsCrowdSeparation = true;
        // 是否开启优化显示
        public bool m_IsOptimizeVis = true;
        // 是否优化拓扑
        public bool m_IsOptimizeTopo = true;
        // 对应CrowdMoveMgr的Param
        public byte m_AvoidanceType = 0;

        public float m_CollisionQueryRange = 1f;

        public Vector3 Extends
        {
            get
            {
                return new Vector3(m_Radius, m_Height, m_Radius);
            }
        }

        internal void _SetId(int id)
        {
            m_ID = id;
        }

        internal void _SetAgent(CrowdAgent agent)
        {
            m_Agent = agent;
        }

        private void InitCrowdMoveMgr()
        {
            var trans = this.transform;
            if (m_Mgr == null && trans.parent != null)
            {
                m_Mgr = trans.parent.GetComponent<CrowdMoveMgr>();
            }
        }

        private void InitParams()
        {
            m_Params = new CrowdAgentParams();
            m_Params.maxSpeed = m_MaxSpeed;
            m_Params.radius = m_Radius;
            m_Params.height = m_Height;
            m_Params.maxAcceleration = m_MaxAcc;
            m_Params.separationWeight = m_SeparationWeight;
            m_Params.avoidanceType = m_AvoidanceType;
            m_Params.collisionQueryRange = m_CollisionQueryRange;
            
            int flags = 0;
            if (m_IsAnticipateTurns)
                flags |= (int)CrowdUpdateFlags.AnticipateTurns;
            if (m_IsObstacleAvoidance)
                flags |= (int)CrowdUpdateFlags.CrowdSeparation;
            if (m_IsOptimizeVis)
                flags |= (int)CrowdUpdateFlags.ObstacleAvoidance;
            if (m_IsOptimizeTopo)
                flags |= (int)CrowdUpdateFlags.OptimizeTopo;

            m_Params.updateFlags = (CrowdUpdateFlags)flags;
        }

        internal bool HasListNode
        {
            get
            {
                return m_Node != null;
            }
        }

        internal LinkedListNode<CrowdAgent> ListNode
        {
            get
            {
                if (m_Node == null && m_Agent != null)
                    m_Node = new LinkedListNode<CrowdAgent>(m_Agent);
                return m_Node;
            }
        }

        internal CrowdAgentParams GetParams()
        {
            return m_Params;
        }

        private bool AddAgent()
        {
            if (m_Mgr != null)
            {
                return m_Mgr.AddAgent(this);
            }
            return false;
        }

        private void CheckID()
        {
            if (m_ID == -1)
            {
                if (!AddAgent())
                    m_ID = -1;
            }
        }

        public bool IsInited
        {
            get
            {
                return m_ID != -1 && m_Agent != null;
            }
        }


        private bool Follow(NavmeshPoint pt)
        {
            if (!IsInited || !NavMeshMap.IsVaild)
                return false;
            return m_Agent.RequestMoveTarget(pt);
        }

        private bool Follow(Vector3 pos)
        {
            if (!IsInited || !NavMeshMap.IsVaild)
                return false;
            NavmeshPoint pt;
            if (!NavMeshMap.GetNavmeshPoint(pos, this.Extends, out pt, GetFilter()))
                return false;
            return Follow(pt);
        }

        void UpdateFollow()
        {
            if (!IsInited)
                return;
            if (m_Target != null)
            {
                Follow(m_Target.transform.position);
            }
        }

        void UpdatePosition()
        {
            if (!IsInited)
                return;
            var trans = this.CacheTransform;
            trans.position = m_Agent.Position;
        }

        void Update()
        {
            CheckID();
            UpdateFollow();
            UpdatePosition();
        }

        // 初始化
        private void Init()
        {
            InitParams();
            InitCrowdMoveMgr();
            
        }

        void Start()
        {
            Init();
        }

        internal CrowdAgent Agent
        {
            get
            {
                return m_Agent;
            }
        }

        private void ClearAgent()
        {
            if (m_Agent != null)
            {
                if (m_Mgr != null)
                {
                    m_Mgr.RemoveAgent(this);
                }
                m_Agent = null;
            }
        }

        void OnDestroy()
        {
            ClearAgent();
        }
    }
}
