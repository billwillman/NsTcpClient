using UnityEngine;
using System.Collections;

namespace FlowFieldTest
{

	public class FlowFieldCell : MonoBehaviour {

		public static readonly int m_ActorStayCost = 255;

		public int Cost = 0;
		public TextMesh Text;
		public int W = 0;
		public int H = 0;
		public int ID = 0;

		private void UpdateTextMesh()
		{
		}

		public void StayActor()
		{
			Cost += m_ActorStayCost;
			UpdateTextMesh ();
		}

		public void UnStayActor()
		{
			Cost -= m_ActorStayCost;
			UpdateTextMesh ();
		}
	}

}
