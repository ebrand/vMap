using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using vMap.Voronoi;

using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;
using Color       = Microsoft.Xna.Framework.Color;
using Keys        = Microsoft.Xna.Framework.Input.Keys;

namespace vMap.MonoGame
{
	public delegate void StartAStarSearchDelegate(Site start, Site goal);

	public class vMapMain : Game
	{
		private readonly MapConfig _config;

		public vMapMain()
		{
			Utilities.Debug("vMapMain Constructor called.");

			// create a new MapConfig object passing it this instance of the 'Game' object
			_config = new MapConfig(this);

			// tell XNA to call the Update() method before Draw() every cycle
			// instead of at a regular interval
			this.IsFixedTimeStep = false;
		}

		protected override void Initialize()
		{
			Utilities.Debug("Initialize() called.");

			// the GraphicsDeviceManager.GraphicsDevice is not created
			// until *after* the 'Game' constructor is called
			_config.GfxDev = _config.GraphicsDeviceManager.GraphicsDevice;

			this.IsMouseVisible = true;
			Content.RootDirectory = "Content";

			// set the plot bounds
			_config.PlotBounds =
				new System.Drawing.Rectangle
				(
					_config.GfxDev.Viewport.TitleSafeArea.X,
					_config.GfxDev.Viewport.TitleSafeArea.Y,
					_config.GfxDev.Viewport.TitleSafeArea.Width,
					_config.GfxDev.Viewport.TitleSafeArea.Height
				);

			// initialize out 1x1 pixel image
			_config.Pixel = new Texture2D(_config.GfxDev, 1, 1);
			_config.Pixel.SetData<Color>(new Color[1] { Color.White });

			// Setup key handlers ///////////////////////////////////////////////////////////////////////////////
			this.CreateKeyHandlers();

			// Create a new map /////////////////////////////////////////////////////////////////////////////////
			this.CreateMap();

			// Create a noise map to auto-determine elevations //////////////////////////////////////////////////
			this.CreateNoiseMap();

			// Create Site triangle, site border and Delaunay line vertex arrays for the entire map /////////////
			this.CreateVertexArrays();

			// create agents, ensuring that each new agent is inside
			// a site and not on the border of two sites
			_config.Agents = new Agent[MapConfig.Constants.AGENT_COUNT];
			var count = 0;
			while(count < MapConfig.Constants.AGENT_COUNT)
			{
				var rndLoc = Utilities.GetRandomLocation(_config.PlotBounds);
				var agentSite = _config.Map.GetSite(rndLoc)?.Data;

				if((agentSite != null) && !agentSite.IsSiteState(SiteState.Impassable) && !agentSite.IsSiteState(SiteState.Wall))
					_config.Agents[count++] = new Agent { Location = rndLoc, Site = agentSite };
			}

			_config.StartAStarSearchDelegate = this.StartAgentSearch;

			// call base.Initialize() last!
			base.Initialize();
		}
		protected override void LoadContent()
		{
			Utilities.Debug("LoadContent() called.");
			Utilities.StartTimer("LoadContent");

			// Create a new SpriteBatch, which can be used to draw textures.
			_config.SpriteBatch = new SpriteBatch(base.GraphicsDevice);

			// fonts
			_config.FontSegoe8Bold     = Content.Load<SpriteFont>("Segoe_8_Bold");
			_config.FontSegoe10Regular = Content.Load<SpriteFont>("Segoe_10_Regular");
			_config.FontSegoe10Bold    = Content.Load<SpriteFont>("Segoe_10_Bold");
			_config.FontSegoe12Regular = Content.Load<SpriteFont>("Segoe_12_Regular");
			_config.FontSegoe12Bold    = Content.Load<SpriteFont>("Segoe_12_Bold");

			// help text
			_config.HelpText =
				$"LM:  Set Search Start{Environment.NewLine}" +
				$"RM:  Set Search Goal{Environment.NewLine}" +
				$"F1:  Borders{Environment.NewLine}" +
				$"F2:  Site Points{Environment.NewLine}" +
				$"F3:  Delaunay Triangulation{Environment.NewLine}" +
				$"F4:  Regenerate Noise{Environment.NewLine}" +
				$"{Environment.NewLine}" +
				$"F5:  Apply Lloyd Centroid Relaxation{Environment.NewLine}" +
				$"F6:  Start A* Search{Environment.NewLine}" +
				$"F7:  Clear Search Start/Goal/Visited{Environment.NewLine}" +
				$"F8:  Initialize Map/Clear Walls{Environment.NewLine}" +
				$"{Environment.NewLine}" +
				$"F9:  Decrease Sites by 1K{Environment.NewLine}" +
				$"F10: Increase Sites by 1K{Environment.NewLine}" +
				$"F11: Toggle Outline/Fill{Environment.NewLine}" +
				$"F12: Help Text{Environment.NewLine}" +
				$"{Environment.NewLine}" +
				 "ESC: Exit";

			_config.HelpText2 =
				$"CTRL-LM:  Create Wall{Environment.NewLine}" +
				$"CTRL-RM:  Set the Rogue Agent{Environment.NewLine}" +
				$"CTRL-F1:  Toggle Agents{Environment.NewLine}" +
				$"CTRL-F2:  -{Environment.NewLine}" +
				$"CTRL-F3:  -{Environment.NewLine}" +
				$"CTRL-F4:  -{Environment.NewLine}" +
				$"{Environment.NewLine}" +
				$"CTRL-F5:  -{Environment.NewLine}" +
				$"CTRL-F6:  -{Environment.NewLine}" +
				$"CTRL-F7:  -{Environment.NewLine}" +
				$"CTRL-F8:  -{Environment.NewLine}" +
				$"{Environment.NewLine}" +
				$"CTRL-F9:  -{Environment.NewLine}" +
				$"CTRL-F10: -{Environment.NewLine}" +
				$"CTRL-F11: -{Environment.NewLine}" +
				$"CTRL-F12: -{Environment.NewLine}";

			_config.TimingHeadingText =
				$"Site count:{Environment.NewLine}" +
				$"FPS:{Environment.NewLine}";

			if (Utilities.TraceLevel >= VoronoiTraceLevel.DetailedTiming)
				_config.TimingHeadingText +=
					$"{Environment.NewLine}" +

					$"Init:{Environment.NewLine}" +
					$" + Noise:{Environment.NewLine}" +
					$"Load content:{Environment.NewLine}" +
					$"Update:{Environment.NewLine}" +
					$" + Keyboard:{Environment.NewLine}" +
					$" + Mouse:{Environment.NewLine}" +

					$"{Environment.NewLine}" +

					$"Draw:{Environment.NewLine}";

			_config.AgentSprite = Content.Load<Texture2D>("agent");
			_config.RogueSprite = Content.Load<Texture2D>("rogue_agent");

			SpriteBatchExtensions.LoadContent(_config.GfxDev);
			_config.LoadContentTimeTicks = Utilities.StopTimer("LoadContent");
		}
		protected override void UnloadContent()
		{
			// TODO: Unload any non ContentManager content here
			Utilities.Debug("UnloadContent() called.");
		}
		protected override void Update(GameTime gameTime)
		{
			Utilities.Debug("vMapMain.Update() called.");
			Utilities.StartTimer("Update");
			
			this.UpdateMouseInput();
			this.UpdateKeyboardInput();
			base.Update(gameTime);

			_config.UpdateTimeTicks = Utilities.StopTimer("Update");
		}
		protected override void Draw(GameTime gameTime)
		{
			Utilities.StartTimer("Draw");

			_config.Fps.Update(gameTime);
			base.GraphicsDevice.Clear(MapConfig.Constants.BG_COLOR);
			_config.SpriteBatch.Begin();

			// handle all the Sites that have been altered and pushed onto the event queue for updating
			while (_config.SitesToUpdate.Count > 0)
				this.UpdateSite(_config.SitesToUpdate.Dequeue());

			// CTRL-F1: Show agents /////////////////////////////////////////////////////////////////////////////
			if(MapConfig.Constants.SHOW_AGENTS)
			{
				if(_config.Rogue != null)
					this.DrawRogue();
				this.DrawAgents();
			}

			// F11: Fill sites //////////////////////////////////////////////////////////////////////////////////
			if (MapConfig.Constants.FILL_SITES)
				this.RenderBuffer(_config.SiteTriangleVertices, PrimitiveType.TriangleList, _config.SiteTriangleCount);

			// F4: Show Delaunay triangulation lines ////////////////////////////////////////////////////////////
			if(MapConfig.Constants.SHOW_DELAUNAY)
				this.RenderBuffer(_config.DelaunayLineVertices, PrimitiveType.LineList, _config.DelaunayLineCount);

			// F2: Show site centerpoints ///////////////////////////////////////////////////////////////////////
			if(MapConfig.Constants.SHOW_SITE_CENTERPOINTS)
				foreach(var site in _config.Map.Sites.Values.Select(s => s.Data))
					_config.SpriteBatch.Draw(_config.Pixel, new Vector2(site.Point.X, site.Point.Y), MapConfig.Constants.CENTERPOINT_COLOR);
			
			// F1: Show site borders ////////////////////////////////////////////////////////////////////////////
			if (MapConfig.Constants.SHOW_SITE_BORDERS)
				this.RenderBuffer(_config.BorderLineVertices, PrimitiveType.LineList, _config.BorderLineCount);

			// F12: Show Help Text //////////////////////////////////////////////////////////////////////////////
			if (MapConfig.Constants.SHOW_HELP)
				this.DrawHelpText();

			_config.SpriteBatch.End();
			base.Draw(gameTime);
			_config.DrawTimeTicks = Utilities.StopTimer("Draw");
		}

		private void RenderBuffer(VertexPositionColor[] vertices, PrimitiveType primitiveType, int primitiveCount)
		{
			using (var buffer = new VertexBuffer(_config.GfxDev, typeof(VertexPositionColor), vertices.Length, BufferUsage.None))
			{
				buffer.SetData(vertices);
				_config.GfxDev.SetVertexBuffer(null);
				_config.GfxDev.SetVertexBuffer(buffer);

				using (var sbe = new StandardBasicEffect(_config.GfxDev))
				{
					sbe.CurrentTechnique.Passes[0].Apply();
					_config.GfxDev.DrawPrimitives(primitiveType, 0, primitiveCount);
				}
			}
		}
		private void DrawRogue()
		{
			if (_config.Rogue.Site != null)
				_config.SpriteBatch.Draw(_config.RogueSprite, _config.Rogue.Location, MapConfig.Constants.FILL_SITES ? _config.Rogue.Site.GetXnaColor() : Color.White);
			else
				_config.SpriteBatch.Draw(_config.RogueSprite, _config.Rogue.Location, Color.White);
		}
		private void DrawAgents()
		{
			foreach(var agent in _config.Agents)
			{
				if(agent.Site != null)
					_config.SpriteBatch.Draw(_config.AgentSprite, agent.Location, MapConfig.Constants.FILL_SITES ? agent.Site.GetXnaColor() : Color.White);
				else
					_config.SpriteBatch.Draw(_config.AgentSprite, agent.Location, Color.White);
			}
		}
		private void DrawHelpText()
		{
			_config.SpriteBatch.DrawString(
				spriteFont : _config.FontSegoe12Regular,
				text       : _config.HelpText,
				position   : new Vector2(10, 10),
				color      : MapConfig.Constants.HELP_TEXT_COLOR
			);

			_config.SpriteBatch.DrawString(
				spriteFont : _config.FontSegoe12Regular,
				text       : _config.HelpText2,
				position   : new Vector2(420, 10),
				color      : MapConfig.Constants.HELP_TEXT_COLOR
			);

			_config.SpriteBatch.DrawString(
				spriteFont : _config.FontSegoe10Bold,
				text       : _config.TimingHeadingText,
                position   : new Vector2(10, _config.PlotBounds.Height - 300),
				color      : MapConfig.Constants.HELP_TEXT_COLOR
			);

			const float TICKS_PER_MS = TimeSpan.TicksPerMillisecond;
			var filledText = MapConfig.Constants.FILL_SITES ? "Filled" : "Outlined";
			var timingText =
				$"{MapConfig.Constants.SITE_COUNT : 0000} ({filledText}){Environment.NewLine}" +
                $"{_config.Fps.CurrentFramesPerSecond : 0000.0000}{Environment.NewLine}";

			if(Utilities.TraceLevel >= VoronoiTraceLevel.DetailedTiming)
				timingText +=
					$"{Environment.NewLine}" +

					$"{_config.InitTimeTicks / TICKS_PER_MS			  : 0000.0000}{Environment.NewLine}" +
					$"{_config.NoiseTimeTicks / TICKS_PER_MS		  : 0000.0000}{Environment.NewLine}" +
					$"{_config.LoadContentTimeTicks / TICKS_PER_MS	  : 0000.0000}{Environment.NewLine}" +
					$"{_config.UpdateTimeTicks / TICKS_PER_MS		  : 0000.0000}{Environment.NewLine}" +
					$"{_config.KeyboardUpdateTimeTicks / TICKS_PER_MS : 0000.0000}{Environment.NewLine}" +
					$"{_config.MouseUpdateTimeTicks / TICKS_PER_MS	  : 0000.0000}{Environment.NewLine}" +

					$"{Environment.NewLine}" +

					$"{_config.DrawTimeTicks / TICKS_PER_MS : 0000.0000}{Environment.NewLine}";

			_config.SpriteBatch.DrawString(
				spriteFont : _config.FontSegoe10Bold,
				text	   : timingText,
				position   : new Vector2(170, _config.PlotBounds.Height - 300),
				color	   : MapConfig.Constants.HELP_TEXT_COLOR
			);

			if (MapConfig.Constants.SHOW_SITE_BORDERS)
				_config.SpriteBatch.DrawString(
					spriteFont : _config.FontSegoe10Bold,
					text       : "Borders",
					position   : new Vector2(10, _config.PlotBounds.Height - 20),
					color      : MapConfig.Constants.HELP_TEXT_COLOR
				);

			if (MapConfig.Constants.SHOW_SITE_CENTERPOINTS)
				_config.SpriteBatch.DrawString(
					spriteFont : _config.FontSegoe10Bold,
					text       : "Centerpoints",
					position   : new Vector2(80, _config.PlotBounds.Height - 20),
					color      : MapConfig.Constants.HELP_TEXT_COLOR
				);

			if (MapConfig.Constants.SHOW_DELAUNAY)
				_config.SpriteBatch.DrawString(
					spriteFont : _config.FontSegoe10Bold,
					text       : "Delaunay",
					position   : new Vector2(190, _config.PlotBounds.Height - 20),
					color      : MapConfig.Constants.HELP_TEXT_COLOR
				);
		}
		private void UpdateMouseInput()
		{
			Utilities.Debug("vMapMain.UpdateMouseInput() called.");
			Utilities.StartTimer("UpdateMouseInput");

			_config.PreviousMouseState   = _config.CurrentMouseState;
			_config.CurrentMouseState    = Mouse.GetState();
			_config.MouseMoved           = (_config.CurrentMouseState != _config.PreviousMouseState);
			_config.CurrentKeyboardState = Keyboard.GetState();

			if ((_config.Map == null) || !_config.MouseMoved)
				return;

			var leftButton  = _config.CurrentMouseState.LeftButton == ButtonState.Pressed;
			var rightButton = _config.CurrentMouseState.RightButton == ButtonState.Pressed;
			var controlKey  = (_config.CurrentKeyboardState.IsKeyDown(Keys.LeftControl) || _config.CurrentKeyboardState.IsKeyDown(Keys.RightControl));
			var shiftKey    = (_config.CurrentKeyboardState.IsKeyDown(Keys.LeftShift) || _config.CurrentKeyboardState.IsKeyDown(Keys.RightShift));

			// get the site under the mouse pointer
			_config.MouseHoverSite = _config.Map.GetSite(new System.Drawing.PointF(_config.CurrentMouseState.X, _config.CurrentMouseState.Y));

			if (_config.MouseHoverSite == null)
				return;

			// update the 'Hover' state (remove this state from the previous site
			// and add it to the current site - if they are different)
			if ((_config.Map.CurrLocation == null) || !_config.Map.CurrLocation.Equals(_config.MouseHoverSite))
			{
				_config.Map.CurrLocation?.Data.RemoveState(SiteState.Hover);
				_config.MouseHoverSite.Data.AddState(SiteState.Hover);
				_config.Map.CurrLocation = _config.MouseHoverSite;
			}

			if (leftButton && controlKey)
				_config.MouseHandler.HandleMouse_ControlLeftButtonClick();
			else if (rightButton && controlKey)
				_config.MouseHandler.HandleMouse_ControlRightButtonClick();
			else if (leftButton && shiftKey)
				_config.MouseHandler.HandleMouse_ShiftLeftButtonClick();
			else if (rightButton && shiftKey)
				_config.MouseHandler.HandleMouse_ShiftRightButtonClick();
			else if (leftButton)
				_config.MouseHandler.HandleMouse_LeftButtonClick();
			else if (rightButton)
				_config.MouseHandler.HandleMouse_RightButtonClick();
			else if (controlKey)
				_config.MouseHandler.HandleMouse_ControlHover();
			else if (shiftKey)
				_config.MouseHandler.HandleMouse_ShiftHover();
			else
				_config.MouseHandler.HandleMouse_Hover();

			_config.MouseUpdateTimeTicks = Utilities.StopTimer("UpdateMouseInput");
		}
		private void UpdateKeyboardInput()
		{
			Utilities.Debug("vMapMain.UpdateKeyboardInput() called.");
			_config.KeyHandler.HandleKeys();
		}
		private void CreateKeyHandlers()
		{
			_config.KeyHandler.KeyHandlers =
				new Dictionary<Keys[], KeyHandlerDelegate>
				{
				    { new Keys[1] { Keys.Escape }			     , Exit },
				    { new Keys[1] { Keys.F1 }				     , ToggleBorders },
				    { new Keys[1] { Keys.F2 }				     , ToggleSiteCenterpoints },
				    { new Keys[1] { Keys.F3 }				     , ToggleDelaunayLines },
				    { new Keys[1] { Keys.F4 }				     , ClearAndRegenerateNoise },
				    { new Keys[1] { Keys.F5 }				     , RelaxVoronoiCells },
				    { new Keys[1] { Keys.F6 }				     , StartAStarSearch },
				    { new Keys[1] { Keys.F7 }				     , ClearSearch },
				    { new Keys[1] { Keys.F8 }				     , ClearMap },
				    { new Keys[1] { Keys.F9 }				     , DecreaseSites },
				    { new Keys[1] { Keys.F10 }				     , IncreaseSites },
				    { new Keys[1] { Keys.F11 }				     , ToggleOutlineFill },
				    { new Keys[1] { Keys.F12 }				     , ToggleHelp },
				    { new Keys[2] { Keys.LeftControl, Keys.F1 }  , ToggleAgents },
					{ new Keys[2] { Keys.RightControl, Keys.F1 } , ToggleAgents },
				};
		}
		private void ClearAndRegenerateNoise()
		{
			this.ClearMap();
			this.CreateNoiseMap();
		}
		private void CreateNoiseMap()
		{
			Utilities.StartTimer("NoiseMap");
			_config.Map.SetNoiseMap(
				NoiseGenerator.GenerateNoise(
					noiseType   : NoiseType.Billow,
					width       : _config.PlotBounds.Width,
					height      : _config.PlotBounds.Height,
					frequency   : MapConfig.Constants.NOISE_FREQUENCY,
					quality     : MapConfig.Constants.NOISE_QUALITY,
					seed        : Utilities.GetRandomInt(),
					octaves     : MapConfig.Constants.NOISE_OCTAVES,
					lacunarity  : MapConfig.Constants.NOISE_LACUNARITY,
					persistence : MapConfig.Constants.NOISE_PERSISTENCE,
					scale       : MapConfig.Constants.NOISE_SCALE
				),
				scale: MapConfig.Constants.NOISE_SCALE
			);
			_config.NoiseTimeTicks = Utilities.StopTimer("NoiseMap");
		}
		private void RelaxVoronoiCells()
		{
			_config.Map.Relax(1);
			this.CreateVertexArrays();
		}
		private void StartAStarSearch()
		{
			if((_config.Map.SearchStart == null) || (_config.Map.SearchGoal == null))
				return;

			var search = new AStarSearch(_config.Map.MapGraph, _config.Map.SearchStart, _config.Map.SearchGoal);
			search.SearchComplete += OnSearchComplete;
			var thread = new Thread(search.Search);
			thread.Start();
		}
		private void ClearSearch()
		{
			if (_config.Map == null)
				return;

			// remove A* search start/goal from the map
			if (_config.Map.SearchStart != null)
			{
				_config.Map.SearchStart.RemoveState(SiteState.AStarSearchStart);
				_config.Map.SearchStart = null;
			}

			if (_config.Map.SearchGoal != null)
			{
				_config.Map.SearchGoal.RemoveState(SiteState.AStarSearchGoal);
				_config.Map.SearchGoal = null;
			}

			foreach (var s in _config.Map.Sites.Values.Where(s => s.Data.IsSiteState(SiteState.Frontier) || s.Data.IsSiteState(SiteState.Visited) || s.Data.IsSiteState(SiteState.Path)))
			{
				s.Data.RemoveState(SiteState.Frontier);
				s.Data.RemoveState(SiteState.Visited);
				s.Data.RemoveState(SiteState.Path);
			}
		}
		private void ClearMap()
		{
			if (_config.Map == null)
				return;

			// remove current site
			if (_config.Map.CurrLocation?.Data != null)
			{
				_config.Map.CurrLocation.Data.RemoveState(SiteState.Hover);
				_config.Map.CurrLocation = null;
			}

			// remove walls
			_config.Map.MapGraph?.PrimaryWalls.Clear();
			foreach (var s in _config.Map.Sites.Values)
			{
				s.Data.RemoveState(SiteState.Impassable);
				s.Data.RemoveState(SiteState.Wall);
			}
			this.ClearSearch();
		}
		private void DecreaseSites()
		{
			MapConfig.Constants.SITE_COUNT -= 1000;
			this.Initialize();
		}
		private void IncreaseSites()
		{
			MapConfig.Constants.SITE_COUNT += 1000;
			this.Initialize();
		}
		private void StartAgentSearch(Site start, Site goal)
		{
			if ((start == null) || (goal == null))
				return;

			var search = new AStarSearch(_config.Map.MapGraph, start, goal);
			search.SearchComplete += OnSearchComplete;
			var thread = new Thread(search.Search);
			thread.Start();
		}
		private void CreateMap()
		{
			_config.Map = new Map(MapConfig.Constants.SITE_COUNT, _config.PlotBounds);
			_config.Map.Relax(MapConfig.Constants.INIT_RELAX_ITERATIONS);
			_config.Map.SiteUpdated += this.OnSiteUpdated;
		}
		private void CreateVertexArrays()
		{
			this.CreateSiteTriangleVertices();
			this.CreateDelaunayLineVertices();
			this.CreateSiteBorderVertices();
		}
		private void CreateSiteTriangleVertices()
		{
			var siteIndex = 0;
			var siteVertexIndex = 0;

			_config.SiteTriangleCount = _config.Map.Sites.Values.Sum(s => s.Data.RegionPoints.Length);
			_config.SiteTriangleVertices = new VertexPositionColor[_config.SiteTriangleCount * 3];
			
			foreach(var site in _config.Map.Sites.Values.Select(s => s.Data))
			{
				site.SiteIndex = siteIndex;
				site.VertexBufferIndex = siteVertexIndex;
				var center = site.Point;
				var color = site.GetXnaColor();
				var vertices = site.RegionPoints;
				var vIndex = 0;

				// store vertices for the site area triangles
				foreach (var v in vertices)
				{
					_config.SiteTriangleVertices[siteVertexIndex] = new VertexPositionColor(new Vector3(center.X, center.Y, 0), color);
					_config.SiteTriangleVertices[siteVertexIndex + 1] = new VertexPositionColor(new Vector3(vertices[vIndex].X, vertices[vIndex].Y, 0), color);
					_config.SiteTriangleVertices[siteVertexIndex + 2] = (vIndex + 1 < vertices.Length)
						? new VertexPositionColor(new Vector3(vertices[vIndex + 1].X, vertices[vIndex + 1].Y, 0), color)
						: new VertexPositionColor(new Vector3(vertices[0].X, vertices[0].Y, 0), color);

					siteVertexIndex += 3;
					vIndex++;
				}
				siteIndex++;
			}
		}
		private void CreateDelaunayLineVertices()
		{
			var delaunayLineIndex = 0;

			_config.DelaunayLineCount = _config.Map.Sites.Values.Sum(s => s.Edges.Count);
			_config.DelaunayLineVertices = new VertexPositionColor[_config.DelaunayLineCount * 2];
			
			foreach (var s in _config.Map.Sites.Values)
			{
				// store vertices for the Delaunay line segments
				foreach (var edge in s.Edges)
				{
					var start = edge.Value.Node1.Data.Point;
					var end = edge.Value.Node2.Data.Point;

					_config.DelaunayLineVertices[delaunayLineIndex] = new VertexPositionColor(new Vector3(start.X, start.Y, 0), MapConfig.Constants.DELAUNAY_COLOR);
					_config.DelaunayLineVertices[delaunayLineIndex + 1] = new VertexPositionColor(new Vector3(end.X, end.Y, 0), MapConfig.Constants.DELAUNAY_COLOR);
					delaunayLineIndex += 2;
				}
			}
		}
		private void CreateSiteBorderVertices()
		{
			var borderLineIndex = 0;

			_config.BorderLineCount = _config.Map.Sites.Values.Sum(s => s.Data.RegionPoints.Length);
			_config.BorderLineVertices = new VertexPositionColor[_config.BorderLineCount  * 2];

			foreach (var site in _config.Map.Sites.Values.Select(s => s.Data))
			{
				// store vertices for the site border line segments
				var last = PointF.Empty;
				foreach (var rp in site.RegionPoints)
				{
					if (last != PointF.Empty)
					{
						_config.BorderLineVertices[borderLineIndex] = new VertexPositionColor(new Vector3(last.X, last.Y, 0), MapConfig.Constants.BORDER_COLOR);
						_config.BorderLineVertices[borderLineIndex + 1] = new VertexPositionColor(new Vector3(rp.X, rp.Y, 0), MapConfig.Constants.BORDER_COLOR);
						borderLineIndex += 2;
					}
					last = rp;
				}
			}
		}
		private void OnSiteUpdated(object sender, EventArgs e)
		{
			_config.SitesToUpdate.Enqueue((Site)sender, 0);
		}
		private void UpdateSite(Site site)
		{
			if (site == null)
				return;

			if (_config.SiteTriangleVertices == null)
				return;

			var i = site.VertexBufferIndex;
			for (var count = 0; count < site.RegionPoints.Length * 3; count++)
				_config.SiteTriangleVertices[i++].Color = site.GetXnaColor();
		}

		private static void ToggleBorders()
		{
			MapConfig.Constants.SHOW_SITE_BORDERS = !MapConfig.Constants.SHOW_SITE_BORDERS;
		}
		private static void ToggleSiteCenterpoints()
		{
			MapConfig.Constants.SHOW_SITE_CENTERPOINTS = !MapConfig.Constants.SHOW_SITE_CENTERPOINTS;
		}
		private static void ToggleDelaunayLines()
		{
			MapConfig.Constants.SHOW_DELAUNAY = !MapConfig.Constants.SHOW_DELAUNAY;
		}
		private static void ToggleOutlineFill()
		{
			MapConfig.Constants.FILL_SITES = !MapConfig.Constants.FILL_SITES;
		}
		private static void ToggleHelp()
		{
			MapConfig.Constants.SHOW_HELP = !MapConfig.Constants.SHOW_HELP;
		}
		private static void ToggleAgents()
		{
			MapConfig.Constants.SHOW_AGENTS = !MapConfig.Constants.SHOW_AGENTS;
		}
		private static void OnSearchComplete(object sender, EventArgs e)
		{
			var search = (AStarSearch)sender;

			if (!search.CameFrom.ContainsKey(search.Goal))
				return;

			var next = search.CameFrom[search.Goal];
			while ((next != null) && !Equals(next, search.Start))
			{
				next.AddState(SiteState.Path);
				next = search.CameFrom[next];
			}
		}
	}
}