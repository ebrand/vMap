using System;

namespace vMap.Voronoi.csDelaunay.Geom
{
	[Serializable]
	public class Circle
	{
		public Vector2f Center;
		public float Radius;
		public Circle(float centerX, float centerY, float radius)
		{
			this.Center = new Vector2f(centerX, centerY);
			this.Radius = radius;
		}
		public override string ToString()
		{
			return "Circle (center: " + Center + "; radius: " + Radius + ")";
		}
	}
}