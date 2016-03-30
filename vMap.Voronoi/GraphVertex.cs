using System;
using System.Collections.Generic;

namespace vMap.Voronoi
{
	public class GraphVertex<T, K>
	{
		public GraphVertex(T data) : this(data, null)
		{}
		public GraphVertex(T data, IEnumerable<GraphEdge<T, K>> edges)
		{
			this.Data = data;
			if(edges != null)
			{
				this.Edges = new Dictionary<int, GraphEdge<T, K>>();
				foreach(var e in edges)
					this.Edges.Add(e.GetHashCode(), e);
			}
			else
				this.Edges = new Dictionary<int, GraphEdge<T, K>>();

			this.CrossEdges = new Dictionary<int, GraphCrossEdge<T, K>>();
		}
		public T Data { get; }
		public Dictionary<int, GraphEdge<T, K>> Edges { get; }
		public Dictionary<int, GraphCrossEdge<T, K>> CrossEdges { get; set; }
		public void AddEdge(GraphEdge<T, K> graphEdge)
		{
			GraphEdge<T, K> foundEdge;
			if (!this.Edges.TryGetValue(graphEdge.GetHashCode(), out foundEdge))
				this.Edges.Add(graphEdge.GetHashCode(), graphEdge);
		}
		public void RemoveEdge(GraphEdge<T, K> graphEdge)
		{
			this.Edges.Remove(graphEdge.GetHashCode());
		}
		public void AddCrossEdge(GraphCrossEdge<T, K> crossEdge)
		{
			GraphCrossEdge<T, K> foundEdge;
			if(!this.CrossEdges.TryGetValue(crossEdge.GetHashCode(), out foundEdge))
				this.CrossEdges.Add(crossEdge.GetHashCode(), crossEdge);
		}
		public void RemoveCrossEdge(GraphCrossEdge<T, K> crossEdge)
		{
			this.CrossEdges.Remove(crossEdge.GetHashCode());
		}
	}
}