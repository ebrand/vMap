using System;
using System.Collections.Generic;
using System.Linq;
using vMap.Voronoi.csDelaunay.Delaunay;

namespace vMap.Voronoi.csDelaunay.Geom
{
	[Serializable]
	public class LineSegment
	{
		public LineSegment(Vector2f p0, Vector2f p1)
		{
			this.P0 = p0;
			this.P1 = p1;
		}

		public Vector2f P0;
		public Vector2f P1;

		public static List<LineSegment> VisibleLineSegments(List<Edge> edges)
		{
			return (
				from edge in edges
					where edge.IsVisible()
					&& !float.IsNaN(edge.ClippedEnds[LR.LEFT].X)
					&& !float.IsNaN(edge.ClippedEnds[LR.RIGHT].Y)

					let p1 = edge.ClippedEnds[LR.LEFT]
					let p2 = edge.ClippedEnds[LR.RIGHT]

				select new LineSegment(p1, p2)
			).ToList();
		}
		public static float CompareLengths_MAX(LineSegment segment0, LineSegment segment1)
		{
			var length0 = (segment0.P0 - segment0.P1).Magnitude;
			var length1 = (segment1.P0 - segment1.P1).Magnitude;

			if (length0 < length1)
				return 1;

			if (length0 > length1)
				return -1;

			return 0;
		}
		public static float CompareLengths(LineSegment edge0, LineSegment edge1)
		{
			return -CompareLengths_MAX(edge0, edge1);
		}
		public override string ToString()
		{
			return $"{P0.X},{P0.Y} - {P1.X},{P1.Y}";
		}
	}
}