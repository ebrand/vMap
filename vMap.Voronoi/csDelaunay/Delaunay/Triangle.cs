using System;
using System.Collections.Generic;

namespace vMap.Voronoi.csDelaunay.Delaunay
{
	[Serializable]
	public class Triangle
	{
		public List<Site> Sites { get; }
		public Triangle(Site a, Site b, Site c)
		{
			Sites = new List<Site> { a, b, c };
		}
		public void Dispose()
		{
			Sites.Clear();
		}
	}
}