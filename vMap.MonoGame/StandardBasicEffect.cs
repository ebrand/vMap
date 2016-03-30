using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace vMap.MonoGame
{
	public class StandardBasicEffect : BasicEffect
	{
		public StandardBasicEffect(GraphicsDevice graphicsDevice) : base(graphicsDevice)
		{
			this.VertexColorEnabled = true;
			this.Projection = Matrix.CreateOrthographicOffCenter(0, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height, 0, 0, 1);
		}
		public StandardBasicEffect(BasicEffect effect) : base(effect)
		{}
		public new BasicEffect Clone()
		{
			return new StandardBasicEffect(this);
		}
	}
}