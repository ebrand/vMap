using System;
using System.Drawing;
using System.Windows.Forms;

namespace vMap.MonoGame
{
	public partial class NoiseMap : Form
	{
		public NoiseMap(Image image)
		{
			InitializeComponent();

			this.Width = image.Width;
			this.Height = image.Height;
			pictureBox.Image = image;
			this.Refresh();
		}
	}
}