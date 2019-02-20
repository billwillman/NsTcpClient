using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using org.critterai.nav;
using org.critterai.u3d;


namespace Recast
{

    public class CrowdAgentAnimation
    {
        public bool active;
	    public Vector3 initPos, startPos, endPos;
	    public uint polyRef;
	    public float t, tmax;
    }

    public class NavMeshAgent: MonoBehaviour
    {

        public Vector3 m_Extends = new Vector3(1f, 1f, 1f);
        public float m_Radius = 1f;
        public NavmeshQueryFilter m_Filter = null;
        private Transform m_Trans = null;
        private PathCorridor m_Agent = null;
        private PathBuffer m_PathBuffer = null;
        public int m_MaxPathSize = 50;
        public int m_MaxCorners = 50;
        // 是否在自動寻路
        private bool m_IsAutoMoving = false;
        private byte m_Flags = 0;
        // 是否朝向根据移动方向移动
        public bool m_IsUseMoveForward = true;


        public CrowdAgentState m_State = CrowdAgentState.Invalid;
        private CrowdAgentAnimation m_AgentAnim = new CrowdAgentAnimation();

        // 初始速度
        public float m_Vec = 10f;
        // 最大速度
        public float m_MaxVec = 10f;
        // 加速度
        public float m_Acc = 0f;
        // 当前速度
        private float m_CurrVec = 0f;
        private Vector3 m_MoveDir = Vector3.zero;

        public bool m_IsFixedXZ = false;

        public float CurrVec
        {
            get
            {
                return m_CurrVec;
            }
        }

        public bool IsAutoMoving
        {
            get
            {
                return m_IsAutoMoving;
            }
        }

        void FreeFilter()
        {
            if (m_Filter != null)
            {
                m_Filter.RequestDisposal();
                m_Filter = null;
            }
        }

        void FreePathBuffer()
        {
            if (m_PathBuffer != null)
            {
                NavMeshMap.StopPathBuffer(m_PathBuffer);
                m_PathBuffer = null;
            }

            m_IsAutoMoving = false;
            m_MoveDir = Vector3.zero;
            m_CurrVec = m_Vec;
            m_State = CrowdAgentState.Invalid;
        }
        
        void FreeAgent()
        {
            if (m_Agent != null)
            {
                m_Agent.RequestDisposal();
                m_Agent = null;
            }
        }

        void OnDestroy()
        {
            FreePathBuffer();
            FreeAgent();
            FreeFilter();
        }

        public Transform CacheTransform
        {
            get
            {
                if (m_Trans == null)
                    m_Trans = this.transform;
                return m_Trans;
            }
        }

        private bool InitAgent()
        {
            bool isNew = false;
            if (m_Agent == null)
            {
                isNew = true;
                m_Agent = NavMeshMap.CreateAgent(m_MaxPathSize, m_MaxCorners, m_Filter);
            }
            bool ret = m_Agent != null;
            if (isNew && ret)
            {
                AttachNavMesh();
            }

            return ret;
        }

        // 停止自动寻路
        public void StopAutoMove()
        {
            if (m_PathBuffer != null)
            {
                FreePathBuffer();
                NavmeshPoint pt;
                // 检查
                if (AttachNavMesh(out pt))
                {
                    NavmeshConnection conn;
                    if (NavMeshMap.GetConnection(pt.polyRef, out conn))
                    {
                        
                    }
                }
            }
        }

        // 判斷是否到了OffMesh连接点
        public bool overOffmeshConnection()
        {
            return overOffmeshConnection(m_Radius);
        }

        private bool overOffmeshConnection(float radius)
        {
            if (m_Agent == null || m_Agent.Corners == null ||
                m_Agent.Corners.cornerCount <= 0 || m_Agent.Corners.flags == null ||
                 m_Agent.Corners.flags.Length <= 0)
                return false;

            WaypointFlag flag = m_Agent.Corners.flags[m_Agent.Corners.flags.Length - 1];
            if ((flag & WaypointFlag.OffMesh) != 0)
            {
                var endData = m_Agent.Corners[m_Agent.Corners.cornerCount - 1];
                if (endData == null)
                    return false;
                Vector3 vv = m_Agent.Position.point - endData.point;

                float dist2 = vv.x * vv.x + vv.y * vv.y + vv.z * vv.z;
                if (dist2 <= radius * radius)
                    return true;
            }

            return false;
        }

        public bool MoveToTarget(Vector3 targetPt)
        {
            if (!InitAgent())
                return false;

            FreePathBuffer();
            Vector3 startPt = CacheTransform.position;
            
            NavmeshPoint pt;
            if (!NavMeshMap.GetNavmeshPoint(startPt, m_Extends, out pt, m_Filter))
                return false;
            NavmeshPoint target;
            if (!NavMeshMap.GetNavmeshPoint(targetPt, m_Extends, out target, m_Filter))
                return false;
            m_PathBuffer = NavMeshMap.FindPath(pt.point, target.point, m_Extends, m_Filter);
            if (m_PathBuffer == null || !m_PathBuffer.IsVaild())
                return false;

            m_Agent.Reset(pt);
            m_Agent.SetCorridor(target.point, m_PathBuffer.Buffer, m_PathBuffer.DataCount);

            var data = m_Agent.Corners;
            if (data == null)
                return false;

            m_CurrVec = m_Vec;
            m_IsAutoMoving = true;
            m_State = CrowdAgentState.OnMesh;

            return true;
        }
        
        void UpdateAutoMoving(float delta)
        {
            if (!m_IsAutoMoving || m_Agent == null)
                return;
            var data = m_Agent.Corners;
            if (data == null)
                return;
            if (data.cornerCount <= 0)
            {
                StopAutoMove();
#if UNITY_EDITOR
                Debug.Log("寻路结束");
#endif
                // 檢測OffLink
                return;
            }

            NavmeshPoint pt = data[0];
            Vector3 target = pt.point;
            var trans = CacheTransform;
            Vector3 source = trans.position;
            Vector3 dist = target - source;

            // 移动速度
            if (m_MoveDir.sqrMagnitude <= float.Epsilon)
            {
                // 初始化

                // 方向
                m_MoveDir = dist;
                m_MoveDir.Normalize();

                // 角度
                if (m_IsUseMoveForward)
                {
                    var forward = m_MoveDir;
                    if (m_IsFixedXZ)
                        forward.y = trans.forward.y;
                    trans.forward = forward;
                }
            }

            // 计算速度
           // float t = 0.033f;
            m_CurrVec = m_CurrVec + m_Acc * delta;
            if (m_CurrVec > m_MaxVec)
                m_CurrVec = m_MaxVec;

            int oldCount = data.cornerCount;
            var currentTarget = source + m_MoveDir * m_CurrVec * delta;

            // 判断是否超过
            var a1 = currentTarget - target;
            float detlaAngle = Vector3.Dot(a1, m_MoveDir);
         //   Debug.LogErrorFormat("Angle: {0}", detlaAngle.ToString());
            if (detlaAngle > 0)
            {
                currentTarget = target;
            }

            var targetPt = m_Agent.MovePosition(currentTarget);
            trans.position = targetPt.point;
            if (data.cornerCount != oldCount)
            {
                // 需要重新初始化一次
                m_MoveDir = Vector3.zero;
            }
        }

        private uint[] m_TmpRefs = new uint[2];
        void CheckOffMeshConnection()
        {
            if (m_Agent == null)
                return;

            if (overOffmeshConnection())
            {
                uint refId = m_Agent.Corners.polyRefs[m_Agent.Corners.cornerCount - 1];
               // m_Agent.MoveOverConnection(refId, m_TmpRefs, )
                UnityEngine.Vector3 startPos = new Vector3();
                UnityEngine.Vector3 endPos = new Vector3();
                if (m_Agent.MoveOverConnection(refId, m_TmpRefs, startPos, endPos))
                {
                    if (NavMeshMap.GetConnectionEndpoints(m_TmpRefs[0], m_TmpRefs[1], out startPos, out endPos))
                    {
                        NavmeshPoint pt;
                        if (AttachNavMesh(out pt))
                        {
                            m_AgentAnim.initPos = m_Agent.Position.point;
                            m_AgentAnim.startPos = startPos;
                            m_AgentAnim.endPos = endPos;
                            m_AgentAnim.polyRef = m_TmpRefs[1];
                            m_AgentAnim.active = true;
                            m_AgentAnim.t = 0f;
                            m_AgentAnim.tmax = ((m_AgentAnim.endPos - m_AgentAnim.startPos).magnitude / m_MaxVec) * 0.5f;

                            FreePathBuffer();

                            m_State = CrowdAgentState.OffMesh;
                        }
                    }
                }
            }
        }

        void UpdateAgentAnim(float delta)
        {
            if (m_AgentAnim == null)
                return;
            if (!m_AgentAnim.active)
                return;
            m_AgentAnim.t += delta;
            if (m_AgentAnim.t > m_AgentAnim.tmax)
            {
                m_AgentAnim.active = false;
                m_State = CrowdAgentState.OnMesh;
                return;
            }

            float ta = m_AgentAnim.tmax * 0.15f;
            float tb = m_AgentAnim.tmax;
            if (m_AgentAnim.t < ta)
            {
                float u = tween(m_AgentAnim.t, ta, tb);
                CacheTransform.position = Vector3.Lerp(m_AgentAnim.initPos, m_AgentAnim.startPos, u);
            } else
            {
                float u = tween(m_AgentAnim.t, ta, tb);
                CacheTransform.position = Vector3.Lerp(m_AgentAnim.startPos, m_AgentAnim.endPos, u);
            }
        }

        private static float tween(float t, float t0, float t1)
        {
            return Mathf.Clamp((t - t0) / (t1 - t0), 0.0f, 1.0f);
        }

        void Update()
        {
            float timeDelta = 0.033f;
            UpdateAutoMoving(timeDelta);
            CheckOffMeshConnection();
            UpdateAgentAnim(timeDelta);
        }

#if UNITY_EDITOR

        [System.NonSerialized]
        public bool m_IsDebugAutoPath = false;

        void OnGUI()
        {
           if (m_IsDebugAutoPath)
           {
               if (Input.GetMouseButton(0))
               {
                   var mainCam = Camera.main;
                   if (mainCam != null)
                   {
                       Vector3 mousePt = Input.mousePosition;
                       Ray ray = mainCam.ScreenPointToRay(mousePt);
                       var mask = LayerMask.GetMask("NavMesh");
                       RaycastHit hit;
                       if (Physics.Raycast(ray, out hit, 1000f, mask) && hit.collider != null)
                       {
                           Vector3 endPt = hit.point;
                           this.MoveToTarget(endPt);
                       }
                   }
               }
           }
        }

#endif

        public bool AttachNavMesh(out NavmeshPoint pt)
        {
            Vector3 source = CacheTransform.position;
            if (NavMeshMap.GetNavmeshPoint(source, m_Extends, out pt, m_Filter))
            {
                CacheTransform.position = pt.point;
                if (m_Agent != null)
                {
                    m_Agent.Reset(pt);
                    return true;
                }
            }
            return false;
        }

        public void AttachNavMesh()
        {
            NavmeshPoint pt;
            AttachNavMesh(out pt);
        }
    }
}
