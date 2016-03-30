using System;

namespace vMap.Voronoi
{
	public class GraphEdge<T, K>
	{
		public GraphVertex<T, K> Node1 { get; }
		public GraphVertex<T, K> Node2 { get; }
		public float Weight { get; set; }

		public GraphEdge(GraphVertex<T, K> node1, GraphVertex<T, K> node2, float weight)
		{
			this.Node1 = node1;
			this.Node2 = node2;
			this.Weight = weight;
		}
		public override int GetHashCode()
		{
			return this.Node1.GetHashCode() + this.Node2.GetHashCode();
		}
	}
}