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
		Visited		= 0x8,

		Frontier	= 0x10,
		
		AStarSearchStart = 0x20,
		AStarSearchGoal = 0x40,

		AgentPath = 0x80
	}
}