using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using vMap.Voronoi;
using LibNoise;

using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;
using Color       = Microsoft.Xna.Framework.Color;
using Keys        = Microsoft.Xna.Framework.Input.Keys;
using MGE         = MonoGame.Extended;

namespace vMap.MonoGame
{
	public delegate void KeyHandler();

	public class vMapMain : Game
	{
		#region Private Members

		// configuration
		private const	 bool  FULL_SCREEN				= false;
		private const	 int   WINDOW_WIDTH				= 1024;
		private const	 int   WINDOW_HEIGHT			= 768;
		private const	 float NOISE_SCALE				= 0.5f;
		private const	 int   AGENT_COUNT				= 10;
		private const	 int   INIT_RELAX_ITERATIONS	= 0;
		private	readonly Color BG_COLOR					= Color.White;
		private readonly Color BORDER_COLOR				= Color.DarkGray;
		private readonly Color DELAUNAY_COLOR			= Color.LightGray;
		private readonly Color CENTERPOINT_COLOR		= Color.Black;
		private readonly Color HELP_TEXT_COLOR			= Color.Black;
		private			 int   _siteCount				= 5000;
		private			 bool  _showSiteBorders			= false;
		private			 bool  _showSiteCenterpoints	= false;
		private			 bool  _showDelaunay			= false;
		private			 bool  _showHelp				= true;
		private			 bool  _showAgents				= false;
		private			 bool  _fillSites				= true;

		private readonly GraphicsDeviceManager			_graphics;
		private			 MGE.FramesPerSecondCounter		_fps;
		private			 PriorityEventQueue<Site>		_sitesToUpdate;
		private			 PriorityEventQueue<Agent>		_agentsToUpdate; 
		private			 SpriteBatch					_spriteBatch;
		private			 Map							_map;
		private			 System.Drawing.Rectangle		_plotBounds;
		private			 SpriteFont						_fontSegoe8Bold;
		private			 SpriteFont						_fontSegoe10Regular;
		private			 SpriteFont						_fontSegoe10Bold;
		private			 SpriteFont						_fontSegoe12Regular;
		private			 SpriteFont						_fontSegoe12Bold;
		private			 KeyboardState					_currentKeyboardState;
		private			 KeyboardState					_previousKeyboardState;
		private			 MouseState						_currentMouseState;
		private			 MouseState						_previousMouseState;
		private			 bool							_mouseMoved;
		private			 string							_helpText;
		private			 string							_helpText2;
		private			 string							_timingHeadingText;
		private			 MGE.Camera2D					_camera;
		private			 Texture2D						_pixel;
		private			 Texture2D						_3x3Pixel;
		private			 Dictionary<Keys[], KeyHandler> _keyHandlers;
		private			 Agent[]						_agents;
		private			 Texture2D						_agentSprite;
		private			 Agent							_rogue;
		private			 Texture2D						_rogueSprite;
		private			 GraphVertex<Site, Coordinate> _mouseHoverSite;

		// vertex arrays
		private			 int							_siteTriangleCount;
		private			 VertexPositionColor[]			_siteTriangleVertices;
		private			 int							_delaunayLineCount;
		private			 VertexPositionColor[]			_delaunayLineVertices;
		private			 int							_borderLineCount;
		private			 VertexPositionColor[]			_borderLineVertices;

		// timing metrics
		private			 long							_drawTimeTicks;
		private			 long							_updateTimeTicks;
		private			 long							_loadContentTimeTicks;
		private			 long							_initTimeTicks;
		private			 long							_noiseTimeTicks;
		private			 long							_keyboardUpdateTimeTicks;
		private			 long							_mouseUpdateTimeTicks;
		#endregion
		
		public vMapMain()
		{
			Utilities.Debug("vMapMain Constructor called.");

			if (FULL_SCREEN)
				_graphics = new GraphicsDeviceManager(this)
				{
					PreferredBackBufferWidth = Screen.PrimaryScreen.Bounds.Width,
					PreferredBackBufferHeight = Screen.PrimaryScreen.Bounds.Height,
					IsFullScreen = true
				};
			else
				_graphics = new GraphicsDeviceManager(this)
				{
					PreferredBackBufferWidth = WINDOW_WIDTH,
					PreferredBackBufferHeight = WINDOW_HEIGHT
				};

			// tell XNA to call the Draw() method as fast as posible
			_graphics.SynchronizeWithVerticalRetrace = false;

			// also tell XNA to call the Update() method before Draw() every cycle
			// instead of at a regular interval
			this.IsFixedTimeStep = false;
		}

		protected override void Initialize()
		{
			Utilities.Debug("Initialize() called.");
			Utilities.StartTimer("Initialize");

			this.IsMouseVisible = true;
			Content.RootDirectory = "Content";

			_plotBounds =
				new System.Drawing.Rectangle
				(
					_graphics.GraphicsDevice.Viewport.TitleSafeArea.X,
					_graphics.GraphicsDevice.Viewport.TitleSafeArea.Y,
					_graphics.GraphicsDevice.Viewport.TitleSafeArea.Width,
					_graphics.GraphicsDevice.Viewport.TitleSafeArea.Height
				);

			// create an FPS counter
			_fps = new MGE.FramesPerSecondCounter();

			// create a new priority event queue to handle site (cell) updates to the UI
			_sitesToUpdate = new PriorityEventQueue<Site>();

			// create a new priority event queue to handle agent updates to the UI
			_agentsToUpdate = new PriorityEventQueue<Agent>();

			// Setup key handlers ///////////////////////////////////////////////////////////////////////////////
			this.CreateKeyHandlers();

			// Create a new map /////////////////////////////////////////////////////////////////////////////////
			this.CreateMap();

			// Create a noise map to auto-determine elevations //////////////////////////////////////////////////
			this.CreateNoiseMap();

			// Create Site triangle, site border and Delaunay line vertex arrays for the entire map /////////////
			this.CreateVertexArrays();

			// Create a 2D camera to enable manipulation of the view ////////////////////////////////////////////
			_camera = new MGE.Camera2D(base.GraphicsDevice);

			_pixel = new Texture2D(_graphics.GraphicsDevice, 1, 1);
			_pixel.SetData<Color>(new Color[1] { Color.White });

			_3x3Pixel = new Texture2D(_graphics.GraphicsDevice, 3, 3);
			_3x3Pixel.SetData<Color>(new Color[1] { Color.White });

			// create agents, ensuring that each new agent is inside
			// a site and not on the border of two sites
			_agents = new Agent[AGENT_COUNT];
			var count = 0;
			while(count < AGENT_COUNT)
			{
				var rndLoc = Utilities.GetRandomLocation(_plotBounds);
				var agentSite = _map.GetSite(rndLoc)?.Data;

				if((agentSite != null) && !agentSite.IsSiteState(SiteState.Impassable) && !agentSite.IsSiteState(SiteState.Wall))
				{
					_agents[count++] = new Agent { Location = rndLoc, Site = agentSite };
				}
			}

			// call base.Initialize() last
			base.Initialize();

			_initTimeTicks = Utilities.StopTimer("Initialize");
		}
		protected override void LoadContent()
		{
			Utilities.Debug("LoadContent() called.");
			Utilities.StartTimer("LoadContent");

			// Create a new SpriteBatch, which can be used to draw textures.
			_spriteBatch = new SpriteBatch(base.GraphicsDevice);

			// fonts
			_fontSegoe8Bold     = Content.Load<SpriteFont>("Segoe_8_Bold");
			_fontSegoe10Regular = Content.Load<SpriteFont>("Segoe_10_Regular");
			_fontSegoe10Bold    = Content.Load<SpriteFont>("Segoe_10_Bold");
			_fontSegoe12Regular = Content.Load<SpriteFont>("Segoe_12_Regular");
			_fontSegoe12Bold    = Content.Load<SpriteFont>("Segoe_12_Bold");

			// help text
			_helpText =
				$"Left Mouse:  Set Search Start{Environment.NewLine}" +
				$"Right Mouse: Set Search Goal{Environment.NewLine}" +
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

			_helpText2 =
				$"CTRL-Left Mouse: Create Wall{Environment.NewLine}" +
				$"{Environment.NewLine}" +
				$"CTRL-F1:  Toggle Agents{Environment.NewLine}" +
				$"CTRL-F2:  Set the Rogue Agent{Environment.NewLine}" +
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

			_timingHeadingText =
				$"Site count:{Environment.NewLine}" +
				$"FPS:{Environment.NewLine}";

			if (Utilities.TraceLevel >= VoronoiTraceLevel.DetailedTiming)
				_timingHeadingText +=
					$"{Environment.NewLine}" +

					$"Init:{Environment.NewLine}" +
					$" + Noise:{Environment.NewLine}" +
					$"Load content:{Environment.NewLine}" +
					$"Update:{Environment.NewLine}" +
					$" + Keyboard:{Environment.NewLine}" +
					$" + Mouse:{Environment.NewLine}" +

					$"{Environment.NewLine}" +

					$"Draw:{Environment.NewLine}";

			_agentSprite = Content.Load<Texture2D>("agent");
			_rogueSprite = Content.Load<Texture2D>("rogue_agent");

			SpriteBatchExtensions.LoadContent(_graphics.GraphicsDevice);
			_loadContentTimeTicks = Utilities.StopTimer("LoadContent");
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
			
			this.UpdateKeyboardInput();
			this.UpdateMouseInput();
			base.Update(gameTime);

			_updateTimeTicks = Utilities.StopTimer("Update");
		}
		protected override void Draw(GameTime gameTime)
		{
			Utilities.StartTimer("Draw");

			_fps.Update(gameTime);
			base.GraphicsDevice.Clear(BG_COLOR);
			_spriteBatch.Begin(transformMatrix: _camera.GetViewMatrix());

			// handle all the Sites that have been altered and pushed onto the event queue for updating
			while (_sitesToUpdate.Count > 0)
				this.UpdateSite(_sitesToUpdate.Dequeue());

			if(_showAgents)
			{
				if(_rogue != null)
					this.DrawRogue();
				this.DrawAgents();
			}

			// F11: Fill sites //////////////////////////////////////////////////////////////////////////////////
			if (_fillSites)
				this.RenderBuffer(_siteTriangleVertices, PrimitiveType.TriangleList, _siteTriangleCount);

			// F4: Show Delaunay triangulation lines ////////////////////////////////////////////////////////////
			if(_showDelaunay)
				this.RenderBuffer(_delaunayLineVertices, PrimitiveType.LineList, _delaunayLineCount);

			// F2: Show site centerpoints ///////////////////////////////////////////////////////////////////////
			if(_showSiteCenterpoints)
				foreach(var site in _map.Sites.Values.Select(s => s.Data))
					_spriteBatch.Draw(_pixel, new Vector2(site.Point.X, site.Point.Y), CENTERPOINT_COLOR);
			
			// F1: Show site borders ////////////////////////////////////////////////////////////////////////////
			if (_showSiteBorders)
				this.RenderBuffer(_borderLineVertices, PrimitiveType.LineList, _borderLineCount);

			// F12: Show Help Text ////////////////////////////////////////////////////////////////////////////////
			if (_showHelp)
				this.DrawHelpText();

			_spriteBatch.End();
			base.Draw(gameTime);
			_drawTimeTicks = Utilities.StopTimer("Draw");
		}

		private void DrawRogue()
		{
			if (_rogue.Site != null)
				_spriteBatch.Draw(_rogueSprite, _rogue.Location, _fillSites ? _rogue.Site.GetXnaColor() : Color.White);
			else
				_spriteBatch.Draw(_rogueSprite, _rogue.Location, Color.White);
		}
		private void DrawAgents()
		{
			foreach(var agent in _agents)
			{
				if(agent.Site != null)
					_spriteBatch.Draw(_agentSprite, agent.Location, _fillSites ? agent.Site.GetXnaColor() : Color.White);
				else
					_spriteBatch.Draw(_agentSprite, agent.Location, Color.White);
			}
		}
		private void DrawHelpText()
		{
			_spriteBatch.DrawString(
				spriteFont : _fontSegoe12Regular,
				text       : _helpText,
				position   : new Vector2(10, 10),
				color      : HELP_TEXT_COLOR
			);

			_spriteBatch.DrawString(
				spriteFont : _fontSegoe12Regular,
				text       : _helpText2,
				position   : new Vector2(420, 10),
				color      : HELP_TEXT_COLOR
			);

			_spriteBatch.DrawString(
				spriteFont : _fontSegoe10Bold,
				text       : _timingHeadingText,
                position   : new Vector2(10, _plotBounds.Height - 300),
				color      : HELP_TEXT_COLOR
			);

			const float TICKS_PER_MS = TimeSpan.TicksPerMillisecond;
			var filledText = _fillSites ? "Filled" : "Outlined";
			var timingText =
				$"{_siteCount: 0000} ({filledText}){Environment.NewLine}" +
                $"{_fps.CurrentFramesPerSecond : 0000.0000}{Environment.NewLine}";

			if(Utilities.TraceLevel >= VoronoiTraceLevel.DetailedTiming)
				timingText +=
					$"{Environment.NewLine}" +

					$"{_initTimeTicks / TICKS_PER_MS: 0000.0000}{Environment.NewLine}" +
					$"{_noiseTimeTicks / TICKS_PER_MS: 0000.0000}{Environment.NewLine}" +
					$"{_loadContentTimeTicks / TICKS_PER_MS: 0000.0000}{Environment.NewLine}" +
					$"{_updateTimeTicks / TICKS_PER_MS: 0000.0000}{Environment.NewLine}" +
					$"{_keyboardUpdateTimeTicks / TICKS_PER_MS: 0000.0000}{Environment.NewLine}" +
					$"{_mouseUpdateTimeTicks / TICKS_PER_MS: 0000.0000}{Environment.NewLine}" +

					$"{Environment.NewLine}" +

					$"{_drawTimeTicks / TICKS_PER_MS: 0000.0000}{Environment.NewLine}";

			_spriteBatch.DrawString(
				spriteFont : _fontSegoe10Bold,
				text	   : timingText,
				position   : new Vector2(170, _plotBounds.Height - 300),
				color	   : HELP_TEXT_COLOR
			);

			if (_showSiteBorders)
				_spriteBatch.DrawString(
					spriteFont : _fontSegoe10Bold,
					text       : "Borders",
					position   : new Vector2(10, _plotBounds.Height - 20),
					color      : HELP_TEXT_COLOR
				);

			if (_showSiteCenterpoints)
				_spriteBatch.DrawString(
					spriteFont : _fontSegoe10Bold,
					text       : "Centerpoints",
					position   : new Vector2(80, _plotBounds.Height - 20),
					color      : HELP_TEXT_COLOR
				);

			if (_showDelaunay)
				_spriteBatch.DrawString(
					spriteFont : _fontSegoe10Bold,
					text       : "Delaunay",
					position   : new Vector2(190, _plotBounds.Height - 20),
					color      : HELP_TEXT_COLOR
				);
		}
		private void UpdateKeyboardInput()
		{
			Utilities.Debug("vMapMain.UpdateKeyboardInput() called.");
			Utilities.StartTimer("UpdateKeyboardInput");

			_previousKeyboardState = _currentKeyboardState;
			_currentKeyboardState = Keyboard.GetState();

			if (_currentKeyboardState == _previousKeyboardState)
			{
				_keyboardUpdateTimeTicks = Utilities.StopTimer("UpdateKeyboardInput");
				return;
			}

			var keys = _currentKeyboardState.GetPressedKeys();
			KeyHandler match = null;
			foreach (var kh in _keyHandlers)
			{
				if (keys.OrderBy(a => a).SequenceEqual(kh.Key.OrderBy(a => a)))
				{
					match = kh.Value;
					break;
				}
			}
			match?.Invoke();

			_keyboardUpdateTimeTicks = Utilities.StopTimer("UpdateKeyboardInput");
		}
		private void CreateKeyHandlers()
		{
			_keyHandlers =
				new Dictionary<Keys[], KeyHandler>
				{
				    { new Keys[1] { Keys.Escape }			     , Exit },
				    { new Keys[1] { Keys.F1 }				     , ToggleBorders },
				    { new Keys[1] { Keys.F2 }				     , ToggleSiteCenterpoints },
				    { new Keys[1] { Keys.F3 }				     , ToggleDelaunayLines },
				    { new Keys[1] { Keys.F4 }				     , CreateNoiseMap },
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
		private void UpdateMouseInput()
		{
			Utilities.Debug("vMapMain.UpdateMouseInput() called.");
			Utilities.StartTimer("UpdateMouseInput");

			_previousMouseState = _currentMouseState;
			_currentMouseState  = Mouse.GetState();
			_mouseMoved         = (_currentMouseState != _previousMouseState);

			if ((_map == null) || !_mouseMoved)
				return;
			
			var leftButton  = _currentMouseState.LeftButton == ButtonState.Pressed;
			var rightButton = _currentMouseState.RightButton == ButtonState.Pressed;
			var controlKey  = (_currentKeyboardState.IsKeyDown(Keys.LeftControl) || _currentKeyboardState.IsKeyDown(Keys.RightControl));
			var shiftKey    = (_currentKeyboardState.IsKeyDown(Keys.LeftShift) || _currentKeyboardState.IsKeyDown(Keys.RightShift));

			// get the site under the mouse pointer
			_mouseHoverSite = _map.GetSite(new System.Drawing.PointF(_currentMouseState.X, _currentMouseState.Y));

			if (_mouseHoverSite == null)
				return;

			// update the 'Hover' state (remove this state from the previous site
			// and add it to the current site - if they are different)
			if((_map.CurrLocation == null) || !_map.CurrLocation.Equals(_mouseHoverSite))
			{
				_map.CurrLocation?.Data.RemoveState(SiteState.Hover);
				_mouseHoverSite.Data.AddState(SiteState.Hover);
				_map.CurrLocation = _mouseHoverSite;
			}

			if(leftButton && controlKey)
				this.HandleMouse_ControlLeftButtonClick();
			else if(rightButton && controlKey)
				this.HandleMouse_ControlRightButtonClick();
			else if(leftButton && shiftKey)
				this.HandleMouse_ShiftLeftButtonClick();
			else if(rightButton && shiftKey)
				this.HandleMouse_ShiftRightButtonClick();
			else if(leftButton)
				this.HandleMouse_LeftButtonClick();
			else if(rightButton)
				this.HandleMouse_RightButtonClick();
			else if(controlKey)
				this.HandleMouse_ControlHover();
			else if(shiftKey)
				this.HandleMouse_ShiftHover();
			else
				this.HandleMouse_Hover();

			_mouseUpdateTimeTicks = Utilities.StopTimer("UpdateMouseInput");
		}

		private void HandleMouse_ControlLeftButtonClick()
		{
			// if the current site isn't already 'Impassable', make it so
			if(_mouseHoverSite.Data.IsSiteState(SiteState.Wall))
				return;

			_mouseHoverSite.Data.AddState(SiteState.Impassable);
			_mouseHoverSite.Data.AddState(SiteState.Wall);
			_map.MapGraph.PrimaryWalls.Add(_mouseHoverSite.Data);
		}
		private void HandleMouse_ControlRightButtonClick()
		{
			_rogue = new Agent { Location = new Vector2(_mouseHoverSite.Data.Point.X, _mouseHoverSite.Data.Point.Y), Site = _mouseHoverSite.Data };
			foreach (var agent in _agents)
				this.StartAgentSearch(agent.Site, _rogue.Site);
		}
		private void HandleMouse_ShiftLeftButtonClick()
		{}
		private void HandleMouse_ShiftRightButtonClick()
		{}
		private void HandleMouse_LeftButtonClick()
		{
			// left-button click = set the current Site as the A* search start
			if((_map.SearchStart != null) && _map.SearchStart.Equals(_mouseHoverSite.Data))
				return;

			_map.SearchStart?.RemoveState(SiteState.AStarSearchStart);
			_mouseHoverSite.Data.AddState(SiteState.AStarSearchStart);
			_map.SearchStart = _mouseHoverSite.Data;
		}
		private void HandleMouse_RightButtonClick()
		{
			// right-button click = set the current Site as the A* search goal
			if((_map.SearchGoal != null) && _map.SearchGoal.Equals(_mouseHoverSite.Data))
				return;

			_map.SearchGoal?.RemoveState(SiteState.AStarSearchGoal);
			_mouseHoverSite.Data.AddState(SiteState.AStarSearchGoal);
			_map.SearchGoal = _mouseHoverSite.Data;
		}
		private void HandleMouse_ControlHover()
		{
			if(Form.ActiveForm != null)
				Form.ActiveForm.Text = _mouseHoverSite.Data.ToString();
		}
		private void HandleMouse_ShiftHover()
		{}
		private void HandleMouse_Hover()
		{
			if (Form.ActiveForm != null)
				Form.ActiveForm.Text = "vMap.MonoGame";
		}
		
		private void RenderBuffer(VertexPositionColor[] vertices, PrimitiveType primitiveType, int primitiveCount)
		{
			using(var buffer = new VertexBuffer(_graphics.GraphicsDevice, typeof(VertexPositionColor), vertices.Length, BufferUsage.None))
			{
				buffer.SetData(vertices);
				_graphics.GraphicsDevice.SetVertexBuffer(null);
				_graphics.GraphicsDevice.SetVertexBuffer(buffer);

				using (var sbe = new StandardBasicEffect(_graphics.GraphicsDevice))
				{
					sbe.CurrentTechnique.Passes[0].Apply();
					_graphics.GraphicsDevice.DrawPrimitives(primitiveType, 0, primitiveCount);
				}
			}
		}
		private void IncreaseSites()
		{
			_siteCount += 1000;
			this.Initialize();
		}
		private void DecreaseSites()
		{
			_siteCount -= 1000;
			this.Initialize();
		}
		private void ToggleOutlineFill()
		{
			_fillSites = !_fillSites;
		}
		private void ToggleBorders()
		{
			_showSiteBorders = !_showSiteBorders;
		}
		private void ToggleSiteCenterpoints()
		{
			_showSiteCenterpoints = !_showSiteCenterpoints;
		}
		private void ToggleDelaunayLines()
		{
			_showDelaunay = !_showDelaunay;
		}
		private void ToggleHelp()
		{
			_showHelp = !_showHelp;
		}
		private void ToggleAgents()
		{
			_showAgents = !_showAgents;
		}
		private void RelaxVoronoiCells()
		{
			_map.Relax(1);
			this.CreateVertexArrays();
		}
		private void StartAStarSearch()
		{
			if((_map.SearchStart == null) || (_map.SearchGoal == null))
				return;

			var search = new AStarSearch(_map.MapGraph, _map.SearchStart, _map.SearchGoal);
			search.SearchComplete += OnSearchComplete;
			var thread = new Thread(search.Search);
			thread.Start();
		}
		private void StartAgentSearch(Site start, Site goal)
		{
			if ((start == null) || (goal == null))
				return;

			var search = new AStarSearch(_map.MapGraph, start, goal);
			search.SearchComplete += OnSearchComplete;
			var thread = new Thread(search.Search);
			thread.Start();
		}
		private void ClearMap()
		{
			if (_map == null)
				return;

			// remove current site
			if (_map.CurrLocation?.Data != null)
			{
				_map.CurrLocation.Data.RemoveState(SiteState.Hover);
				_map.CurrLocation = null;
			}

			// remove walls
			_map.MapGraph?.PrimaryWalls.Clear();
			foreach(var s in _map.Sites.Values)
			{
				s.Data.RemoveState(SiteState.Impassable);
				s.Data.RemoveState(SiteState.Wall);
			}
			this.ClearSearch();
		}
		private void ClearSearch()
		{
			if (_map == null)
				return;

			// remove A* search start/goal from the map
			if (_map.SearchStart != null)
			{
				_map.SearchStart.RemoveState(SiteState.AStarSearchStart);
				_map.SearchStart = null;
			}

			if (_map.SearchGoal != null)
			{
				_map.SearchGoal.RemoveState(SiteState.AStarSearchGoal);
				_map.SearchGoal = null;
			}

			foreach (var s in _map.Sites.Values.Where(s => s.Data.IsSiteState(SiteState.Frontier) || s.Data.IsSiteState(SiteState.Visited) || s.Data.IsSiteState(SiteState.Path)))
			{
				s.Data.RemoveState(SiteState.Frontier);
				s.Data.RemoveState(SiteState.Visited);
				s.Data.RemoveState(SiteState.Path);
			}
		}
		private void CreateMap()
		{
			_map = new Map(_siteCount, _plotBounds);
			_map.Relax(INIT_RELAX_ITERATIONS);
			_map.SiteUpdated += this.OnSiteUpdated;
		}
		private void CreateNoiseMap()
		{
			Utilities.StartTimer("NoiseMap");
			_map.SetNoiseMap(
				NoiseGenerator.GenerateNoise(
					noiseType   : NoiseType.Billow,
					width       : _plotBounds.Width,
					height      : _plotBounds.Height,
					frequency   : .003,
					quality     : NoiseQuality.High,
					seed        : Utilities.GetRandomInt(),
					octaves     : 6,
					lacunarity  : 1.5,
					persistence : 0.75,
					scale       : NOISE_SCALE
				),
				scale: NOISE_SCALE
			);
			_noiseTimeTicks = Utilities.StopTimer("NoiseMap");
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

			_siteTriangleCount = _map.Sites.Values.Sum(s => s.Data.RegionPoints.Length);
			_siteTriangleVertices = new VertexPositionColor[_siteTriangleCount * 3];
			
			foreach(var site in _map.Sites.Values.Select(s => s.Data))
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
					_siteTriangleVertices[siteVertexIndex] = new VertexPositionColor(new Vector3(center.X, center.Y, 0), color);
					_siteTriangleVertices[siteVertexIndex + 1] = new VertexPositionColor(new Vector3(vertices[vIndex].X, vertices[vIndex].Y, 0), color);
					_siteTriangleVertices[siteVertexIndex + 2] = (vIndex + 1 < vertices.Length)
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

			_delaunayLineCount = _map.Sites.Values.Sum(s => s.Edges.Count);
			_delaunayLineVertices = new VertexPositionColor[_delaunayLineCount * 2];
			
			foreach (var s in _map.Sites.Values)
			{
				// store vertices for the Delaunay line segments
				foreach (var edge in s.Edges)
				{
					var start = edge.Value.Node1.Data.Point;
					var end = edge.Value.Node2.Data.Point;

					_delaunayLineVertices[delaunayLineIndex] = new VertexPositionColor(new Vector3(start.X, start.Y, 0), DELAUNAY_COLOR);
					_delaunayLineVertices[delaunayLineIndex + 1] = new VertexPositionColor(new Vector3(end.X, end.Y, 0), DELAUNAY_COLOR);
					delaunayLineIndex += 2;
				}
			}
		}
		private void CreateSiteBorderVertices()
		{
			var borderLineIndex = 0;

			_borderLineCount = _map.Sites.Values.Sum(s => s.Data.RegionPoints.Length);
			_borderLineVertices = new VertexPositionColor[_borderLineCount  * 2];

			foreach (var site in _map.Sites.Values.Select(s => s.Data))
			{
				// store vertices for the site border line segments
				var last = PointF.Empty;
				foreach (var rp in site.RegionPoints)
				{
					if (last != PointF.Empty)
					{
						_borderLineVertices[borderLineIndex] = new VertexPositionColor(new Vector3(last.X, last.Y, 0), BORDER_COLOR);
						_borderLineVertices[borderLineIndex + 1] = new VertexPositionColor(new Vector3(rp.X, rp.Y, 0), BORDER_COLOR);
						borderLineIndex += 2;
					}
					last = rp;
				}
			}
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
		private void OnSiteUpdated(object sender, EventArgs e)
		{
			_sitesToUpdate.Enqueue((Site)sender, 0);
		}
		private void UpdateSite(Site site)
		{
			if (site == null)
				return;

			if (_siteTriangleVertices == null)
				return;

			var i = site.VertexBufferIndex;
			for (var count = 0; count < site.RegionPoints.Length * 3; count++)
				_siteTriangleVertices[i++].Color = site.GetXnaColor();
		}
	}
}