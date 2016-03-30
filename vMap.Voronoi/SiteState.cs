using System;

namespace vMap.Voronoi
{
	[Flags]
	public enum SiteState
	{
		Unknown		= 0x0,
		Hover		= 0x1,
		Current		= 0x2,

		Impassable  = 0x4,
		Wall        = 0x8,

		Visited		= 0x10,
		Frontier	= 0x20,
		AStarSearchStart = 0x40,
		AStarSearchGoal = 0x80,
		Path = 0x100
	}
}