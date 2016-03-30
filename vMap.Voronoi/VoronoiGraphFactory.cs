using System;
using System.Collections.Generic;
using System.Drawing;
using vMap.Voronoi.csDelaunay.Geom;

namespace vMap.Voronoi
{
	public class VoronoiGraphFactory : IProvideMetrics
	{
		public static readonly VoronoiGraphFactory Instance = new VoronoiGraphFactory();
		private VoronoiGraphFactory()
		{
			this.MetricsName = "Voronoi Graph Factory";
			this.Timings = new Timings();
			this.Counts = new Counts();
		}

		public string MetricsName { get; set; }
		public Timings Timings { get; set; }
		public Counts Counts { get; set; }

		public csDelaunay.Delaunay.Voronoi Initialize(int siteCount, Rectangle plotBounds)
		{
			var points = GetPoints(siteCount, plotBounds);
			return this.Initialize(points, plotBounds);
		}
		public csDelaunay.Delaunay.Voronoi Initialize(PointF[] points, Rectangle plotBounds)
		{
			var sites = new List<Vector2f>();
			foreach(var p in points)
				sites.Add(new Vector2f(p.X, p.Y));

			this.Timings.Start();
			var result = new csDelaunay.Delaunay.Voronoi(
				sites,
				new Rectf(plotBounds.X, plotBounds.Y, plotBounds.Width, plotBounds.Height)
			);
			this.Timings.Stop("Initialize Graph");

			this.Counts.Add("Sites", result.SitesIndexedByLocation.Count);
			this.Counts.Add("Regions", result.GetRegions().Count);
			this.Counts.Add("Edges", result.Edges.Count);
			this.Counts.Add("Line Segments", result.GetLineSegments().Count);
			this.Counts.Add("Triangles", result.Triangles.Count);

			return result;
		}
		public PointF[] GetPoints(int siteCount, Rectangle plotBounds)
		{
			var rnd = new Random();
			var points = new PointF[siteCount];
			for (var i = 0; i < siteCount; i++)
			{
				var x = plotBounds.Width * rnd.NextDouble();
				var y = plotBounds.Height * rnd.NextDouble();
				points[i] = new PointF((float)x, (float)y);
			}
			return points;
		}
	}
}