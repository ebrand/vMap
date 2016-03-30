using System;
using System.Collections.Generic;

namespace vMap.Voronoi.csDelaunay.Geom
{
	public class Polygon
	{
		private readonly List<Vector2f> _vertices;

		public Polygon(List<Vector2f> vertices)
		{
			_vertices = vertices;
		}

		public float GetArea()
		{
			return Math.Abs(this.SignedDoubleArea() * 0.5f);
		}
		public Winding PolyWinding()
		{
			var signedDoubleArea = this.SignedDoubleArea();

			if (signedDoubleArea < 0)
				return Winding.CLOCKWISE;

			return signedDoubleArea > 0
				? Winding.COUNTERCLOCKWISE
				: Winding.NONE;
		}
		private float SignedDoubleArea()
		{
			var vertCount = _vertices.Count;
			float signedDoubleArea = 0;

			for (var i = 0; i < vertCount; i++)
			{
				var nextIndex = (i + 1) % vertCount;
				var point = _vertices[i];
				var next = _vertices[nextIndex];
				signedDoubleArea += (point.X * next.Y) - (next.X * point.Y);
			}
			return signedDoubleArea;
		}
	}
}