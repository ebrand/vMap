using System;
using System.Collections.Generic;
using vMap.Voronoi.csDelaunay.Geom;

namespace vMap.Voronoi.csDelaunay
{
	public class Utilities
	{
		public static readonly Random _random = new Random();
		public const float EPSILON = 0.005f;

		public static List<Vector2f> GetRandomPoints(int qty, Rectf bounds)
		{
			var result = new List<Vector2f>(qty);
			for(var x=0; x < bounds.Width; x++)
				for(var y = 0; y < bounds.Height; y++)
					result.Add(new Vector2f(_random.Next((int)bounds.Width + 1), _random.Next((int)bounds.Height + 1)));
			
			return result;
		} 
	}
}