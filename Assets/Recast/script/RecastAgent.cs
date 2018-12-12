using UnityEngine;
using System.Collections;
using org.critterai.nav;
using org.critterai.u3d;

namespace Recast
{
    // 比较复杂的寻路
    public class RecastAgent : MonoBehaviour
    {
        public NavmeshQueryFilter m_Filter = null;

        private float radius = 0.6f;						///< Agent radius. [Limit: >= 0]
        private float height = 2f;						///< Agent height. [Limit: > 0]
        private float maxAcceleration = 8.0f;				///< Maximum allowed acceleration. [Limit: >= 0]
        private float maxSpeed = 3.5f;						///< Maximum allowed speed. [Limit: >= 0]
                                                        ///
        /// Defines how close a collision element must be before it is considered for steering behaviors. [Limits: > 0]
        private float collisionQueryRange = 0.6f * 12f;

        private float pathOptimizationRange = 0.6f * 30f;		///< The path visibility optimization range. [Limit: > 0]

        /// How aggressive the agent manager should be at avoiding collisions with this agent. [Limit: >= 0]
        private float separationWeight = 2f;

        public enum NavMeshAgentSyncFlag
        {
            NONE = 0,
            NODE_TO_AGENT = 1,
            AGENT_TO_NODE = 2,
            NODE_AND_NODE = NODE_TO_AGENT | AGENT_TO_NODE,
        };

        private int _agentID = -1;
        private CrowdAgent _crowd = null;
    }
}
