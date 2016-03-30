using System;
using System.Collections.Generic;

namespace vMap.Voronoi.csDelaunay.Delaunay
{
	public class EdgeReorderer
	{
		public List<Edge> Edges { get; private set; }
		public List<LR> EdgeOrientations { get; private set; }

		public EdgeReorderer(IReadOnlyList<Edge> origEdges, Type type)
		{
			this.Edges = new List<Edge>();
			this.EdgeOrientations = new List<LR>();
			if (origEdges.Count > 0)
			{
				this.Edges = this.ReorderEdges(origEdges, type);
			}
		}
		public void Dispose()
		{
			this.Edges = null;
			this.EdgeOrientations = null;
		}
		private List<Edge> ReorderEdges(IReadOnlyList<Edge> origEdges, Type type)
		{
			ICoord firstPoint;
			ICoord lastPoint;
			var n = origEdges.Count;
			var done = new List<bool>();
			var nDone = 0;
			var newEdges = new List<Edge>();
			var i = 0;
			var edge = origEdges[i];

			for (var b = 0; b < n; b++)
				done.Add(false);

			newEdges.Add(edge);
			this.EdgeOrientations.Add(LR.LEFT);
			

			if (type == typeof(Vertex))
			{
				firstPoint = edge.LeftVertex;
				lastPoint = edge.RightVertex;
			}
			else
			{
				firstPoint = edge.LeftSite;
				lastPoint = edge.RightSite;
			}

			if ((firstPoint == Vertex.VertexAtInfinity) || (lastPoint == Vertex.VertexAtInfinity))
				return new List<Edge>();

			done[i] = true;
			nDone++;

			while (nDone < n)
			{
				for (i = 1; i < n; i++)
				{
					if (done[i])
						continue;

					edge = origEdges[i];
					ICoord leftPoint;
					ICoord rightPoint;

					if (type == typeof(Vertex))
					{
						leftPoint = edge.LeftVertex;
						rightPoint = edge.RightVertex;
					}
					else
					{
						leftPoint = edge.LeftSite;
						rightPoint = edge.RightSite;
					}

					if ((leftPoint == Vertex.VertexAtInfinity) || (rightPoint == Vertex.VertexAtInfinity))
						return new List<Edge>();

					if (leftPoint == lastPoint)
					{
						lastPoint = rightPoint;
						this.EdgeOrientations.Add(LR.LEFT);
						newEdges.Add(edge);
						done[i] = true;
					}
					else if (rightPoint == firstPoint)
					{
						firstPoint = leftPoint;
						this.EdgeOrientations.Insert(0, LR.LEFT);
						newEdges.Insert(0, edge);
						done[i] = true;
					}
					else if (leftPoint == firstPoint)
					{
						firstPoint = rightPoint;
						this.EdgeOrientations.Insert(0, LR.RIGHT);
						newEdges.Insert(0, edge);
						done[i] = true;
					}
					else if (rightPoint == lastPoint)
					{
						lastPoint = leftPoint;
						this.EdgeOrientations.Add(LR.RIGHT);
						newEdges.Add(edge);
						done[i] = true;
					}

					if (done[i])
						nDone++;
				}
			}
			return newEdges;
		}
	}
}