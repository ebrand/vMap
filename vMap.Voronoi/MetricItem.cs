using System;

namespace vMap.Voronoi
{
	public class MetricItem
	{
		public string Key { get; set; }
		public double Value { get; set; }
		public bool Display { get; set; }
		public MetricItem(string key, double value, bool display = true)
		{
			this.Key = key;
			this.Value = value;
			this.Display = display;
		}
	}
}