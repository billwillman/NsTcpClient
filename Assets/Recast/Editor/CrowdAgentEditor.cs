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
                Vector3 vec = moveAgent.GetCurrVec();
                GUILayout.Label("当前速度: " + vec.ToString("f2"));
            }
        }
    }
}