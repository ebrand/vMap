using System;
using vMap.Voronoi.csDelaunay.Geom;

namespace vMap.Voronoi.csDelaunay.Delaunay
{
	public interface ICoord
	{
		Vector2f Coordinate { get; set; }
	}
}