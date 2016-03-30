using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace vMap.MonoGame
{
	public static class SpriteBatchExtensions
	{
		private static Texture2D _blankTexture;
		public static void LoadContent(GraphicsDevice graphicsDevice)
		{
			_blankTexture = new Texture2D(graphicsDevice, 1, 1, false, SurfaceFormat.Color);
			_blankTexture.SetData(new[] { Color.White });
		}
		public static void LoadContent(Texture2D blankTexture)
		{
			_blankTexture = blankTexture;
		}
		public static void DrawLineSegment(this SpriteBatch spriteBatch, Vector2 point1, Vector2 point2, Color color, float lineWidth)
		{
			var angle = (float)Math.Atan2(point2.Y - point1.Y, point2.X - point1.X);
			var length = Vector2.Distance(point1, point2);

			spriteBatch.Draw(
				_blankTexture,					// Texture2D
				point1,							// position
				null,							// source rectangle
				color,							// color
				angle,							// rotation
				Vector2.Zero,					// origin
				new Vector2(length, lineWidth),	// scale
				SpriteEffects.None,				// effects
				0								// layer depth
			);
		}
	}
}