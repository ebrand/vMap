using Microsoft.Xna.Framework.Graphics;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace vMap.MonoGame
{
	public static class BitmapExtensions
	{
		public static Texture2D ToTexture2D(this Bitmap bitmap, GraphicsDevice graphicsDevice)
		{
			var bitmapData = bitmap.LockBits(
				new System.Drawing.Rectangle(
					0,
					0,
					bitmap.Width,
					bitmap.Height
				),
				ImageLockMode.ReadOnly,
				PixelFormat.Format32bppArgb
			);

			int bufferSize = bitmapData.Height * bitmapData.Stride;
			//create data buffer 
			byte[] bytes = new byte[bufferSize];
			// copy bitmap data into buffer
			Marshal.Copy(bitmapData.Scan0, bytes, 0, bytes.Length);

			Texture2D t2d = new Texture2D(graphicsDevice, bitmap.Width, bitmap.Height);
			t2d.SetData<byte>(bytes);
			// unlock the bitmap data
			bitmap.UnlockBits(bitmapData);

			return t2d;
		}
	}
}