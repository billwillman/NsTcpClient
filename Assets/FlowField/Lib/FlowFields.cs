using System.Collections;

namespace FlowFields
{
	public enum EDirection
	{
		ED_NULL,			//无;
		ED_U,				//上
		ED_D,				//下;
		ED_L,				//左;
		ED_R,				//右;
		ED_UL,				//上左;
		ED_UR,				//上右;
		ED_DL,				//下左;
		ED_DR				//下右;
	};

	public enum EGridType
	{
		EGT_NORMAL,			//普通网格;
		EGT_OPEN,			//open列表中的网格;
		EGT_CLOSE,			//close列表中的网格;
		EGT_OBSTOCLE,		//障碍物;
		EGT_DESTINATION,	//终点;
	};

	public interface IFlowFieldCell
	{
		// 流动方向
		EDirection Dir
		{
			get;
			set;
		}

		// 格子寻路的类型
		EGridType GType
		{
			get;
			set;
		}

		// 代价
		int CCost {
			get;
			set;
		}
	}

	public interface IFlowFieldMap
	{
		IFlowFieldCell GetCellAtXY (int x, int y);
		void GetCellCount (out int x, out int y);
	}

	public static class FlowFiledHelper
	{
		public static readonly int cINFI = 0xFFFFFFF;

		// 设置阻挡
		public static void SetObstocle(IFlowFieldMap map, int x, int y)
		{
			if (map == null || x < 0 || y < 0)
				return;
			int maxX, maxY;
			map.GetCellCount (maxX, maxY);
			if (maxX <= 0 || maxY <= 0 || x >= maxX || y >= maxY)
				return;
			var cell = map.GetCellAtXY (x, y);
			if (cell == null)
				return;
			cell.CCost = cINFI;
			cell.Dir = EDirection.ED_NULL;
			cell.GType = EGridType.EGT_OBSTOCLE;
		}
	}
}