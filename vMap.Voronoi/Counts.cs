using System;
using System.Collections.Generic;
using System.Linq;

namespace vMap.Voronoi
{
	public class Counts
	{
		public HashSet<MetricItem> Items { get; set; }
		public MetricItem[] DisplayableItems
		{
			get { return this.Items?.Where(i => i.Display).ToArray(); }
		}

		public Counts()
		{
			this.Items = new HashSet<MetricItem>();
		}
		public void Add(string key, long value, bool display = true)
		{
			if(this.Items.Any(ci => ci.Key == key))
			{
				var item = this.Items.First(ci => ci.Key == key);
				item.Value = value;
			}
			else
				this.Items.Add(new MetricItem(key, value));
		}
	}
}