using System;
using System.Collections.Generic;
using System.Linq;
using vMap.Voronoi.csDelaunay.Geom;

namespace vMap.Voronoi.csDelaunay.Delaunay
{
	[Serializable]
	public class SiteList
	{
		#region Private Fields
		private readonly List<Site> _sites;
		private int _currentIndex;
		private bool _sorted;
		#endregion

		#region Constructors
		public SiteList(int capacity)
		{
			_sites = new List<Site>(capacity);
			_sorted = false;
		}
		public SiteList()
		{
			_sites = new List<Site>();
			_sorted = false;
		}
		#endregion

		#region Public Methods
		public int Add(Site site)
		{
			_sorted = false;
			_sites.Add(site);
			return _sites.Count;
		}
		public int Count()
		{
			return _sites.Count;
		}
		public Site Next()
		{
			if (!_sorted)
				throw new Exception("SiteList.Next(): sites have not been sorted");
			return _currentIndex < _sites.Count ? _sites[_currentIndex++] : null;
		}
		public Rectf GetSitesBounds()
		{
			if (!_sorted)
			{
				this.SortList();
				this.ResetListIndex();
			}

			if (_sites.Count == 0)
				return Rectf.Zero;

			var xMin = float.MaxValue;
			var xMax = float.MinValue;

			foreach (var site in _sites)
			{
				if (site.X < xMin) xMin = site.X;
				if (site.X > xMax) xMax = site.X;
			}
			
			// here's where we assume that the sites have been sorted on y:
			var yMin = _sites[0].Y;
			var yMax = _sites[_sites.Count - 1].Y;

			return new Rectf(xMin, yMin, xMax - xMin, yMax - yMin);
		}
		public List<Vector2f> GetSiteCoords()
		{
			return _sites.Select(site => site.Coordinate).ToList();
		}
		public List<Circle> GetCircles()
		{
			// return the largest circle centered at each site that fits in its region;
			// if the region is infinite, return a circle of radius 0.
			var circles = new List<Circle>();
			foreach (var site in _sites)
			{
				var radius = 0F;
				var nearestEdge = site.GetNearestEdge();

				if (!nearestEdge.IsPartOfConvexHull())
					radius = nearestEdge.GetSitesDistance() * 0.5f;

				circles.Add(new Circle(site.X, site.Y, radius));
			}
			return circles;
		}
		public List<List<Vector2f>> GetRegions(Rectf plotBounds)
		{
			return _sites.Select(site => site.GetRegion(plotBounds)).ToList();
		}
		public void ResetListIndex()
		{
			_currentIndex = 0;
		}
		public void SortList()
		{
			Site.SortSites(_sites);
			_sorted = true;
		}
		public void Dispose()
		{
			_sites.Clear();
		}
		#endregion
	}
}