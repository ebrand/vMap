using System;

namespace vMap.Voronoi
{
	public delegate double InterpolationFunction2D(double a, double b, double x);

	public delegate double InterpolationFunctionCubic(double beforeA, double a, double b, double afterB, double x);

	public class Interpolate
	{
		public static double Linear(double a, double b, double x)
		{
			return a * (1 - x) + (b * x);
		}
		public static double Cosine(double a, double b, double x)
		{
			var ft = x * Math.PI;
			var f = (1 - Math.Cos(ft)) * 0.5;
			return a * (1 - f) + (b * f);
		}
		public static double Cubic(double beforeA, double a, double b, double afterB, double x)
		{
			var p = afterB - b - beforeA - a;
			var q = beforeA - a - p;
			var r = b - beforeA;
			var s = a;

			return p * Math.Pow(x, 3) + q * Math.Pow(x, 2) + r * x + s;
		}
	}
}