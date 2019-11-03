using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace FlowFieldTest
{

	//[ExecuteInEditMode]
	public class FlowFieldTestMap : MonoBehaviour {
		//private List<List<FlowFieldCell>> m_Cells = null;
		[SerializeField]
		private int m_W = 0;
		[SerializeField]
		private int m_H = 0;

		public int m_CellWidth = 1;
		public int m_CellHeight = 1;

		[SerializeField]
		private FlowFieldCell[] m_Cells = null;

		public FlowFieldCell m_InstanceCell = null;

		public void GetWidthAndHeight(out int w, out int h)
		{
			w = m_W;
			h = m_H;
		}

		public void DestroyAllCell()
		{
			if (m_Cells == null || m_Cells.Length <= 0) {
				m_W = 0;
				m_H = 0;
				return;
			}
			for (int idx = 0; idx < m_Cells.Length; ++idx)
			{
					FlowFieldCell cell = m_Cells [idx];
					if (cell != null) {
						#if UNITY_EDITOR
						GameObject.DestroyImmediate(cell.gameObject);
						#else
						GameObject.Destroy (cell.gameObject);
						#endif
						m_Cells [idx] = null;
					}
			}
			m_Cells = null;
			m_W = 0;
			m_H = 0;
		}

		public void Build(int width, int height)
		{
			int oldW, oldH;
			GetWidthAndHeight (out oldW, out oldH);
			if ((oldW == width) && (oldH == height))
				return;
			DestroyAllCell (); 
			if (m_InstanceCell == null) {
				Debug.LogError ("InstanceObj is null~!");
				return;
			}
			if ((width <= 0) || (height <= 0)) {
				return;
			}
			int allWidth = width * m_CellWidth;
			float halfAllW = (float)allWidth / 2.0f;
			int allHeight = height * m_CellHeight;
			float halfAllH = (float)allHeight / 2.0f;
			float halfW = (float)m_CellWidth / 2.0f;
			float halfH = (float)m_CellHeight / 2.0f;
			var trans = this.transform;
			var instObj = m_InstanceCell.gameObject;
			int id = 1;
			m_W = width;
			m_H = height;
			m_Cells = new FlowFieldCell[m_H * m_W];
			for (int h = 0; h < height; ++h) {
				for (int w = 0; w < width; ++w) {
					GameObject gameObj = GameObject.Instantiate (instObj);
					gameObj.SetActive (true);
					var child = gameObj.transform;
					child.SetParent (trans, false);
					child.localRotation = instObj.transform.rotation;
					child.localScale = instObj.transform.lossyScale;
					child.localPosition = new Vector3(-halfAllW + halfW + w * m_CellWidth, 0, -halfAllH + halfH + h * m_CellHeight);
					var cell = gameObj.GetComponent<FlowFieldCell> ();
					cell.Cost = 0;
					cell.W = w; cell.H = h;
					cell.ID = id++;
					m_Cells [h * m_W + w] = cell;
				}
			}
		}
	}

}
