using System;
using System.Drawing;

namespace vMap.Voronoi
{
	public class Coordinate
	{
		public PointF Point { get; set; }
		public float Weight { get; set; }

		public Coordinate(float x, float y, float weight)
		{
			this.Point = new PointF(x, y);
			this.Weight = weight;
		}
		public override string ToString()
		{
			return $"X: {this.Point.X}, Y: {this.Point.Y}, Weight: {this.Weight}";
		}
	}
}