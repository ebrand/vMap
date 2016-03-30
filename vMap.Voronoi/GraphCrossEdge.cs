using System;

namespace vMap.Voronoi
{
	public class GraphCrossEdge<T, K>
	{
		public GraphVertex<T, K> PrimaryNode { get; set; }
		public GraphVertex<K, T> ConnectedNode { get; set; }

		public GraphCrossEdge(GraphVertex<T, K> primaryNode, GraphVertex<K, T> connectedNode)
		{
			this.PrimaryNode = primaryNode;
			this.ConnectedNode = connectedNode;
		}
	}
}