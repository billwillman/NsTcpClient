using UnityEngine;
using UnityEditor;
using System.Collections;
using org.critterai.nav;
using org.critterai.u3d;

public class FlowFieldDataExport : EditorWindow {

	public FlowFieldDataExport() {
        this.titleContent = new GUIContent("FlowField导出工具");
       

    }

    [MenuItem("FlowField/导出...")]
    public static void CreateWindow() {
        EditorWindow.GetWindow(typeof(FlowFieldDataExport));
    }
}
