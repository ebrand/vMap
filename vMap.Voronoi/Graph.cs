using System;
using System.Collections.Generic;

namespace vMap.Voronoi
{
	public class Graph<T, K, O> : IWeightedGraph<T, K, O>
	{
		public Dictionary<O, GraphVertex<T, K>> PrimaryGraph { get; set; }
		public Dictionary<O, GraphVertex<K, T>> SecondaryGraph { get; set; }
		public Graph()
		{
			this.PrimaryGraph   = new Dictionary<O, GraphVertex<T, K>>();
			this.SecondaryGraph = new Dictionary<O, GraphVertex<K, T>>();
			this.Visited        = new HashSet<T>();
			this.PrimaryWalls   = new HashSet<T>();
			this.SecondaryWalls = new HashSet<K>();
		}

		public HashSet<T> Visited { get; set; }
		public HashSet<T> PrimaryWalls { get; set; }
		public HashSet<K> SecondaryWalls { get; set; } 

		#region IWeightedGraph<O> Implementation
		public int Cost(T a, T b)
		{
			return this.Visited.Contains(b) ? 5 : 1;
		}
		public IEnumerable<T> PrimaryNeighbors(O key)
		{
			foreach (var edge in this.PrimaryGraph[key].Edges)
			{
				var neighbor = edge.Value.Node2.Data;
				if(!this.PrimaryWalls.Contains(neighbor))
				{
					yield return neighbor;
				}
			}
		}
		public IEnumerable<K> SecondaryNeighbors(O key)
		{
			foreach (var edge in this.SecondaryGraph[key].Edges)
			{
				var neighbor = edge.Value.Node2.Data;
				if (!this.SecondaryWalls.Contains(neighbor))
				{
					yield return neighbor;
				}
			}
		}
		#endregion

		public override string ToString()
		{
			return $"PG: {this.PrimaryGraph.Count} (walls: {this.PrimaryWalls.Count}), SG: {this.SecondaryGraph.Count} (walls: {this.SecondaryWalls.Count}), Visited: {this.Visited.Count}";
		}
	}
}