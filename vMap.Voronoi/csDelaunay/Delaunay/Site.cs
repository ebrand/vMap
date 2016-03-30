using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using vMap.Voronoi.csDelaunay.Geom;

namespace vMap.Voronoi.csDelaunay.Delaunay {

	[Serializable]
	public class Site : ICoord
	{
		#region Private Fields
		private static readonly Queue<Site> _queue = new Queue<Site>();
		#endregion

		#region Constructors
		public Site(Vector2f coord, int index, float weight)
		{
			Init(coord, index, weight);
		}
		#endregion

		#region Public Properties
		public Color Color { get; set; }
		public Vector2f PrevCoordinates { get; set; }
		public Vector2f Coordinate { get; set; }
		public List<Edge> CircumferenceEdges { get; private set; }
		public List<LR> CircumferenceEdgeOrientations; // which end of each edge hooks up with the previous edge in edges
		public List<Vector2f> RegionCornerCoordinates; // ordered list of points that define the region clipped to bounds
		public int SiteIndex { get; set; }
		public float Weight { get; private set; }
		public float X => this.Coordinate.X;
		public float Y => this.Coordinate.Y;
		public float prevX => this.PrevCoordinates.X;
		public float prevY => this.PrevCoordinates.Y;
		#endregion

		#region Public Methods
		public void AddEdge(Edge edge)
		{
			CircumferenceEdges.Add(edge);
		}
		public int CompareToSite(Site s1)
		{
			var result = Voronoi.CompareByYThenX(this, s1);
			int tempIndex;

			switch(result)
			{
				case -1:

					if (this.SiteIndex <= s1.SiteIndex)
						return result;

					tempIndex = this.SiteIndex;
					this.SiteIndex = s1.SiteIndex;
					s1.SiteIndex = tempIndex;

					break;

				case 1:

					if (s1.SiteIndex <= this.SiteIndex)
						return result;

					tempIndex = s1.SiteIndex;
					s1.SiteIndex = this.SiteIndex;
					this.SiteIndex = tempIndex;

					break;
			}
			return result;
		}
		public static Site CreateSite(Vector2f coord, int index, float weight)
		{
			return _queue.Count > 0
				? _queue.Dequeue().Init(coord, index, weight)
				: new Site(coord, index, weight) { PrevCoordinates = new Vector2f(coord.prevX, coord.prevY) };
		}
		public float GetDistance(ICoord p)
		{
			return (this.Coordinate - p.Coordinate).Magnitude;
		}
		public Edge GetNearestEdge()
		{
			CircumferenceEdges.Sort(Edge.CompareSitesDistances);
			return CircumferenceEdges[0];
		}
		public List<Site> GetNeighborSites()
		{
			if ((CircumferenceEdges == null) || (CircumferenceEdges.Count == 0))
				return new List<Site>();

			if (CircumferenceEdgeOrientations == null)
				ReorderEdges();

			return CircumferenceEdges.Select(GetNeighborSite).ToList();
		}
		public List<Vector2f> GetRegion(Rectf clippingBounds)
		{
			if ((CircumferenceEdges == null) || (CircumferenceEdges.Count == 0))
				return new List<Vector2f>();

			if(CircumferenceEdgeOrientations != null)
				return RegionCornerCoordinates;

			ReorderEdges();

			RegionCornerCoordinates = ClipToBounds(clippingBounds);

			if ((new Polygon(RegionCornerCoordinates)).PolyWinding() == Winding.CLOCKWISE)
				RegionCornerCoordinates.Reverse();

			return RegionCornerCoordinates;
		}
		public static void SortSites(List<Site> sites)
		{
			sites.Sort(delegate (Site s0, Site s1)
			{
				int tempIndex;
				var result = Voronoi.CompareByYThenX(s0, s1);

				switch(result)
				{
					case -1:

						if (s0.SiteIndex <= s1.SiteIndex)
							return result;

						tempIndex = s0.SiteIndex;
						s0.SiteIndex = s1.SiteIndex;
						s1.SiteIndex = tempIndex;

						break;

					case 1:

						if (s1.SiteIndex <= s0.SiteIndex)
							return result;

						tempIndex = s1.SiteIndex;
						s1.SiteIndex = s0.SiteIndex;
						s0.SiteIndex = tempIndex;

						break;
				}
				return result;
			});
		}
		public override string ToString()
		{
			return $"Site: {this.SiteIndex}: {this.Coordinate}";
		}
		#endregion

		#region Private Methods
		private Site Init(Vector2f p, int index, float weigth)
		{
			this.Coordinate = p;
			this.PrevCoordinates =
				  ((Math.Abs(p.prevX) < vMap.Voronoi.Utilities.MathPrecision)
				&& (Math.Abs(p.prevY) < vMap.Voronoi.Utilities.MathPrecision))
				? new Vector2f(p.X, p.Y) 
				: new Vector2f(p.prevX, p.prevY);

			this.SiteIndex = index;
			this.Weight = weigth;
			this.CircumferenceEdges = new List<Edge>();
			this.RegionCornerCoordinates = null;

			return this;
		}
		private void Clear()
		{
			if (this.CircumferenceEdges != null)
			{
				this.CircumferenceEdges.Clear();
				this.CircumferenceEdges = null;
			}

			if (this.CircumferenceEdgeOrientations != null)
			{
				this.CircumferenceEdgeOrientations.Clear();
				this.CircumferenceEdgeOrientations = null;
			}

			if(this.RegionCornerCoordinates == null)
				return;

			this.RegionCornerCoordinates.Clear();
			this.RegionCornerCoordinates = null;
		}
		private List<Vector2f> ClipToBounds(Rectf bounds)
		{
			var points = new List<Vector2f>();
			var n = CircumferenceEdges.Count;
			var i = 0;

			while ((i < n) && !CircumferenceEdges[i].IsVisible())
				i++;

			if (i == n) // No edges visible
				return new List<Vector2f>();

			var edge = CircumferenceEdges[i];
			var orientation = CircumferenceEdgeOrientations[i];
			points.Add(edge.ClippedEnds[orientation]);
			points.Add(edge.ClippedEnds[LR.Other(orientation)]);

			for (var j = i + 1; j < n; j++)
			{
				edge = CircumferenceEdges[j];

				if (!edge.IsVisible())
					continue;

				this.Connect(ref points, j, bounds);
			}
			
			// Close up the polygon by adding another corner point of the bounds if needed:
			this.Connect(ref points, i, bounds, true);

			return points;
		}
		private static bool IsCloseEnough(Vector2f p0, Vector2f p1)
		{
			return (p0 - p1).Magnitude < Utilities.EPSILON;
		}
		private void Connect(ref List<Vector2f> points, int j, Rectf bounds, bool closingUp = false)
		{
			var rightPoint = points[points.Count - 1];
			var newEdge = CircumferenceEdges[j];
			var newOrientation = CircumferenceEdgeOrientations[j];

			// The point that must be conected to rightPoint:
			var newPoint = newEdge.ClippedEnds[newOrientation];

			if (!Site.IsCloseEnough(rightPoint, newPoint))
			{
				// The points do not coincide, so they must have been clipped at the bounds;
				// see if they are on the same border of the bounds:
				if (   (Math.Abs(rightPoint.X - newPoint.X) > Utilities.EPSILON)
					&& (Math.Abs(rightPoint.Y - newPoint.Y) > Utilities.EPSILON)
				){
					// They are on different borders of the bounds;
					// insert one or two corners of bounds as needed to hook them up:
					// (NOTE this will not be correct if the region should take up more than
					// half of the bounds rect, for then we will have gone the wrong way
					// around the bounds and included the smaller part rather than the larger)
					var rightCheck = BoundsCheck.Check(rightPoint, bounds);
					var newCheck = BoundsCheck.Check(newPoint, bounds);
					float px;
					float py;
					if ((rightCheck & BoundsCheck.RIGHT) != 0)
					{
						px = bounds.Right;

						if ((newCheck & BoundsCheck.BOTTOM) != 0)
						{
							py = bounds.Bottom;
							points.Add(new Vector2f(px, py));

						}
						else if ((newCheck & BoundsCheck.TOP) != 0)
						{
							py = bounds.Top;
							points.Add(new Vector2f(px, py));

						}
						else if ((newCheck & BoundsCheck.LEFT) != 0)
						{
							py = rightPoint.Y - bounds.Y + newPoint.Y - bounds.Y < bounds.Height
								? bounds.Top
								: bounds.Bottom;
							
							points.Add(new Vector2f(px, py));
							points.Add(new Vector2f(bounds.Left, py));
						}
					}
					else if ((rightCheck & BoundsCheck.LEFT) != 0)
					{
						px = bounds.Left;

						if ((newCheck & BoundsCheck.BOTTOM) != 0)
						{
							py = bounds.Bottom;
							points.Add(new Vector2f(px, py));

						}
						else if ((newCheck & BoundsCheck.TOP) != 0)
						{
							py = bounds.Top;
							points.Add(new Vector2f(px, py));

						}
						else if ((newCheck & BoundsCheck.RIGHT) != 0)
						{
							if (rightPoint.Y - bounds.Y + newPoint.Y - bounds.Y < bounds.Height)
								py = bounds.Top;
							else
								py = bounds.Bottom;

							points.Add(new Vector2f(px, py));
							points.Add(new Vector2f(bounds.Right, py));
						}
					}
					else if ((rightCheck & BoundsCheck.TOP) != 0)
					{
						py = bounds.Top;

						if ((newCheck & BoundsCheck.RIGHT) != 0)
						{
							px = bounds.Right;
							points.Add(new Vector2f(px, py));

						}
						else if ((newCheck & BoundsCheck.LEFT) != 0)
						{
							px = bounds.Left;
							points.Add(new Vector2f(px, py));

						}
						else if ((newCheck & BoundsCheck.BOTTOM) != 0)
						{
							if (rightPoint.X - bounds.X + newPoint.X - bounds.X < bounds.Width)
							{
								px = bounds.Left;
							}
							else {
								px = bounds.Right;
							}
							points.Add(new Vector2f(px, py));
							points.Add(new Vector2f(px, bounds.Bottom));
						}
					}
					else if ((rightCheck & BoundsCheck.BOTTOM) != 0)
					{
						py = bounds.Bottom;

						if ((newCheck & BoundsCheck.RIGHT) != 0)
						{
							px = bounds.Right;
							points.Add(new Vector2f(px, py));

						}
						else if ((newCheck & BoundsCheck.LEFT) != 0)
						{
							px = bounds.Left;
							points.Add(new Vector2f(px, py));

						}
						else if ((newCheck & BoundsCheck.TOP) != 0)
						{
							if (rightPoint.X - bounds.X + newPoint.X - bounds.X < bounds.Width)
							{
								px = bounds.Left;
							}
							else {
								px = bounds.Right;
							}
							points.Add(new Vector2f(px, py));
							points.Add(new Vector2f(px, bounds.Top));
						}
					}
				}
				if (closingUp)
				{
					// newEdge's ends have already been added
					return;
				}
				points.Add(newPoint);
			}
			Vector2f newRightPoint = newEdge.ClippedEnds[LR.Other(newOrientation)];
			if (!IsCloseEnough(points[0], newRightPoint))
			{
				points.Add(newRightPoint);
			}
		}
		private Site GetNeighborSite(Edge edge)
		{
			if (this == edge.LeftSite)
				return edge.RightSite;

			return this == edge.RightSite
				? edge.LeftSite
				: null;
		}
		private void ReorderEdges()
		{
			var reorderer = new EdgeReorderer(this.CircumferenceEdges, typeof(Vertex));
			this.CircumferenceEdges = reorderer.Edges;
			this.CircumferenceEdgeOrientations = reorderer.EdgeOrientations;
			reorderer.Dispose();
		}
		#endregion
	}
}