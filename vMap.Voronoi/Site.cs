#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
using System;
using System.Drawing;
using Color = System.Drawing.Color;

namespace vMap.Voronoi
{
	public class Site
	{
		public event EventHandler SiteUpdated;

		public int SiteIndex { get; set; }
		public int VertexBufferIndex { get; set; }
		public SiteState State { get; set; }
		public SiteType SiteType { get; set; }
		public double Elevation { get; set; }
		public PointF PrevCoordinates { get; set; }
		public PointF Point { get; }
		public PointF[] RegionPoints { get; set; }
		public float Weight { get; set; }
		public Color DefaultColor { get; set; }
		public bool Hover => this.IsSiteState(SiteState.Hover);
		public bool Current => this.IsSiteState(SiteState.Current);
		public Site(PointF coords, PointF[] regionPoints)
		{
			Utilities.Debug($"vMap.Voronoi.Site Constructor({coords}, {regionPoints}) called.");

			this.Point = coords;
			this.RegionPoints = regionPoints;
		}
		public void Copy(Site oldSite)
		{
			Utilities.Debug($"vMap.Voronoi.Site.Copy({oldSite}) called.");

			this.PrevCoordinates = oldSite.Point;
			this.Elevation		 = oldSite.Elevation;
			this.Weight			 = oldSite.Weight;
			this.SiteType		 = oldSite.SiteType;
			this.State           = oldSite.State;
		}
		public void AddState(SiteState state)
		{
			Utilities.Debug($"vMap.Voronoi.Site.AddState({state}) called.");
			this.State |= state;
			SiteUpdated?.Invoke(this, EventArgs.Empty);
		}
		public void RemoveState(SiteState state)
		{
			Utilities.Debug($"vMap.Voronoi.Site.RemoveState({state}) called.");
			this.State &= ~state;
			SiteUpdated?.Invoke(this, EventArgs.Empty);
		}
		public void Enter()
		{
			Utilities.Debug($"vMap.Voronoi.Site.Enter() called.");

			this.AddState(SiteState.Visited);
			this.AddState(SiteState.Current);
		}
		public void Exit()
		{
			Utilities.Debug($"vMap.Voronoi.Site.Exit() called.");

			this.RemoveState(SiteState.Current);
		}
		public Color GetColor()
		{
			Utilities.Debug($"vMap.Voronoi.Site.GetColor() called.");

			var result = MapColors.GetColor(this.SiteType);

			if((this.State & SiteState.Current) == SiteState.Current)
				result = Color.ForestGreen;
			else if((this.State & SiteState.Visited) == SiteState.Visited)
				result = Color.LightGreen;
			//else if ((this.State & SiteState.Frontier) == SiteState.Frontier)
			//	result = Color.DarkBlue;
			else if ((this.State & SiteState.AStarSearchStart) == SiteState.AStarSearchStart)
				result = Color.Red;
			else if ((this.State & SiteState.AStarSearchGoal) == SiteState.AStarSearchGoal)
				result = Color.Orange;
			else if ((this.State & SiteState.Wall) == SiteState.Wall)
				result = Color.Black;
			else if((this.State & SiteState.Path) == SiteState.Path)
				result = Color.DarkGray;
			
			if ((this.State & SiteState.Hover) == SiteState.Hover)
				result = Color.Yellow;

			return result;
		}
		public Microsoft.Xna.Framework.Color GetXnaColor()
		{
			Utilities.Debug($"vMap.Voronoi.Site.GetXnaColor() called.");

			var color = GetColor();
			return new Microsoft.Xna.Framework.Color(color.R, color.G, color.B);
		}
		public bool IsSiteState(SiteState targetState)
		{
			Utilities.Debug($"vMap.Voronoi.Site.IsSiteState({targetState}) called.");

			return (this.State & targetState) == targetState;
		}
		public override bool Equals(object obj)
		{
			return this.Point.Equals(((Site)obj).Point);
		}
		public override string ToString()
		{
			return $"Site: {this.State}, {this.SiteType}, {this.Elevation}, {this.Point}";
        }
	}
}