using System;
using System.Collections.Generic;
using System.Linq;
using vMap.Voronoi.csDelaunay.Geom;

namespace vMap.Voronoi.csDelaunay.Delaunay
{
	[Serializable]
	public class Voronoi
	{
		#region Private Fields
		private SiteList _sites;
		public IReadOnlyList<Vector2f> SitePoints;
		#endregion

		#region Constructors
		public Voronoi(IReadOnlyList<Vector2f> points, Rectf plotBounds) : this(points, plotBounds, 0)
		{}
		public Voronoi(IReadOnlyList<Vector2f> points, Rectf plotBounds, int lloydIterations)
		{
			Init(points, plotBounds);
			ApplyLloydRelaxation(lloydIterations);
		}
		#endregion

		#region Public Properties
		public List<Edge> Edges { get; private set; }
		public Rectf PlotBounds { get; private set; }
		public List<Triangle> Triangles { get; private set; }
		public Dictionary<Vector2f, Site> SitesIndexedByLocation { get; private set; }
		#endregion

		#region Public Methods
		public void Dispose()
		{
			_sites.Dispose();
			_sites = null;

			foreach (var t in Triangles)
				t.Dispose();

			this.Triangles.Clear();

			foreach (var e in Edges)
				e.Dispose();

			this.Edges.Clear();

			this.PlotBounds = Rectf.Zero;
			this.SitesIndexedByLocation.Clear();
			this.SitesIndexedByLocation = null;
		}
		public void Scale(Rectf newbounds)
		{
			try
			{
				var newSites = new List<Vector2f>(_sites.GetSiteCoords().Count);
				foreach(var v in _sites.GetSiteCoords())
				{
					var newX = newbounds.Width * (v.X / this.PlotBounds.Width);
					var newY = newbounds.Height * (v.Y / this.PlotBounds.Height);
					newSites.Add(new Vector2f(newX, newY, v.X, v.Y));
				}
				this.Init(newSites, newbounds);

				//newSites.AddRange(
				//	from v in _sites.GetSiteCoords()
				//		let newX = newbounds.Width * (v.X / this.PlotBounds.Width)
				//		let newY = newbounds.Height * (v.Y / this.PlotBounds.Height)
				//	select new Vector2f(newX, newY)
				//);
			}
			catch(Exception exc)
			{
				throw new Exception(@"An error occurred when scaling the Voronoi graph.", exc);
			}
		}
		public List<Vector2f> GetRegion(Vector2f p)
		{
			Site site;
			return this.SitesIndexedByLocation.TryGetValue(p, out site)
				? site.GetRegion(this.PlotBounds)
				: new List<Vector2f>();
		}
		public List<Vector2f> GetNeighborSitesForSite(Vector2f coord)
		{
			var points = new List<Vector2f>();

			Site site;
			if (!SitesIndexedByLocation.TryGetValue(coord, out site))
				return points;

			var sites = site.GetNeighborSites();
			points.AddRange(sites.Select(s => s.Coordinate));

			return points;
		}
		public List<Circle> GetCircles()
		{
			return _sites.GetCircles();
		}
		public List<LineSegment> GetVoronoiBoundaryForSite(Vector2f coord)
		{
			return LineSegment.VisibleLineSegments(Edge.GetSelectEdgesForSitePoint(coord, this.Edges));
		}
		public List<LineSegment> GetLineSegments()
		{
			return LineSegment.VisibleLineSegments(this.Edges);
		}
		public List<Edge> GetHullEdges()
		{
			return Edges.FindAll(e => e.IsPartOfConvexHull());
		}
		public List<Vector2f> GetHullPointsInOrder()
		{
			var hullEdges = GetHullEdges();
			var points = new List<Vector2f>();

			if (hullEdges.Count == 0)
				return points;

			var reorderer = new EdgeReorderer(hullEdges, typeof(Site));
			hullEdges = reorderer.Edges;
			var orientations = reorderer.EdgeOrientations;
			reorderer.Dispose();

			for (var i = 0; i < hullEdges.Count; i++)
			{
				var edge = hullEdges[i];
				var orientation = orientations[i];
				points.Add(edge.GetSite(orientation).Coordinate);
			}
			return points;
		}
		public List<List<Vector2f>> GetRegions()
		{
			return _sites.GetRegions(this.PlotBounds);
		}
		public List<Vector2f> GetSiteCoords()
		{
			return _sites.GetSiteCoords();
		}
		public void ApplyLloydRelaxation(int iterations)
		{
			// Reapeat the whole process for the number of iterations asked
			for (var i = 0; i < iterations; i++)
			{
				var newPoints = new List<Vector2f>();
				
				// Go thourgh all sites
				_sites.ResetListIndex();
				var site = _sites.Next();

				while (site != null)
				{
					var siteColor = site.Color;

					// Loop all corners of the site to calculate the centroid
					var region = site.GetRegion(PlotBounds);
					if (region.Count < 1)
					{
						site = _sites.Next();
						continue;
					}

					var centroid = new Vector2f(0,0);
					centroid.Color = siteColor;
					centroid.prevX = (Math.Abs(site.X) < vMap.Voronoi.Utilities.MathPrecision) ? site.prevX : site.X;
					centroid.prevY = (Math.Abs(site.Y) < vMap.Voronoi.Utilities.MathPrecision) ? site.prevY : site.Y;

					float signedArea = 0;
					float x0 = 0;
					float y0 = 0;
					float x1 = 0;
					float y1 = 0;
					float a = 0;

					// For all vertices except last
					for (var j = 0; j < region.Count - 1; j++)
					{
						x0 = region[j].X;
						y0 = region[j].Y;
						x1 = region[j + 1].X;
						y1 = region[j + 1].Y;
						a = x0 * y1 - x1 * y0;
						signedArea += a;
						centroid.X += (x0 + x1) * a;
						centroid.Y += (y0 + y1) * a;
					}

					// Do last vertex
					x0 = region[region.Count - 1].X;
					y0 = region[region.Count - 1].Y;
					x1 = region[0].X;
					y1 = region[0].Y;
					a = x0 * y1 - x1 * y0;
					signedArea += a;
					centroid.X += (x0 + x1) * a;
					centroid.Y += (y0 + y1) * a;

					signedArea *= 0.5f;
					centroid.X /= (6 * signedArea);
					centroid.Y /= (6 * signedArea);
					
					// Move site to the centroid of its Voronoi cell
					newPoints.Add(centroid);
					centroid = null;
					site = _sites.Next();
				}

				// Between each replacement of the centroid of the cell,
				// we need to recompute Voronoi diagram:
				var origPlotBounds = this.PlotBounds;
				Dispose();
				Init(newPoints, origPlotBounds);
			}
		}
		public static int CompareByYThenX(Site s1, Site s2)
		{
			if (s1.Y < s2.Y)
				return -1;

			if (s1.Y > s2.Y)
				return 1;

			if (s1.X < s2.X)
				return -1;

			if (s1.X > s2.X)
				return 1;

			return 0;
		}
		public static int CompareByYThenX(Site s1, Vector2f s2)
		{
			if (s1.Y < s2.Y)
				return -1;

			if (s1.Y > s2.Y)
				return 1;

			if (s1.X < s2.X)
				return -1;

			if (s1.X > s2.X)
				return 1;

			return 0;
		}
		#endregion

		#region Private Methods
		private void Init(IReadOnlyList<Vector2f> points, Rectf plotBounds)
		{
			this.SitePoints = points;
			_sites = new SiteList(this.SitePoints.Count);
			this.SitesIndexedByLocation = new Dictionary<Vector2f, Site>(this.SitePoints.Count);

			// this *must* come after the above 'SitesIndexByLocation' property initialization
			this.AddSites(this.SitePoints);

			this.PlotBounds = plotBounds;
			this.Triangles = new List<Triangle>();
			this.Edges = new List<Edge>();
			ApplyFortunesAlgorithm();
		}
		private void AddSites(IReadOnlyList<Vector2f> points)
		{
			for (var i = 0; i < points.Count; i++)
				AddSite(points[i], i);
		}
		private void AddSite(Vector2f p, int index)
		{
			var site = Site.CreateSite(p, index, (float)new Random().NextDouble() * 100);
			site.Color = p.Color;
			//site.PrevCoordinates = new Vector2f(p.prevX, p.prevY);
			_sites.Add(site);
			SitesIndexedByLocation[p] = site;
		}
		private static Site GetLeftRegionAsSite(Halfedge halfEdge, Site bottomMostSite)
		{
			var edge = halfEdge.Edge;

			if (edge == null)
				return bottomMostSite;

			return edge.GetSite(halfEdge.LeftRight);
		}
		private static Site GetRightRegionAsSite(Halfedge halfEdge, Site bottomMostSite)
		{
			var edge = halfEdge.Edge;

			return edge == null
				? bottomMostSite
				: edge.GetSite(LR.Other(halfEdge.LeftRight));
		}
		private void ApplyFortunesAlgorithm()
		{
			var newIntStar = Vector2f.Zero;
			var dataBounds = _sites.GetSitesBounds();
			var sqrtSitesNb = (int)Math.Sqrt(_sites.Count() + 4);
			var queue = new HalfedgePriorityQueue(dataBounds.Y, dataBounds.Height, sqrtSitesNb);
			var edgeList = new EdgeList(dataBounds.X, dataBounds.Width, sqrtSitesNb);
			var halfEdges = new List<Halfedge>();
			var vertices = new List<Vertex>();
			var bottomMostSite = _sites.Next();
			var newSite = _sites.Next();

			while (true)
			{
				if (!queue.IsEmpty())
					newIntStar = queue.GetMin();

				Site bottomSite;
				Halfedge lbnd;
				Halfedge rbnd;
				Vertex vertex;
				Halfedge bisector;
				Edge edge;

				if (newSite != null && (queue.IsEmpty() || CompareByYThenX(newSite, newIntStar) < 0))
				{
					// New site is smallest
					//Debug.Log("smallest: new site " + newSite);

					lbnd = edgeList.GetLeftNeighbor(newSite.Coordinate);    // The halfedge just to the left of newSite
					rbnd = lbnd.EdgeListRightNeighbor;						// The halfedge just to the right
					bottomSite = GetRightRegionAsSite(lbnd, bottomMostSite);         // This is the same as leftRegion(rbnd)
																			// This Site determines the region containing the new site
																			
					edge = Edge.CreateBisectingEdge(bottomSite, newSite);
					Edges.Add(edge);
					bisector = Halfedge.Create(edge, LR.LEFT);
					halfEdges.Add(bisector);
					
					// Inserting two halfedges into edgelist constitutes Step 10:
					// Insert bisector to the right of lbnd:
					edgeList.Insert(lbnd, bisector);

					// First half of Step 11:
					if ((vertex = Vertex.Intersect(lbnd, bisector)) != null)
					{
						vertices.Add(vertex);
						queue.Remove(lbnd);
						lbnd.Vertex = vertex;
						lbnd.Ystar = vertex.Y + newSite.GetDistance(vertex);
						queue.Insert(lbnd);
					}

					lbnd = bisector;
					bisector = Halfedge.Create(edge, LR.RIGHT);
					halfEdges.Add(bisector);
					
					// Second halfedge for Step 10::
					// Insert bisector to the right of lbnd:
					edgeList.Insert(lbnd, bisector);

					// Second half of Step 11:
					if ((vertex = Vertex.Intersect(bisector, rbnd)) != null)
					{
						vertices.Add(vertex);
						bisector.Vertex = vertex;
						bisector.Ystar = vertex.Y + newSite.GetDistance(vertex);
						queue.Insert(bisector);
					}
					newSite = _sites.Next();
				}
				else if (!queue.IsEmpty())
				{
					// Intersection is smallest
					lbnd = queue.ExtractMin();
					var llbnd = lbnd.EdgeListLeftNeighbor;
					rbnd = lbnd.EdgeListRightNeighbor;
					var rrbnd = rbnd.EdgeListRightNeighbor;
					bottomSite = GetLeftRegionAsSite(lbnd, bottomMostSite);
					var topSite = GetRightRegionAsSite(rbnd, bottomMostSite);
					
					// These three sites define a Delaunay triangle
					this.Triangles.Add(new Triangle(bottomSite, topSite, GetRightRegionAsSite(lbnd, bottomMostSite)));

					var v = lbnd.Vertex;
					v.SetIndex();
					lbnd.Edge.SetVertex(lbnd.LeftRight, v);
					rbnd.Edge.SetVertex(rbnd.LeftRight, v);
					edgeList.Remove(lbnd);
					queue.Remove(rbnd);
					edgeList.Remove(rbnd);
					var leftRight = LR.LEFT;

					if (bottomSite.Y > topSite.Y)
					{
						var tempSite = bottomSite;
						bottomSite = topSite;
						topSite = tempSite;
						leftRight = LR.RIGHT;
					}

					edge = Edge.CreateBisectingEdge(bottomSite, topSite);
					Edges.Add(edge);
					bisector = Halfedge.Create(edge, leftRight);
					halfEdges.Add(bisector);
					edgeList.Insert(llbnd, bisector);
					edge.SetVertex(LR.Other(leftRight), v);

					if ((vertex = Vertex.Intersect(llbnd, bisector)) != null)
					{
						vertices.Add(vertex);
						queue.Remove(llbnd);
						llbnd.Vertex = vertex;
						llbnd.Ystar = vertex.Y + bottomSite.GetDistance(vertex);
						queue.Insert(llbnd);
					}

					if ((vertex = Vertex.Intersect(bisector, rrbnd)) != null)
					{
						vertices.Add(vertex);
						bisector.Vertex = vertex;
						bisector.Ystar = vertex.Y + bottomSite.GetDistance(vertex);
						queue.Insert(bisector);
					}
				}
				else
					break;
			}

			// Queue should be empty now
			queue.Dispose();
			edgeList.Dispose();

			foreach (var halfedge in halfEdges)
				halfedge.ReallyDispose();

			halfEdges.Clear();

			// we need the vertices to clip the edges
			foreach (var e in Edges)
				e.ClipVertices(PlotBounds);
			
			// But we don't actually ever use them again!
			foreach (var ve in vertices)
				ve.Dispose();

			vertices.Clear();
		}
		#endregion
	}
}