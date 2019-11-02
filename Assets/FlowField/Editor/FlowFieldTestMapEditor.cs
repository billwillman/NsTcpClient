using UnityEditor;
using UnityEngine;
using System.Collections;
using FlowFieldTest;

[CustomEditor(typeof(FlowFieldTestMap))]
public class FlowFieldTestMapEditor : Editor {
	private int m_LastInstanceID = 0;
	private int m_XCnt = 0;
	private int m_ZCnt = 0;

	private void Init(FlowFieldTestMap map, bool ingoreDiff = false)
	{
		if (map != null) {
			if (ingoreDiff || (m_LastInstanceID != map.GetInstanceID()))
			{
				m_LastInstanceID = map.GetInstanceID ();
				map.GetWidthAndHeight (out m_XCnt, out m_ZCnt);
			}
		} else {
			m_LastInstanceID = 0;
			m_XCnt = 0;
			m_ZCnt = 0;
		}
	}

	public override void OnInspectorGUI ()
	{
		base.OnInspectorGUI ();
		FlowFieldTestMap map = this.target as FlowFieldTestMap;
		Init (map);
		if (map == null)
			return;
		
		EditorGUILayout.BeginHorizontal ();
		m_XCnt = EditorGUILayout.IntField ("X数量", m_XCnt);
		m_XCnt = Mathf.Max (m_XCnt, 0);
		m_ZCnt = EditorGUILayout.IntField ("Z数量", m_ZCnt);
		m_ZCnt = Mathf.Max (m_ZCnt, 0);
		EditorGUILayout.EndHorizontal ();

		EditorGUILayout.BeginHorizontal ();
		if (GUILayout.Button ("生成Cell")) {

			map.Build (m_XCnt, m_ZCnt);
			Init (map, true);
		}

		if (GUILayout.Button ("清理Cell")) {
			map.DestroyAllCell ();
			//Init (map, true);
		}
		EditorGUILayout.EndHorizontal ();
	}
}
