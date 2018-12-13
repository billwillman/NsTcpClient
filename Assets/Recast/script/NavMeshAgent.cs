using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using org.critterai.nav;
using org.critterai.u3d;


namespace Recast
{
    public class NavMeshAgent: MonoBehaviour
    {

        public Vector3 m_Extends = new Vector3(1f, 1f, 1f);
        public NavmeshQueryFilter m_Filter = null;
        private Transform m_Trans = null;
        private PathCorridor m_Agent = null;
        private PathBuffer m_PathBuffer = null;
        public int m_MaxPathSize = 50;
        public int m_MaxCorners = 50;
        // 是否在自動寻路
        private bool m_IsAutoMoving = false;
        // 是否朝向根据移动方向移动
        public bool m_IsUseMoveForward = true;
      
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
                AttachNavMesh();
            }
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

            return true;
        }
        
        void UpdateAutoMoving()
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
            float t = 0.033f;
            m_CurrVec = m_CurrVec + m_Acc * t;
            if (m_CurrVec > m_MaxVec)
                m_CurrVec = m_MaxVec;

            int oldCount = data.cornerCount;
            var currentTarget = source + m_MoveDir * m_CurrVec * t;

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

        void Update()
        {
            UpdateAutoMoving();
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

        public void AttachNavMesh()
        {
            Vector3 source = CacheTransform.position;
            Vector3 result;
            if (NavMeshMap.GetGroudPt(source, m_Extends, out result, m_Filter))
            {
                CacheTransform.position = result;
            }

            if (m_Agent != null)
            {
                NavmeshPoint pt;
                if (NavMeshMap.GetNavmeshPoint(source, m_Extends, out pt, m_Filter))
                    m_Agent.Reset(pt);
            }
        }
    }
}
