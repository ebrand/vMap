using System;

namespace vMap.Voronoi.csDelaunay.Delaunay
{
	[Serializable]
	public class LR
	{
		public static readonly LR LEFT = new LR("left");
		public static readonly LR RIGHT = new LR("right");
		private readonly string _name;

		public LR(string name)
		{
			_name = name;
		}

		public static LR Other(LR leftRight)
		{
			return leftRight == LEFT ? RIGHT : LEFT;
		}
		public override string ToString()
		{
			return _name;
		}
	}
}