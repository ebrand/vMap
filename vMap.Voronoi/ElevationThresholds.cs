using System;

namespace vMap.Voronoi
{
	public class ElevationThresholds
	{
		public int Ocean { get; set; } //=> 30;
		public int Beach { get; set; } //=> 75;
		public int Lowland { get; set; } //=> 175;
		public int Highland { get; set; } //=> 240;
		public int Mountain { get; set; } //=> 255;
		public ElevationThresholds()
		{
			this.Ocean = 30;
			this.Beach = 75;
			this.Lowland = 175;
			this.Highland = 240;
			this.Mountain = 255;
		}
		public override string ToString()
		{
			return $"Ocean: {this.Ocean}, Beach: {this.Beach}, Lowland: {this.Lowland}, Highland: {this.Highland}, Mountain: {this.Mountain}";
		}
	}
}