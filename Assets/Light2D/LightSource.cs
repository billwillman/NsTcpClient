using System;
using System.Collections.Generic;
using UnityEngine;

namespace Light2D
{
    public class LightSource: MonoBehaviour
    {
        private Transform m_CacheTransform = null;

        public Color m_Color = Color.white;
        public float m_intensity = 1f;
        public float m_PhysicalRadius = 1f;
        public float m_SectorAngle = 0f;
        public float m_SectorSoftAngle = 0f;
        public float m_angle = 0f;
        private bool m_update = false;

        public le_BB m_BB;

        public float m_radius = 0f;
        public int m_res = 0;

        public Transform CacheTransform
        {
            get
            {
                if (m_CacheTransform == null)
                    m_CacheTransform = this.transform;
                return m_CacheTransform;
            }
        }

        public Vector2 Position
        {
            get
            {
                Vector3 p = CacheTransform.position;
                Vector2 ret = new Vector2(p.x, p.z);
                return ret;
            }
        }


    }
}
