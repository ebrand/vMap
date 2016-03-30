using System;

namespace vMap.Voronoi
{
	public class SiteStateChangedEventArgs : EventArgs
	{
		public Site Site { get; }
		public SiteStateChangedEventArgs(Site site)
		{
			this.Site = site;
		}
	}
}