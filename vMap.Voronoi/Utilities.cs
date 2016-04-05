#pragma warning disable 1591 // disable 'missing XML comments' compiler warnings
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Color = System.Drawing.Color;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Point = System.Drawing.Point;
using Rectangle = System.Drawing.Rectangle;

namespace vMap.Voronoi
{
	public static class Utilities
	{
		public static double MathPrecision = 1E-10;
		public static VoronoiTraceLevel TraceLevel = VoronoiTraceLevel.DetailedTiming;

		private static readonly Dictionary<string, Stopwatch> _stopwatches = new Dictionary<string, Stopwatch>();

		private static readonly Random _random = new Random();
		public static double GetRandomDoubleBetweenZeroAndOne()
		{
			return _random.NextDouble();
		}
		public static int GetRandomNumberInRange(int inclusiveLowerBound, int inclusiveUpperBound)
		{
			return _random.Next(inclusiveLowerBound, inclusiveUpperBound + 1);
		}
		public static int GetRandomInt()
		{
			return GetRandomNumberInRange(0, int.MaxValue - 1);
		}
		public static int RndPosNumUnder(int inclusiveUpperBound)
		{
			return _random.Next(0, inclusiveUpperBound + 1);
		}

		public static int GetRandomXAxisValue(Rectangle plotBounds)
		{
			return GetRandomNumberInRange(plotBounds.Left, plotBounds.Right);
		}
		public static int GetRandomYAxisValue(Rectangle plotBounds)
		{
			return GetRandomNumberInRange(plotBounds.Top, plotBounds.Bottom);
		}
		public static Vector2 GetRandomLocation(Rectangle plotBounds)
		{
			return new Vector2(GetRandomXAxisValue(plotBounds), GetRandomYAxisValue(plotBounds));
		}
		public static int IsLeft(Point a, Point b, Point c)
		{
			return ((b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X)) > 0 ? 1 : -1;
		}
		public static double Distance(Point a, Point b)
		{
			var dx = a.X - b.X;
			var dy = a.Y - b.Y;
			return Math.Sqrt(dx * dx + dy * dy);
		}
		public static Color RandomNarrowbandRed
		{
			get
			{
				var rgb = GetRandomNumberInRange(60, 90);
				return Color.FromArgb(rgb, 0, 0);
			}
		}
		public static Color RandomNarrowbandGreen
		{
			get
			{
				var rgb = GetRandomNumberInRange(60, 90);
				return Color.FromArgb(0, rgb, 0);
			}
		}
		public static Color RandomNarrowbandBlue
		{
			get
			{
				var rgb = GetRandomNumberInRange(60, 90);
				return Color.FromArgb(0, 0, rgb);
			}
		}
		public static Color RandomNarrowbandGray
		{
			get
			{
				var rgb = GetRandomNumberInRange(50, 150);
				return Color.FromArgb(rgb, rgb, rgb);
			}
		}
		public static Color RandomColor
		{
			get
			{
				var r = GetRandomNumberInRange(0, 255);
				var g = GetRandomNumberInRange(0, 255);
				var b = GetRandomNumberInRange(0, 255);
				return Color.FromArgb(r, g, b);
			}
		}
		public static Color RandomGray
		{
			get
			{
				var x = GetRandomNumberInRange(0, 255);
				return Color.FromArgb(x, x, x);
			}
		}
		public static double LinearInterpolation(double x, double y, double amt)
		{
			return x + (y - x) * amt;
		}
		public static double CosineInterpolation(double x, double y, double amt)
		{
			var f = (1 - Math.Cos(amt * Math.PI)) * 0.5;
			return x * (1 - f) + y * f;
		}
		public static double CubeInterpolation(double x0, double x1, double x2, double x3, double mu)
		{
			var mu2 = mu * mu;
			var a0 = x3 - x2 - x0 + x1;
			var a1 = x0 - x1 - a0;
			var a2 = x2 - x0;
			var a3 = x1;
			return (a0 * mu * mu2 + a1 * mu2 + a2 * mu + a3);
		}
		public static void SetRadioCheckedState(RadioButton rdo, bool value, object tag, EventHandler eventMethod)
		{
			if (rdo == null)
			{
				throw new ArgumentException(@"the 'RadioButton' is undefined.", nameof(rdo));
			}
			if (eventMethod == null)
			{
				throw new ArgumentException(@"the 'EventMethod' is undefined.", nameof(eventMethod));
			}

			rdo.CheckedChanged -= eventMethod;
			rdo.Checked = value;
			rdo.Tag = tag;
			rdo.CheckedChanged += eventMethod;
		}
		public static void SetCheckState(CheckBox chk, bool value, EventHandler eventMethod)
		{
			if (chk == null)
			{
				throw new ArgumentException(@"the 'CheckBox' is undefined.", nameof(chk));
			}
			if (eventMethod == null)
			{
				throw new ArgumentException(@"the 'EventMethod' is undefined.", nameof(eventMethod));
			}

			chk.CheckedChanged -= eventMethod;
			chk.Checked = value;
			chk.CheckedChanged += eventMethod;
		}
		public static bool IsPointInCircle(PointF center, double radius, PointF testPoint)
		{
			var intTestPoint = new PointF((int)testPoint.X, (int)testPoint.Y);

			if (!IsPointInRectangle(center, radius, intTestPoint))
				return false;

			var dx = center.X - testPoint.X;
			var dy = center.Y - testPoint.Y;
			dx *= dx;
			dy *= dy;
			var distanceSquared = dx + dy;
			var radiusSquared = radius * radius;

			return distanceSquared <= radiusSquared;
		}
		public static bool IsPointInRectangle(PointF center, double radius, PointF testPoint)
		{
			return (testPoint.X >= center.X - radius)
			    && (testPoint.X <= center.Y + radius)
			    && (testPoint.Y >= center.Y - radius)
			    && (testPoint.Y <= center.Y + radius);
		}
		public static bool IsInPolygon(PointF[] polygon, PointF point)
		{
			var isInside = false;
			for (int i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++)
				if (((polygon[i].Y > point.Y) != (polygon[j].Y > point.Y)) && (point.X < (polygon[j].X - polygon[i].X) * (point.Y - polygon[i].Y) / (polygon[j].Y - polygon[i].Y) + polygon[i].X))
					isInside = !isInside;
			return isInside;
		}
		public static double ConvertFromNoiseParameterValue(NoiseParameter param, int rawVal)
		{
			var result = 0.0;
			switch (param)
			{
				case NoiseParameter.Frequency:
					result = rawVal / 1000.0;
					break;
				case NoiseParameter.Lacunarity:
					result = rawVal / 4.0;
					break;
				case NoiseParameter.Persistence:
					result = rawVal / 4.0;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(param), param, null);
			}
			return result;
		}
		public static int ConvertToNoiseParameterTrackBar(NoiseParameter param, double rawVal)
		{
			var result = 0;
			switch (param)
			{
				case NoiseParameter.Frequency:
					result = (int)(rawVal * 1000);
					break;
				case NoiseParameter.Lacunarity:
					result = (int)(rawVal * 4);
					break;
				case NoiseParameter.Persistence:
					result = (int)(rawVal * 4);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(param), param, null);
			}
			return result;
		}
		public static float ScaleFloat(float rangeFloor, float rangeCeiling, float sampleLowerBound, float sampleUpperBound, float value)
		{
			return (value - sampleLowerBound) * (rangeCeiling - rangeFloor) / (sampleUpperBound - sampleLowerBound) + rangeFloor;
		}
		public static void Debug(string debugMsg, VoronoiTraceLevel traceLevel = VoronoiTraceLevel.Debug)
		{
			if(Utilities.TraceLevel >= traceLevel)
				System.Diagnostics.Debug.WriteLine(debugMsg);
		}
		public static void StartTimer(string name, VoronoiTraceLevel traceLevel = VoronoiTraceLevel.DetailedTiming)
		{
			if(Utilities.TraceLevel < traceLevel)
				return;

			var sw = new Stopwatch();

			if (!_stopwatches.ContainsKey(name))
				_stopwatches.Add(name, sw);
			else
				sw = _stopwatches[name];

			sw.Reset();
			sw.Start();
		}
		public static long StopTimer(string name, VoronoiTraceLevel traceLevel = VoronoiTraceLevel.DetailedTiming)
		{
			if (Utilities.TraceLevel < traceLevel)
				return 0;

			if (!_stopwatches.ContainsKey(name))
				return 0;

			var sw = _stopwatches[name];
			sw.Stop();
			return sw.ElapsedTicks;
		}
	}
}