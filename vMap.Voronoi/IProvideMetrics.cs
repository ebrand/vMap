using System;

namespace vMap.Voronoi
{
	public interface IProvideMetrics : IProvideTimings, IProvideCounts
	{
		string MetricsName { get; set; }
	}
}