using System;
using System.Collections.Generic;
using vMap.Voronoi.csDelaunay.Geom;

namespace vMap.Voronoi.csDelaunay.Delaunay
{
	[Serializable]
	public class Edge
	{
		// The line segment connecting the two Sites is part of the Delaunay triangulation
		// The line segment connecting the two Vertices is part of the Voronoi diagram

		#region Private Fields
		private static readonly Queue<Edge> _queue = new Queue<Edge>();
		private static int _nEdges = 0;
		private Dictionary<LR, Site> _sites; // The two input Sites for which this Edge is a bisector:
		#endregion

		#region Constructors
		public Edge()
		{
			this.EdgeIndex = _nEdges++;
			this.Init();
		}
		#endregion

		#region Public Properties
		public static readonly Edge DELETED = new Edge();
		public float A; // The equation of the edge: ax + by = c
		public float B;
		public float C;
		public int EdgeIndex { get; }
		// Once clipVertices() is called, this Dictionary will hold two Points
		// representing the clipped coordinates of the left and the right ends...
		public Dictionary<LR, Vector2f> ClippedEnds { get; private set; }
		public Vertex LeftVertex { get; private set; } // The two Voronoi vertices that the edge connects (if one of them is null, the edge extends to infinity)
		public Vertex RightVertex { get; private set; }
		public Site LeftSite { get { return _sites[LR.LEFT]; } set { _sites[LR.LEFT] = value; } }
		public Site RightSite { get { return _sites[LR.RIGHT]; } set { _sites[LR.RIGHT] = value; } }
		#endregion

		#region Public Methods
		public static List<Edge> GetSelectEdgesForSitePoint(Vector2f coord, List<Edge> edgesToTest)
		{
			return edgesToTest.FindAll(
				delegate (Edge e)
				{
					if (e.LeftSite != null)
						if (e.LeftSite.Coordinate == coord) return true;

					if (e.RightSite != null)
						if (e.RightSite.Coordinate == coord) return true;

					return false;
				}
			);
		}
		public Vertex GetVertex(LR leftRight)
		{
			return leftRight == LR.LEFT
				? this.LeftVertex
				: this.RightVertex;
		}
		public void SetVertex(LR leftRight, Vertex v)
		{
			if (leftRight == LR.LEFT)
				this.LeftVertex = v;
			else
				this.RightVertex = v;
		}
		public bool IsPartOfConvexHull()
		{
			return (this.LeftVertex == null) || (this.RightVertex == null);
		}
		public float GetSitesDistance()
		{
			return (this.LeftSite.Coordinate - this.RightSite.Coordinate).Magnitude;
		}
		public static int CompareSitesDistances_MAX(Edge edge0, Edge edge1)
		{
			var length0 = edge0.GetSitesDistance();
			var length1 = edge1.GetSitesDistance();

			if (length0 < length1)
				return 1;

			if (length0 > length1)
				return -1;

			return 0;
		}
		public static int CompareSitesDistances(Edge edge0, Edge edge1)
		{
			return -CompareSitesDistances_MAX(edge0, edge1);
		}
		public bool IsVisible()
		{
			// Unless the entire Edge is outside the bounds.
			// In that case visible will be false:
			return this.ClippedEnds != null;
		}
		public Site GetSite(LR leftRight)
		{
			return _sites[leftRight];
		}
		public void Dispose()
		{
			this.LeftVertex = null;
			this.RightVertex = null;

			if (this.ClippedEnds != null)
			{
				this.ClippedEnds.Clear();
				this.ClippedEnds = null;
			}

			_sites.Clear();
			_sites = null;
			_queue.Enqueue(this);
		}
		public Edge Init()
		{
			_sites = new Dictionary<LR, Site>();
			return this;
		}
		public override string ToString()
		{
			return "Edge " + EdgeIndex + "; sites " + _sites[LR.LEFT] + ", " + _sites[LR.RIGHT] +
				"; endVertices " + (this.LeftVertex?.VertexIndex.ToString() ?? "null") + ", " +
					(this.RightVertex?.VertexIndex.ToString() ?? "null") + "::";
		}
		public void ClipVertices(Rectf bounds)
		{
			// Set clippedVertices to contain the two ends of the portion of the Voronoi edge that is visible
			// within the bounds. If no part of the Edge falls within the bounds, leave clippedVertices null
			// param bounds

			Vertex vertex0;
			Vertex vertex1;
			var xMin = bounds.X;
			var yMin = bounds.Y;
			var xMax = bounds.Right;
			var yMax = bounds.Bottom;
			float x0;
			float x1;
			float y0;
			float y1;

			if ((Math.Abs(A - 1) < Utilities.EPSILON) && (B >= 0))
			{
				vertex0 = this.RightVertex;
				vertex1 = this.LeftVertex;
			}
			else
			{
				vertex0 = this.LeftVertex;
				vertex1 = this.RightVertex;
			}

			if (Math.Abs(A - 1) < Utilities.EPSILON)
			{
				y0 = yMin;
				if ((vertex0 != null) && (vertex0.Y > yMin))
					y0 = vertex0.Y;

				if (y0 > yMax)
					return;

				x0 = C - B * y0;
				y1 = yMax;

				if ((vertex1 != null) && (vertex1.Y < yMax))
					y1 = vertex1.Y;

				if (y1 < yMin)
					return;

				x1 = C - B * y1;

				if (((x0 > xMax) && (x1 > xMax)) || ((x0 < xMin) && (x1 < xMin)))
					return;

				if (x0 > xMax)
				{
					x0 = xMax;
					y0 = (C - x0) / B;
				}
				else if (x0 < xMin)
				{
					x0 = xMin;
					y0 = (C - x0) / B;
				}

				if (x1 > xMax)
				{
					x1 = xMax;
					y1 = (C - x1) / B;
				}
				else if (x1 < xMin)
				{
					x1 = xMin;
					y1 = (C - x1) / B;
				}
			}
			else {
				x0 = xMin;

				if ((vertex0 != null) && (vertex0.X > xMin))
					x0 = vertex0.X;

				if (x0 > xMax)
					return;

				y0 = C - A * x0;
				x1 = xMax;

				if ((vertex1 != null) && (vertex1.X < xMax))
					x1 = vertex1.X;

				if (x1 < xMin)
					return;

				y1 = C - A * x1;

				if (((y0 > yMax) && (y1 > yMax)) || ((y0 < yMin) && (y1 < yMin)))
					return;

				if (y0 > yMax)
				{
					y0 = yMax;
					x0 = (C - y0) / A;
				}
				else if (y0 < yMin)
				{
					y0 = yMin;
					x0 = (C - y0) / A;
				}

				if (y1 > yMax)
				{
					y1 = yMax;
					x1 = (C - y1) / A;
				}
				else if (y1 < yMin)
				{
					y1 = yMin;
					x1 = (C - y1) / A;
				}
			}

			this.ClippedEnds = new Dictionary<LR, Vector2f>();
			if (vertex0 == LeftVertex)
			{
				this.ClippedEnds[LR.LEFT] = new Vector2f(x0, y0);
				this.ClippedEnds[LR.RIGHT] = new Vector2f(x1, y1);
			}
			else {
				this.ClippedEnds[LR.RIGHT] = new Vector2f(x0, y0);
				this.ClippedEnds[LR.LEFT] = new Vector2f(x1, y1);
			}
		}
		public LineSegment DelaunayLine()
		{
			return new LineSegment(this.LeftSite.Coordinate, this.RightSite.Coordinate);
		}
		public LineSegment VoronoiEdge()
		{
			return !this.IsVisible()
				? new LineSegment(Vector2f.Zero, Vector2f.Zero)
				: new LineSegment(this.ClippedEnds[LR.LEFT], this.ClippedEnds[LR.RIGHT]);
		}
		public static Edge CreateBisectingEdge(Site s0, Site s1)
		{
			float a;
			float b;
			var dx = s1.X - s0.X;
			var dy = s1.Y - s0.Y;
			var absdx = dx > 0 ? dx : -dx;
			var absdy = dy > 0 ? dy : -dy;
			var c = s0.X * dx + s0.Y * dy + (dx * dx + dy * dy) * 0.5f;

			if (absdx > absdy)
			{
				a = 1;
				b = dy / dx;
				c /= dx;
			}
			else {
				b = 1;
				a = dx / dy;
				c /= dy;
			}

			var edge = Edge.Create();

			edge.LeftSite = s0;
			edge.RightSite = s1;
			s0.AddEdge(edge);
			s1.AddEdge(edge);

			edge.A = a;
			edge.B = b;
			edge.C = c;

			return edge;
		}
		#endregion

		#region Private Methods
		private static Edge Create()
		{
			Edge edge;

			if (_queue.Count > 0)
			{
				edge = _queue.Dequeue();
				edge.Init();
			}
			else
				edge = new Edge();

			return edge;
		}
		#endregion
	}
}