using System;
using System.Collections.Generic;

namespace vMap.Voronoi
{
	public interface IWeightedGraph<T, out K, in O>
	{
		int Cost(T a, T b);
		IEnumerable<T> PrimaryNeighbors(O id);
		IEnumerable<K> SecondaryNeighbors(O id);
		HashSet<T> Visited { get; set; }
		HashSet<T> PrimaryWalls { get; set; }
	}
}