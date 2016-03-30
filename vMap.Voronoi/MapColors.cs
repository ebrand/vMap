using System;
using System.Drawing;

namespace vMap.Voronoi
{
	public class MapColors
	{
		/*
			OCEAN: 0x44447a,
			COAST: 0x33335a,
			LAKESHORE: 0x225588,
			LAKE: 0x336699,
			RIVER: 0x225588,
			MARSH: 0x2f6666,
			ICE: 0x99ffff,
			BEACH: 0xa09077,
			ROAD1: 0x442211,
			ROAD2: 0x553322,
			ROAD3: 0x664433,
			BRIDGE: 0x686860,
			LAVA: 0xcc3333,
			GRASSLAND: 0x88aa55,
			TROPICAL_RAIN_FOREST: 0x337755,
		*/
		public static Color Ocean = Color.FromArgb(68, 68, 122);
		public static Color Beach = Color.FromArgb(160, 144, 119);
		public static Color Lowland = Color.FromArgb(136, 170, 85);
		public static Color Highland = Color.FromArgb(51, 119, 85);
		public static Color Mountain = Color.FromArgb(255, 255, 255);

		public static Color GetColor(SiteType siteType)
		{
			switch(siteType)
			{
				case SiteType.Ocean:
					return MapColors.Ocean;
				case SiteType.Beach:
					return MapColors.Beach;
				case SiteType.Lowland:
					return MapColors.Lowland;
				case SiteType.Highland:
					return MapColors.Highland;
				case SiteType.Mountain:
					return MapColors.Mountain;
				case SiteType.Unknown:
					return Color.White;
				default:
					throw new ArgumentOutOfRangeException(nameof(siteType), siteType, null);
			}
		}
	}
}