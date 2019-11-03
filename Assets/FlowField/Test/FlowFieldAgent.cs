using UnityEngine;
using System.Collections;

namespace FlowFieldTest
{
	public class FlowFieldAgent : MonoBehaviour {
		public FlowFieldTestMap OwnMap = null;
		public int m_InitCellX = -1;
		public int m_InitCellY = -1;

		public int m_MovedCellX = -1;
		public int m_MovedCellY = -1;

		public void Awake()
		{
			RaycastHit hit;
			if (Physics.Raycast (this.transform.position + new Vector3 (0, 1, 0), new Vector3 (0, -1, 0), out hit) && (hit.collider != null) && (hit.collider.gameObject != null)) {
				var gameObj = hit.collider.gameObject;
				FlowFieldCell cell = gameObj.GetComponent<FlowFieldCell>();
				if (cell != null) {
					m_InitCellX = cell.W;
					m_InitCellY = cell.H;
					cell.StayActor ();
				}
			}
		}
	}
}
