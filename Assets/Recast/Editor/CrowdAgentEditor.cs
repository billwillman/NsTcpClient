using UnityEngine;
using UnityEditor;
using System.Collections;
using Recast;

[CustomEditor(typeof(Recast.CrowdMoveAgent))]
public class CrowdAgentEditor: Editor
{
    public override void OnInspectorGUI()
    {
        base.DrawDefaultInspector();
        if (EditorApplication.isPlaying)
        {
            Recast.CrowdMoveAgent moveAgent = this.target as Recast.CrowdMoveAgent;
            if (moveAgent != null)
            {
                bool m_IsStartAutoPath = moveAgent.m_IsDebugAutoPath;
                string str = m_IsStartAutoPath ? "关闭自动寻路" : "开启自动寻路";
                if (GUILayout.Button(str))
                {
                    m_IsStartAutoPath = !m_IsStartAutoPath;
                }
                moveAgent.m_IsDebugAutoPath = m_IsStartAutoPath;
                if (!moveAgent.m_IsDebugAutoPath)
                    moveAgent.StopMove();
                Vector3 vec = moveAgent.GetCurrVec();
                GUILayout.Label("当前速度: " + vec.ToString("f2"));
            }
        }
    }
}