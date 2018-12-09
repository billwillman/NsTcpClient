using UnityEngine;
using org.critterai.nav;
using org.critterai.u3d;

namespace Recast
{
    public class NavMeshAgent: MonoBehaviour
    {
        // 移动到
        public void MoveTo(Vector3 target)
        {

        }

        // 设置位置
        public bool SetPosition(Vector3 pos)
        {
            var map = NavMeshMap.GetInstance();
            if (map == null)
                return false;

            

            return true;
        }
    }
}
