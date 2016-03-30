using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.Xna.Framework;
using vMap.Voronoi.csDelaunay.Geom;
using Rectangle = System.Drawing.Rectangle;

namespace vMap.Voronoi
{
	public class Map
	{
		public event EventHandler SiteUpdated;

		#region Private Fields
		private double[,] _noiseMap;
		#endregion

		#region Constructors
		public Map(int siteCount, Rectangle plotBounds)
		{
			Utilities.Debug($"vMap.Voronoi.Map Constructor({siteCount}, {plotBounds}) called.");

			this.Initialize(VoronoiGraphFactory.Instance.GetPoints(siteCount, plotBounds), plotBounds);
		}
		public Map(PointF[] sitePoints, Rectangle plotBounds)
		{
			Utilities.Debug($"vMap.Voronoi.Map Constructor({sitePoints}, {plotBounds}) called.");

			this.Initialize(sitePoints, plotBounds);
		}
		private void Initialize(PointF[] points, Rectangle plotBounds)
		{
			Utilities.Debug($"vMap.Voronoi.Map.Initialize({points}, {plotBounds}) called.");

			this.Voronoi = VoronoiGraphFactory.Instance.Initialize(points, plotBounds);
			this.SitePoints = this.Voronoi.SitePoints.Select(p => new PointF(p.X, p.Y)).ToArray();
			this.ElevationThresholds = new ElevationThresholds();
			this.GenerateMap();
		}
		#endregion

		#region Public Properties
		public string MapSavePathAndName;
		public PointF[] SitePoints;
		public vMap.Voronoi.csDelaunay.Delaunay.Voronoi Voronoi;
		public Graph<Site, Coordinate, PointF> MapGraph;
		public ElevationThresholds ElevationThresholds;
		public double[,] NoiseMap => _noiseMap;
		/*	The 'MapGraph' (Graph<Site, Vertex>) is a collection of two directed,
			acyclic graph in which GraphVertex and GraphEdge objects are related
			in a "2-to-many" relationship. The GraphVertex<T, K> has two collections
			which allow the graph to hold a relationship between the two DAGs.
			The "Edges" collection allow for the normal relationship between two
			like vertex objects while the "CrossEdges" collection allows for a
			relationship between two different graph vertex objects (in this case
			"Site" and 'Coordinate' objects).
			                                                                         
		                      	     1 +----------------+ 1                                                                
		                      	  +----| GRAPH_VERTEX_A |----+                                                                
		                      	 1|    +----------------+    |1                                                              
		                      +--------------+   |1   1|    +--------------+                                      
		              "Edges" |  GRAPH_EDGE  |---+     +----|  GRAPH_EDGE  | "CrossEdges"                                              
		                      +--------------+*           * +--------------+                                      
		                      	                             |1                                                   
		                      	       +----------------+    |                                                               
		                      	       | GRAPH_VERTEX_B |----+                                                               
		                      	       +----------------+   1                                                                                                                                  

			In our 'Sites' graph, the edges will represent the 'path' between neighboring
			sites as well as the lines in the Delaunay triangulation visualization.
			
			'A', 'B' and 'C' below are 'sites' on the map. Each site has a set of
			coordinates originally used to calculate the Voronoi edges by the Voronoi
			algorithm passed-in to this map. As such, these coordinates are not at
			the center of the site's region. The centers of each site (region) are
			calculated and stored separately.
			
			The dotted lines connecting the sites below are the edges in our graph.
			The edges in the graph are useful for pathfinding, etc.
			
			The '+' signs represent x/y coordinates for each corner of the site's
			region. These are collected as an array of 'PointF' objects to facilitate
			rendering the site region.
					                                                            
					       +------+                                              
					      /        \                                      
					     /    A     +----+                                  
					    +     . .  /      \                              
					     \    .  ./        \                             
					      \   .  /.         \                            
					       +--.-+  .         +                                
					      /   .  \  .C      /                           
					     /    .   .        /                            
						/     . .  \      /                                  
					   +      B     +----+                               
					    \          /                                      
					     \        /                                        
					      +------+                                              
		*/
		public Dictionary<PointF, GraphVertex<Site, Coordinate>> Sites => this.MapGraph.PrimaryGraph;
		/*
			In our 'Vertices' graph, the graph edges will represent the 'path' between
			neighboring coordinates. These are the lines that will be drawn representing
			the map cell borders. The graph coordinates will represent an x, y coordinate
			of the points at the corners of each cell.
			
			This data comes from a call to the Voronoi graph's 'GetLineSegments()'
			method.	Each line segment is comprised of two coordinates. When we traverse
			this collection, we will encounter the same coordinate (at the same x/y position)
			several times for points that are shared between two line segments (i.e. line
			segments 'B'-'C', 'C'-'D' and 'C'-'M' in the diagram below all share
			point 'C'). These points will need to be normalized in our graph (stored once
			and referenced by multiple 'Edges').

		                   A      B              
					       +------+                                              
					      /        \ C   D                                
					     /          +----+                                  
					  L +          /      \                              
					     \        /        \                             
					      \    M /          \                            
					     K +----+            + E                              
					      /      \          /                           
					     /        \        /                            
						/          \      /                                  
					 J +            +----+                               
					    \          / G    F                               
					     \        /                                        
					      +------+
			             I        H                                    
		*/
		public Dictionary<PointF, GraphVertex<Coordinate, Site>> Vertices => this.MapGraph.SecondaryGraph;
		public GraphVertex<Site, Coordinate> CurrLocation;
		public Site SearchStart { get; set; }
		public Site SearchGoal { get; set; }
		#endregion

		#region Public Methods
		public Map GenerateMap()
		{
			Utilities.Debug($"vMap.Voronoi.Map.Generatemap() called.");

			var newMapGraph = new Graph<Site, Coordinate, PointF>();
			
			/*	xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
				GENERATE SITE GRAPH
			*/
			foreach(var dSite in this.Voronoi.SitesIndexedByLocation.Values)
				this.AddSiteToGraph(graph: newMapGraph, dSite: dSite);

			if(newMapGraph.PrimaryGraph.Any(s => s.Value.Data.Current))
				this.CurrLocation = newMapGraph.PrimaryGraph.Single(s => s.Value.Data.Current).Value;

			/*	xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
				GENERATE SITE GRAPH EDGES
			*/
			foreach(var dSite in this.Voronoi.SitesIndexedByLocation.Values)
			{
				GraphVertex<Site, Coordinate> foundSite;
				if(!newMapGraph.PrimaryGraph.TryGetValue(new PointF(dSite.Coordinate.X, dSite.Coordinate.Y), out foundSite))
					continue;

				foreach (var dNeighborSite in dSite.GetNeighborSites())
				{
					GraphVertex<Site, Coordinate> foundNeighborSite;
					if(newMapGraph.PrimaryGraph.TryGetValue(new PointF(dNeighborSite.Coordinate.X, dNeighborSite.Coordinate.Y), out foundNeighborSite))
						foundSite.AddEdge(new GraphEdge<Site, Coordinate>(foundSite, foundNeighborSite, 0F));
				}
			}

			/*	xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
				GENERATE VERTEX GRAPH
			*/
			foreach(var dLineSegment in this.Voronoi.GetLineSegments())
			{
				if(!dLineSegment.P0.Equals(dLineSegment.P1))
					AddVertexToGraph(graph: newMapGraph, dLineSegment: dLineSegment);
			}

			/*	xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
				COORELATE VERTICES AND SITES
			*/
			foreach (var site in newMapGraph.PrimaryGraph.Values)
			{
				foreach (var regPoint in site.Data.RegionPoints)
				{
					GraphVertex<Coordinate, Site> foundVertex;
					if(newMapGraph.SecondaryGraph.TryGetValue(regPoint, out foundVertex))
						site.AddCrossEdge(new GraphCrossEdge<Site, Coordinate>(site, foundVertex));
				}
				site.Data.RegionPoints =
					site.CrossEdges.Select(ce =>
						new PointF(
							ce.Value.ConnectedNode.Data.Point.X,
							ce.Value.ConnectedNode.Data.Point.Y)
						).ToArray();
			}
			this.MapGraph = newMapGraph;
			return this;
		}
		public void Relax(int iterations)
		{
			Utilities.Debug($"vMap.Voronoi.Map.Relax({iterations}) called.");

			if (this.Voronoi == null)
				return;

			this.Voronoi.ApplyLloydRelaxation(iterations);
			this.GenerateMap();
			this.AssignElevations();
		}
		public void Scale(Rectangle newBounds)
		{
			Utilities.Debug($"vMap.Voronoi.Map.Scale({newBounds}) called.");

			if (this.Voronoi == null)
				return;

			this.Voronoi.Scale(new Rectf(newBounds.X, newBounds.Y, newBounds.Width, newBounds.Height));
			this.ScaleNoiseMap(newBounds);
			this.GenerateMap();
			this.AssignElevations();
		}
		public bool MoveToRandomNeighbor()
		{
			Utilities.Debug($"vMap.Voronoi.Map.MoveToRandomNeighbor() called.");

			// try to pick a new site
			GraphVertex<Site, Coordinate> targetSite = null;
			var tries = 0;
			while ((targetSite == null) || (tries < this.CurrLocation.Edges.Count))
			{
				targetSite = this.CurrLocation.Edges.Values.ToArray()[Utilities.GetRandomNumberInRange(0, this.CurrLocation.Edges.Values.Count - 1)].Node2;
				tries++;
			}

			// exit the current site and enter the target site
			this.CurrLocation.Data.Exit();
			this.CurrLocation = targetSite;
			this.CurrLocation.Data.Enter();

			return true;
		}
		public void SaveConfiguration()
		{
			var bs = new BinaryFormatter();
			var fs = File.Create($"{this.MapSavePathAndName}");
			bs.Serialize(fs, this);
			fs.Close();
		}
		public static Map LoadConfiguration(string filePath, Rectangle plotBounds)
		{
			var fs = File.OpenRead($"{filePath}");
			var map = (Map)new BinaryFormatter().Deserialize(fs);
			fs.Close();

			map.Scale(plotBounds);
			return map;
		}
		public void SetBiomeIntensity(ThresholdColorProperty thresholdProperty, int value)
		{
			Utilities.Debug($"vMap.Voronoi.Map.SetBiomeIntensity({thresholdProperty}, {value}) called.");

			if (this.ElevationThresholds == null)
				this.ElevationThresholds = new ElevationThresholds();

			switch(thresholdProperty)
			{
				case ThresholdColorProperty.Ocean:
					this.ElevationThresholds.Ocean = value;
					break;
				case ThresholdColorProperty.Beach:
					this.ElevationThresholds.Beach = value;
					break;
				case ThresholdColorProperty.Lowland:
					this.ElevationThresholds.Lowland = value;
					break;
				case ThresholdColorProperty.Highland:
					this.ElevationThresholds.Highland = value;
					break;
				case ThresholdColorProperty.Mountain:
					this.ElevationThresholds.Mountain = value;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(thresholdProperty), thresholdProperty, null);
			}
			this.AssignElevations();
		}
		public void SetBiomeIntensityToDefaults()
		{
			this.ElevationThresholds = new ElevationThresholds();
			this.AssignElevations();
		}
		public void SetNoiseMap(double[,] noiseMap, double scale = 1.0)
		{
			Utilities.Debug("vMap.Voronoi.Map.SetNoiseMap() called.");

			if (noiseMap == null)
				return;

			if(scale > 1.0)
				scale = 1.0;

			_noiseMap = (Math.Abs(scale - 1.0) > Utilities.MathPrecision)
				? ScaleNoiseMap(noiseMap, scale, scale)
				: noiseMap;

			this.AssignElevations();
		}
		public GraphVertex<Site, Coordinate> GetSite(Vector2 point)
		{
			return GetSite(new PointF(point.X, point.Y));
		} 
		public GraphVertex<Site, Coordinate> GetSite(PointF point)
		{
			Utilities.Debug($"vMap.Voronoi.Map.GetSite({point}) called.");

			var sw = new Stopwatch();
			sw.Reset();
			sw.Start();
			GraphVertex <Site, Coordinate> foundSite = null;
			foreach(var siteVertex in this.Sites.Values)
			{
				if(!Utilities.IsInPolygon(siteVertex.Data.RegionPoints, point))
					continue;

				foundSite = siteVertex;
				break;
			}
			sw.Stop();
			Utilities.Debug($"GetSite took {sw.ElapsedMilliseconds} ms ({sw.ElapsedTicks} ticks)");
			return foundSite;
		}
		#endregion

		#region Private Methods
		private void AddSiteToGraph(Graph<Site, Coordinate, PointF> graph, csDelaunay.Delaunay.Site dSite)
		{
			Utilities.Debug($"vMap.Voronoi.Map.AddSiteToGraph({graph}, {dSite}) called.");

			if (dSite == null)
				throw new ArgumentException(@"The provided 'csDelaunay.Site' cannot be null (obviously).", nameof(dSite));

			// Grab the previous and current X,Y coordinates from the 'csDelaunay.Site' passed-in
			var prevCoords = new PointF(dSite.prevX, dSite.prevY);
			var coords = new PointF(dSite.X, dSite.Y);

			// Look up the old (pre-scaled) Site with the previous set of X,Y coordinates.
			// We do this in order to copy over important Site state to persist after the
			// re -scale.
			Site prevSite = null;
			if ((this.MapGraph?.PrimaryGraph != null) && this.MapGraph.PrimaryGraph.ContainsKey(prevCoords))
				prevSite = this.Sites[prevCoords].Data;

			// Populate the 'RegionCornerCoordinates' property by calling the
			// site's 'GetRegion' method passing in the plot bounds of the entire
			// Voronoi graph. I assume the plot bounds are used for clipping, if
			// necessary.
			var dRegionCoords = dSite.GetRegion(Voronoi.PlotBounds);

			// should have received a 'List<Vector2f>' from the above method call
			if((dRegionCoords == null) || (dRegionCoords.Count <= 0))
				return;

			// Capture the region coordinates for this site. These will be used to
			// render the site region.
			var i = 0;
			var regCoords = new PointF[dSite.RegionCornerCoordinates.Count];

			foreach (var dRegionCoord in dRegionCoords)
				regCoords[i++] = new PointF(dRegionCoord.X, dRegionCoord.Y);

			// Grab a few other site properties.
			var color = dSite.Color;
			var weight = dSite.Weight;

			// Create and add this site to the site graph as a 'GraphVertex' object.
			var newSite = new Site(coords: coords, regionPoints: regCoords ) { Weight = weight, DefaultColor = color };
			newSite.SiteUpdated += OnSiteUpdated;
			// copy any state from the coorelated site in the previous MapGraph (pre-scaling)
			if (prevSite != null)
				newSite.Copy(prevSite);

			// add this site to the MapGraph's PrimaryGraph ('Sites' graph)
			var graphVertex = new GraphVertex<Site, Coordinate>(newSite);
			graph.PrimaryGraph?.Add(graphVertex.Data.Point, graphVertex);
		}

		private void OnSiteUpdated(object sender, EventArgs e)
		{
			this.SiteUpdated?.Invoke(sender, e);
		}

		private static void AddVertexToGraph(Graph<Site, Coordinate, PointF> graph, LineSegment dLineSegment)
		{
			Utilities.Debug($"vMap.Voronoi.Map.AddVertexToGraph({graph}, {dLineSegment}) called.");

			GraphVertex <Coordinate, Site> existingStart;
			graph.SecondaryGraph.TryGetValue(new PointF(dLineSegment.P0.X, dLineSegment.P0.Y), out existingStart);

			GraphVertex<Coordinate, Site> existingEnd;
			graph.SecondaryGraph.TryGetValue(new PointF(dLineSegment.P1.X, dLineSegment.P1.Y), out existingEnd);

			// if I've found *both* endpoints in the 'Vertices' graph, we've already
			// added this line before, skip it...
			if((existingStart != null) && (existingEnd != null))
			{
				// Check to see if the two found points (start and end0 are conected
				// by an edge, if so this is a duplicatd line
				GraphEdge<Coordinate, Site> foundEdge;
				if(existingStart.Edges.TryGetValue(existingStart.GetHashCode() + existingEnd.GetHashCode(), out foundEdge))
					return;
			}

			// otherwise, either the start or end is new. Find out which and create
			// a new 'GraphVertex<Vertex>' object for one or both...
			var start = existingStart ?? new GraphVertex<Coordinate, Site>(new Coordinate(dLineSegment.P0.X, dLineSegment.P0.Y, 0F));
			var end = existingEnd ?? new GraphVertex<Coordinate, Site>(new Coordinate(dLineSegment.P1.X, dLineSegment.P1.Y, 0F));

			// Now create a 'GraphEdge<Coordinate, Site>' connecting the start and end and hook
			// it up.
			var edge = new GraphEdge<Coordinate, Site>(start, end, 0F);
			start.AddEdge(edge);
			end.AddEdge(edge);

			// Finally, add the two vertices to the graph (if necessary).
			if(existingStart == null)
				graph.SecondaryGraph.Add(start.Data.Point, start);

			if(existingEnd == null)
				graph.SecondaryGraph.Add(end.Data.Point, end);
		}
		private void ScaleNoiseMap(Rectangle plotBounds)
		{
			Utilities.Debug($"vMap.Voronoi.Map.ScaleNoiseMap({plotBounds}) called.");

			if (_noiseMap == null)
				return;

			var horizAspect = (double)(_noiseMap.GetLength(0)) / plotBounds.Width;
			var vertAspect = (double)(_noiseMap.GetLength(1)) / plotBounds.Height;
			_noiseMap = ScaleNoiseMap(_noiseMap, horizAspect, vertAspect);
		}
		public static double[,] ScaleNoiseMap(double[,] oldNoiseMap, double horizAspect = 1.0, double vertAspect = 1.0)
		{
			Utilities.Debug($"vMap.Voronoi.Map.ScaleNoiseMap({oldNoiseMap}, {horizAspect}, {vertAspect}) called.");

			var newWidth    = (int)(oldNoiseMap.GetLength(0) / horizAspect);
			var newHeight   = (int)(oldNoiseMap.GetLength(1) / vertAspect);
			var newNoiseMap = new double[newWidth, newHeight];

			for (var x = 0; x < newWidth; x++)
				for(var y = 0; y < newHeight; y++)
				{
					newNoiseMap[x, y] = oldNoiseMap[(int)(x * horizAspect), (int)(y * vertAspect)];
				}
			return newNoiseMap;
		}
		private void AssignElevations()
		{
			Utilities.Debug("vMap.Voronoi.Map.AssignElevations() called.");

			if (this.Sites == null)
				return;

			if((this.NoiseMap == null) || (this.NoiseMap.Length <= 0))
				return;

			foreach(var site in this.Sites.Values)
			{
				SiteType siteType;
				double elevation;
				var passable = true;
				var intensity = (int)this.NoiseMap[(int)site.Data.Point.X, (int)site.Data.Point.Y];

				if(intensity <= this.ElevationThresholds.Ocean)
				{
					passable = false;
					siteType = SiteType.Ocean;
					elevation = 0.0;
				}
				else if(intensity <= this.ElevationThresholds.Beach)
				{
					siteType = SiteType.Beach;
					elevation = 1.0;
				}
				else if(intensity <= this.ElevationThresholds.Lowland)
				{
					siteType = SiteType.Lowland;
					elevation = 2.0;
				}
				else if(intensity <= this.ElevationThresholds.Highland)
				{
					siteType = SiteType.Highland;
					elevation = 3.0;
				}
				else
				{
					passable = false;
					siteType = SiteType.Mountain;
					elevation = 4.0;
				}

				if (!passable)
				{
					site.Data.AddState(SiteState.Impassable);
					this.MapGraph.PrimaryWalls.Add(site.Data);
				}
				else
				{
					site.Data.RemoveState(SiteState.Impassable);
					this.MapGraph.PrimaryWalls.Remove(site.Data);
				}
				site.Data.SiteType = siteType;
				site.Data.Elevation = elevation;
			}
		}
		#endregion
	}
}