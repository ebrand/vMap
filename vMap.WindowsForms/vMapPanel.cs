using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using vMap.Voronoi;

namespace vMap.WindowsForms
{
	public partial class vMapPanel : UserControl
	{
		private readonly Map _map;

		public vMapPanel()
		{
			InitializeComponent();
			_map = new Map(1000, this.ClientRectangle);
		}
		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);
			
			_map.Scale(this.ClientRectangle);

			var g = this.CreateGraphics();
			g.Clear(Color.White);
			using (var pen = new Pen(Color.Black))
			{
				var sw = new Stopwatch();
				sw.Start();

				foreach (var s in _map.Sites.Values)
					g.DrawPolygon(pen, s.Data.RegionPoints);

				sw.Stop();
				Utilities.Debug($"Map render: {sw.ElapsedMilliseconds} ms ({sw.ElapsedTicks} ticks).", VoronoiTraceLevel.Info);
			}
		}
	}
}