using System;
using System.Collections.Generic;
using vMap.Voronoi.csDelaunay.Geom;

namespace vMap.Voronoi.csDelaunay.Delaunay
{
	[Serializable]
	public class Vertex : ICoord
	{
		#region Private Fields
		private static int _nVertices = 0;
		private static readonly Queue<Vertex> _queue = new Queue<Vertex>();
		#endregion

		#region Constructors
		public Vertex(float x, float y)
		{
			Init(x, y);
		}
		#endregion

		#region Public Properties
		public Vector2f Coordinate { get; set; }
		public float X => Coordinate.X;
		public float Y => Coordinate.Y;
		public static readonly Vertex VertexAtInfinity = new Vertex(float.NaN, float.NaN);
		public int VertexIndex { get; private set; }
		#endregion

		#region Public Methods
		public void Dispose()
		{
			Coordinate = Vector2f.Zero;
			_queue.Enqueue(this);
		}
		public void SetIndex()
		{
			VertexIndex = _nVertices++;
		}
		public override string ToString()
		{
			return "Vertex (" + VertexIndex + ")";
		}
		public static Vertex Intersect(Halfedge halfedge0, Halfedge halfedge1)
		{
			Edge edge;
			Halfedge halfedge;

			var edge0 = halfedge0.Edge;
			var edge1 = halfedge1.Edge;

			if (edge0 == null || edge1 == null)
				return null;

			if (edge0.RightSite == edge1.RightSite)
				return null;

			var determinant = edge0.A * edge1.B - edge0.B * edge1.A;

			if (Math.Abs(determinant) < 1E-10) // edges are parallel
				return null;

			var intersectionX = (edge0.C * edge1.B - edge1.C * edge0.B) / determinant;
			var intersectionY = (edge1.C * edge0.A - edge0.C * edge1.A) / determinant;

			if (Voronoi.CompareByYThenX(edge0.RightSite, edge1.RightSite) < 0)
			{
				halfedge = halfedge0;
				edge = edge0;
			}
			else {
				halfedge = halfedge1;
				edge = edge1;
			}

			var rightOfSite = intersectionX >= edge.RightSite.X;

			if ((rightOfSite && halfedge.LeftRight == LR.LEFT) || (!rightOfSite && halfedge.LeftRight == LR.RIGHT))
				return null;

			return Create(intersectionX, intersectionY);
		}
		#endregion

		#region Private Methods
		private static Vertex Create(float x, float y)
		{
			if (float.IsNaN(x) || float.IsNaN(y))
				return VertexAtInfinity;

			return _queue.Count > 0 ? _queue.Dequeue().Init(x, y) : new Vertex(x, y);
		}
		private Vertex Init(float x, float y)
		{
			Coordinate = new Vector2f(x, y);
			return this;
		}
		#endregion
	}
}