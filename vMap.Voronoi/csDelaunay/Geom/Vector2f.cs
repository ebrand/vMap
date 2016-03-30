using System;
using System.Drawing;

// Recreation of the UnityEngine.Vector3, so it can be used in other thread
namespace vMap.Voronoi.csDelaunay.Geom
{
	[Serializable]
	public class Vector2f
	{
		#region Constructors
		public Vector2f(float x, float y)
		{
			this.X = x;
			this.Y = y;
			//this.prevX = x;
			//this.prevY = y;
			this.Color = Color.Empty;
		}
		public Vector2f(double x, double y)
		{
			this.X = (float)x;
			this.Y = (float)y;
			//this.prevX = (float)x;
			//this.prevY = (float)y;
			this.Color = Color.Empty;
		}
		public Vector2f(float x, float y, float prevX, float prevY)
		{
			this.X     = x;
			this.Y     = y;
			this.prevX = prevX;
			this.prevY = prevY;
			this.Color = Color.Empty;
		}
		public Vector2f(double x, double y, double prevX, double prevY)
		{
			this.X     = (float)x;
			this.Y     = (float)y;
			this.prevX = (float)prevX;
			this.prevY = (float)prevY;
			this.Color = Color.Empty;
		}
		#endregion

		#region Public Properties
		public float X { get; set; }
		public float Y { get; set; }
		public float prevX { get; set; }
		public float prevY { get; set; }
		public Color Color { get; set; }
		public static readonly Vector2f Zero  = new Vector2f(0, 0);
		public static readonly Vector2f One   = new Vector2f(1, 1);
		public static readonly Vector2f Right = new Vector2f(1, 0);
		public static readonly Vector2f Left  = new Vector2f(-1, 0);
		public static readonly Vector2f Up    = new Vector2f(0, 1);
		public static readonly Vector2f Down  = new Vector2f(0, -1);
		#endregion

		#region Public Methods
		public float Magnitude => (float)Math.Sqrt(this.X * this.X + this.Y * this.Y);
		public void Normalize()
		{
			var magnitude = this.Magnitude;
			this.X /= magnitude;
			this.Y /= magnitude;
		}
		public static Vector2f Normalize(Vector2f a)
		{
			var magnitude = a.Magnitude;
			return new Vector2f(a.X / magnitude, a.Y / magnitude);
		}
		public override bool Equals(object other)
		{
			if (!(other is Vector2f))
				return false;

			var vectorToCompare = (Vector2f)other;
			return (Math.Abs(X - vectorToCompare.X) < Utilities.EPSILON) && (Math.Abs(Y - vectorToCompare.Y) < Utilities.EPSILON);
		}
		public override string ToString()
		{
			return $"[V2f] {this.X},{this.Y} - {this.prevX},{this.prevY}";
		}
		public override int GetHashCode()
		{
			return this.X.GetHashCode() ^ (this.Y.GetHashCode() << 2);
		}
		public float GetDistanceSquare(Vector2f v)
		{
			return Vector2f.GetDistanceSquare(this, v);
		}
		public static float GetDistanceSquare(Vector2f a, Vector2f b)
		{
			var cx = b.X - a.X;
			var cy = b.Y - a.Y;
			return cx * cx + cy * cy;
		}
		public static Vector2f GetMin(Vector2f a, Vector2f b)
		{
			return new Vector2f(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y));
		}
		public static Vector2f GetMax(Vector2f a, Vector2f b)
		{
			return new Vector2f(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y));
		}
		#endregion

		#region Operator Implementation
		public static bool     operator == (Vector2f a, Vector2f b)
		{
			if(a != null && b == null)
				return false;
			if(a == null && b != null)
				return false;
		
			return (Math.Abs(a.X - b.X) < Utilities.EPSILON) && (Math.Abs(a.Y - b.Y) < Utilities.EPSILON);
		}
		public static bool     operator != (Vector2f a, Vector2f b)
		{
			return (Math.Abs(a.X - b.X) > Utilities.EPSILON) || (Math.Abs(a.Y - b.Y) > Utilities.EPSILON);
		}
		public static Vector2f operator -  (Vector2f a, Vector2f b)
		{
			return new Vector2f(a.X - b.X, a.Y - b.Y);
		}
		public static Vector2f operator +  (Vector2f a, Vector2f b)
		{
			return new Vector2f(a.X + b.X, a.Y + b.Y);
		}
		public static Vector2f operator *  (Vector2f a, int i)
		{
			return new Vector2f(a.X * i, a.Y * i);
		}
		#endregion
	}
}