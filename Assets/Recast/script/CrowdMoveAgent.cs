using UnityEngine;
using System.Collections;
using org.critterai.nav;
using org.critterai.u3d;

namespace Recast
{
    // 跟随对象
    public class CrowdMoveAgent : MonoBehaviour
    {
        private CrowdAgent m_Agent = null;

        public CrowdMoveMgr m_Mgr = null;

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
