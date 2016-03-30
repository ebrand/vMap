using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace vMap.Voronoi
{
	public class Timings
	{
		private readonly Stopwatch _sw;

		public HashSet<MetricItem> Items { get; set; }
		public MetricItem[] DisplayableItems
		{
			get { return this.Items?.Where(i => i.Display).ToArray(); }
		} 
		public Timings()
		{
			this.Items = new HashSet<MetricItem>();
			_sw = new Stopwatch();
		}
		public void Reset()
		{
			_sw.Reset();
		}
		public void Start()
		{
			_sw.Start();
		}
		public void Stop(string key, bool average = false)
		{
			_sw.Stop();
			this.Add(key, _sw.ElapsedMilliseconds, average);
			_sw.Reset();
		}
		public void StopAverage(string key)
		{
			this.Stop(key, true);
		}
		public void Add(string key, double value, bool average = false)
		{
			if(this.Items == null)
				return;

			if(this.Items.Any(ti => ti.Key == key))
			{
				var item = this.Items.First(ci => ci.Key == key);
				if(average)
				{
					var itemAvgCurrentTotal = this.Items.First(ci => ci.Key == key + "[avg.currentTotal]");
					var itemAvgCount        = this.Items.First(ci => ci.Key == key + "[avg.count]");
					itemAvgCount.Value++;
					itemAvgCurrentTotal.Value = itemAvgCurrentTotal.Value + value;
					item.Value = itemAvgCurrentTotal.Value / itemAvgCount.Value;
				}
				else
				{
					item.Value = value;
				}
			}
			else
			{
				this.Items.Add(new MetricItem(key, value));
				this.Items.Add(new MetricItem(key + "[avg.currentTotal]", value, display: false));
				this.Items.Add(new MetricItem(key + "[avg.count]", 1, display: false));
			}
		}
	}
}