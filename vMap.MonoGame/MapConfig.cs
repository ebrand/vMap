using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using vMap.Voronoi;
using System.Collections.Generic;
using LibNoise;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using System.Windows.Forms;

using Keys = Microsoft.Xna.Framework.Input.Keys;
using MGE  = MonoGame.Extended;

namespace vMap.MonoGame
{
	public class MapConfig
	{
		public static class Constants
		{
			public static Color			BG_COLOR               = Color.White;
			public static Color			BORDER_COLOR           = Color.Black;
			public static Color			DELAUNAY_COLOR         = Color.LightGray;
			public static Color			CENTERPOINT_COLOR      = Color.Black;
			public static Color			HELP_TEXT_COLOR        = Color.Black;
			public static bool			FULL_SCREEN            = true;
			public static int			WINDOW_WIDTH           = 1024;
			public static int			WINDOW_HEIGHT          = 768;
			
			public static double		NOISE_FREQUENCY        = 0.003D;
			public static NoiseQuality	NOISE_QUALITY          = NoiseQuality.Low;
			public static int			NOISE_OCTAVES          = 6;
			public static double		NOISE_LACUNARITY       = 1.750D;
			public static double		NOISE_PERSISTENCE      = 0.650D;
			public static float			NOISE_SCALE            = 0.250f;
			
			public static int			AGENT_COUNT            = 10;
			public static int			INIT_RELAX_ITERATIONS  = 4;
			public static int			SITE_COUNT             = 10000;
			public static bool			SHOW_SITE_BORDERS      = false;
			public static bool			SHOW_SITE_CENTERPOINTS = false;
			public static bool			SHOW_DELAUNAY          = false;
			public static bool			SHOW_HELP              = true;
			public static bool			SHOW_AGENTS            = false;
			public static bool			FILL_SITES             = true;
		}

		public IMouseHandler						  MouseHandler;
		public IKeyHandler							  KeyHandler;
		public GraphicsDeviceManager				  GraphicsDeviceManager;
		public GraphicsDevice						  GfxDev;
		public FramesPerSecondCounter				  Fps;
		public PriorityEventQueue<Site>				  SitesToUpdate;
		public PriorityEventQueue<Agent>			  AgentsToUpdate;

		public Texture2D							  Pixel;
		public Map									  Map;
		public int									  SiteTriangleCount;
		public int									  DelaunayLineCount;
		public int									  BorderLineCount;
		public long									  DrawTimeTicks;
		public long									  UpdateTimeTicks;
		public long									  LoadContentTimeTicks;
		public long									  InitTimeTicks;
		public long									  NoiseTimeTicks;
		public long									  KeyboardUpdateTimeTicks;
		public long									  MouseUpdateTimeTicks;
		public SpriteBatch							  SpriteBatch;
		public System.Drawing.Rectangle				  PlotBounds;
		public SpriteFont							  FontSegoe8Bold;
		public SpriteFont							  FontSegoe10Regular;
		public SpriteFont							  FontSegoe10Bold;
		public SpriteFont							  FontSegoe12Regular;
		public SpriteFont							  FontSegoe12Bold;
		public KeyboardState						  CurrentKeyboardState;
		public KeyboardState						  PreviousKeyboardState;
		public MouseState							  CurrentMouseState;
		public MouseState							  PreviousMouseState;
		public bool									  MouseMoved;
		public string								  HelpText;
		public string								  HelpText2;
		public string								  TimingHeadingText;
		public Agent[]								  Agents;
		public Texture2D							  AgentSprite;
		public Agent								  Rogue;
		public Texture2D							  RogueSprite;
		public GraphVertex<Site, Coordinate>		  MouseHoverSite;
		public StartAStarSearchDelegate				  StartAStarSearchDelegate;
		public VertexPositionColor[]				  SiteTriangleVertices;
		public VertexPositionColor[]				  DelaunayLineVertices;
		public VertexPositionColor[]				  BorderLineVertices;
		public Dictionary<Keys[], KeyHandlerDelegate> KeyHandlers;

		public MapConfig(Game game)
		{
			this.MouseHandler = new DefaultMouseHandler(this);
			this.KeyHandler = new DefaultKeyHandler(this);

			// create a new 'GraphicsDeviceManager' based on whether we are in full-screen
			// or windowed mode
			if (MapConfig.Constants.FULL_SCREEN)
				this.GraphicsDeviceManager =
					new GraphicsDeviceManager(game)
					{
						PreferredBackBufferWidth = Screen.PrimaryScreen.Bounds.Width,
						PreferredBackBufferHeight = Screen.PrimaryScreen.Bounds.Height,
						IsFullScreen = true
					};
			else
				this.GraphicsDeviceManager =
					new GraphicsDeviceManager(game)
					{
						PreferredBackBufferWidth = MapConfig.Constants.WINDOW_WIDTH,
						PreferredBackBufferHeight = MapConfig.Constants.WINDOW_HEIGHT
					};

			// tell XNA to call the Draw() method as fast as posible
			this.GraphicsDeviceManager.SynchronizeWithVerticalRetrace = false;

			// create an FPS counter
			this.Fps = new MGE.FramesPerSecondCounter();

			// create a new priority event queue to handle site (cell) updates to the UI
			this.SitesToUpdate = new PriorityEventQueue<Site>();

			// create a new priority event queue to handle agent updates to the UI
			this.AgentsToUpdate = new PriorityEventQueue<Agent>();
		}
	}
}