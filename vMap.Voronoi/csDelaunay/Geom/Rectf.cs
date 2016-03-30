using System;

namespace vMap.Voronoi.csDelaunay.Geom
{
	[Serializable]
	public struct Rectf
	{
		public Rectf(float x, float y, float width, float height)
		{
			this.X = x;
			this.Y = y;
			this.Width = width;
			this.Height = height;
		}

		#region Public Properties
		public static readonly Rectf Zero = new Rectf(0, 0, 0, 0);
		public static readonly Rectf One = new Rectf(1, 1, 1, 1);
		public float X;
		public float Y;
		public float Width;
		public float Height;
		public float	Left		=> this.X;
		public float	Right		=> this.X + this.Width;
		public float	Top			=> this.Y;
		public float	Bottom		=> this.Y + this.Height;
		public Vector2f TopLeft		=> new Vector2f(this.Left, this.Top);
		public Vector2f BottomRight => new Vector2f(this.Right, this.Bottom);
		#endregion
	}
}