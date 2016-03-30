using System;
using System.Collections.Generic;
using vMap.Voronoi.csDelaunay.Geom;

namespace vMap.Voronoi.csDelaunay.Delaunay
{
	[Serializable]
	public class Halfedge
	{
		#region Private Fields
		private static readonly Queue<Halfedge> _queue = new Queue<Halfedge>();
		#endregion

		#region Constructors
		public Halfedge(Edge edge, LR lr)
		{
			Init(edge, lr);
		}
		#endregion

		#region Public Properties
		public Halfedge EdgeListLeftNeighbor;
		public Halfedge EdgeListRightNeighbor;
		public Halfedge NextInPriorityQueue;
		public Edge Edge;
		public LR LeftRight;
		public Vertex Vertex;
		public float Ystar; // The vertex's y-coordinate in the transformed Voronoi space V
		#endregion

		#region Public Methods
		public static Halfedge Create(Edge edge, LR lr)
		{
			return _queue.Count > 0
				? _queue.Dequeue().Init(edge, lr)
				: new Halfedge(edge, lr);
		}
		public static Halfedge CreateDummy()
		{
			return Create(null, null);
		}
		public override string ToString()
		{
			return "Halfedge (LeftRight: " + LeftRight + "; vertex: " + Vertex + ")";
		}
		public void Dispose()
		{
			if (EdgeListLeftNeighbor != null || EdgeListRightNeighbor != null)
				return; // still in EdgeList

			if (NextInPriorityQueue != null)
				return; // still in PriorityQueue

			Edge = null;
			LeftRight = null;
			Vertex = null;
			_queue.Enqueue(this);
		}
		public void ReallyDispose()
		{
			EdgeListLeftNeighbor = null;
			EdgeListRightNeighbor = null;
			NextInPriorityQueue = null;
			Edge = null;
			LeftRight = null;
			Vertex = null;
			_queue.Enqueue(this);
		}
		public bool IsLeftOf(Vector2f p)
		{
			bool above;
			var topSite = this.Edge.RightSite;
			var rightOfSite = p.X > topSite.X;

			if (rightOfSite && (this.LeftRight == LR.LEFT))
				return true;

			if (!rightOfSite && (this.LeftRight == LR.RIGHT))
				return false;

			if (Math.Abs(this.Edge.A - 1) < Utilities.EPSILON)
			{
				var dyp = p.Y - topSite.Y;
				var dxp = p.X - topSite.X;
				var fast = false;

				if ((!rightOfSite && (this.Edge.B < 0)) || (rightOfSite && (this.Edge.B >= 0)))
				{
					above = dyp >= this.Edge.B * dxp;
					fast = above;
				}

				else
				{
					above = p.X + p.Y * Edge.B > Edge.C;
					if (this.Edge.B < 0) above = !above;
					if (!above) fast = true;
				}

				if (fast)
					return this.LeftRight == LR.LEFT ? above : !above;

				var dxs = topSite.X - Edge.LeftSite.X;
				above = Edge.B * (dxp * dxp - dyp * dyp) < dxs * dyp * (1 + 2 * dxp / dxs + Edge.B * Edge.B);
				if (Edge.B < 0)
					above = !above;
			}
			else
			{
				var y1 = this.Edge.C - this.Edge.A * p.X;
				var t1 = p.Y - y1;
				var t2 = p.X - topSite.X;
				var t3 = y1 - topSite.Y;
				above = t1 * t1 > t2 * t2 + t3 * t3;
			}
			return this.LeftRight == LR.LEFT ? above : !above;
		}
		#endregion

		#region Private Methods
		private Halfedge Init(Edge edge, LR lr)
		{
			this.Edge = edge;
			this.LeftRight = lr;
			this.NextInPriorityQueue = null;
			this.Vertex = null;

			return this;
		}
		#endregion
	}
}