using System;
using System.Collections.Generic;

namespace vMap.Voronoi
{
	public class SiteComparer : IComparer<Site>
	{
		public int Compare(Site a, Site b)
		{
			return AStarSearch.Heuristic(a, b);
		}
	}
}