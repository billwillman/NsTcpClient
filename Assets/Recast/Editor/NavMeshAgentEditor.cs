using UnityEngine;
using UnityEditor;
using System.Collections;
using Recast;

[CustomEditor(typeof(Recast.NavMeshAgent))]
public class NavMeshAgentEditor : Editor {
    public override void OnInspectorGUI()
    {
        base.DrawDefaultInspector();
        if (EditorApplication.isPlaying)
        {
             Recast.NavMeshAgent agent = this.target as Recast.NavMeshAgent;
             if (agent != null)
             {
                 if (GUILayout.Button("依附NavMesh"))
                 {
                     agent.AttachNavMesh();
                 }

                 bool m_IsStartAutoPath = agent.m_IsDebugAutoPath;
                 string str = m_IsStartAutoPath ? "关闭自动寻路" : "开启自动寻路";
                 if (GUILayout.Button(str))
                 {
                     m_IsStartAutoPath = !m_IsStartAutoPath;
                 }
                 agent.m_IsDebugAutoPath = m_IsStartAutoPath;
                 if (!agent.m_IsDebugAutoPath)
                     agent.StopAutoMove();

                 if (agent.IsAutoMoving)
                 {
                     GUILayout.Label("正在自动寻路中...");
                     float vec = agent.CurrVec;
                     GUILayout.Label("当前速度: " + vec.ToString("f2"));
                 }
             }
        }
    }
}
