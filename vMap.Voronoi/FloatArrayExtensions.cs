using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vMap.Voronoi
{
	public static class FloatArrayExtensions
	{
		public static float GetMin(this float[] input)
		{
			var min = float.MaxValue;
			foreach(var f in input)
			{
				if(f < min)
					min = f;
			}
			return min;
		}
		public static float GetMax(this float[] input)
		{
			var max = float.MinValue;
			foreach (var f in input)
			{
				if (f > max)
					max = f;
			}
			return max;
		}
	}
}