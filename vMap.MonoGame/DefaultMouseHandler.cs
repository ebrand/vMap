using System;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using vMap.Voronoi;

namespace vMap.MonoGame
{
	public class DefaultMouseHandler : IMouseHandler
	{
		private readonly MapConfig _config;
		
		public DefaultMouseHandler(MapConfig config)
		{
			_config = config;
			//_config.MouseHoverSite = hoverSite;
		}

		public void HandleMouse_ControlLeftButtonClick()
		{
			// if the current site isn't already 'Impassable', make it so
			if (_config.MouseHoverSite.Data.IsSiteState(SiteState.Wall))
				return;

			_config.MouseHoverSite.Data.AddState(SiteState.Impassable);
			_config.MouseHoverSite.Data.AddState(SiteState.Wall);
			_config.Map.MapGraph.PrimaryWalls.Add(_config.MouseHoverSite.Data);
		}
		public void HandleMouse_ControlRightButtonClick()
		{
			_config.Rogue = new Agent { Location = new Vector2(_config.MouseHoverSite.Data.Point.X, _config.MouseHoverSite.Data.Point.Y), Site = _config.MouseHoverSite.Data };
			foreach (var agent in _config.Agents)
				_config.StartAStarSearchDelegate.Invoke(agent.Site, _config.Rogue.Site);
		}
		public void HandleMouse_ShiftLeftButtonClick()
		{ }
		public void HandleMouse_ShiftRightButtonClick()
		{ }
		public void HandleMouse_LeftButtonClick()
		{
			// left-button click = set the current Site as the A* search start
			if ((_config.Map.SearchStart != null) && _config.Map.SearchStart.Equals(_config.MouseHoverSite.Data))
				return;

			_config.Map.SearchStart?.RemoveState(SiteState.AStarSearchStart);
			_config.MouseHoverSite.Data.AddState(SiteState.AStarSearchStart);
			_config.Map.SearchStart = _config.MouseHoverSite.Data;
		}
		public void HandleMouse_RightButtonClick()
		{
			// right-button click = set the current Site as the A* search goal
			if ((_config.Map.SearchGoal != null) && _config.Map.SearchGoal.Equals(_config.MouseHoverSite.Data))
				return;

			_config.Map.SearchGoal?.RemoveState(SiteState.AStarSearchGoal);
			_config.MouseHoverSite.Data.AddState(SiteState.AStarSearchGoal);
			_config.Map.SearchGoal = _config.MouseHoverSite.Data;
		}
		public void HandleMouse_ControlHover()
		{
			if (Form.ActiveForm != null)
				Form.ActiveForm.Text = _config.MouseHoverSite.Data.ToString();
		}
		public void HandleMouse_ShiftHover()
		{ }
		public void HandleMouse_Hover()
		{
			if (Form.ActiveForm != null)
				Form.ActiveForm.Text = "vMap.MonoGame";
		}
	}
}