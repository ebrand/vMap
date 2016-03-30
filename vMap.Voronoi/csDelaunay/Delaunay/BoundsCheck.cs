using System;
using vMap.Voronoi.csDelaunay.Geom;

namespace vMap.Voronoi.csDelaunay.Delaunay
{
	public class BoundsCheck
	{
		public const int TOP = 1;
		public const int BOTTOM = 2;
		public const int LEFT = 4;
		public const int RIGHT = 8;

		public static int Check(Vector2f point, Rectf bounds)
		{
			var value = 0;
			if (Math.Abs(point.X - bounds.Left) < Utilities.EPSILON)
				value |= BoundsCheck.LEFT;

			if (Math.Abs(point.X - bounds.Right) < Utilities.EPSILON)
				value |= BoundsCheck.RIGHT;

			if (Math.Abs(point.Y - bounds.Top) < Utilities.EPSILON)
				value |= BoundsCheck.TOP;

			if (Math.Abs(point.Y - bounds.Bottom) < Utilities.EPSILON)
				value |= BoundsCheck.BOTTOM;

			return value;
		}
	}
}