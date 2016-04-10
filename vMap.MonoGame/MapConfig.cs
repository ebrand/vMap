using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using vMap.Voronoi;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;

namespace vMap.MonoGame
{
	public class MapConfig
	{
		public Color BG_COLOR						  = Color.White;
		public Color BORDER_COLOR					  = Color.Black;
		public Color DELAUNAY_COLOR					  = Color.LightGray;
		public Color CENTERPOINT_COLOR				  = Color.Black;
		public Color HELP_TEXT_COLOR				  = Color.Black;
		public bool  FULL_SCREEN					  = true;
		public int   WINDOW_WIDTH					  = 1024;
		public int   WINDOW_HEIGHT					  = 768;
		public float NOISE_SCALE					  = 0.25f;
		public int   AGENT_COUNT					  = 10;
		public int	 INIT_RELAX_ITERATIONS			  = 4;
		public int   SiteCount						  = 15000;
		public bool  ShowSiteBorders				  = false;
		public bool  ShowSiteCenterpoints			  = false;
		public bool  ShowDelaunay					  = false;
		public bool  ShowHelp						  = true;
		public bool  ShowAgents						  = false;
		public bool  FillSites						  = true;
													  
		public int									  SiteTriangleCount;
		public int									  DelaunayLineCount;
		public int									  BorderLineCount;
		public long									  DrawTimeTicks;
		public long									  InitTimeTicks;
		public long									  NoiseTimeTicks;
		public long									  MouseUpdateTimeTicks;
		public GraphicsDeviceManager				  GraphicsDeviceManager;
		public GraphicsDevice						  GfxDev;
		public PriorityEventQueue<Site>				  SitesToUpdate;
		public PriorityEventQueue<Agent>			  AgentsToUpdate;
		public SpriteBatch							  SpriteBatch;
		public Map									  Map;
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
		public Texture2D							  Pixel;
		public Agent[]								  Agents;
		public Texture2D							  AgentSprite;
		public Agent								  Rogue;
		public Texture2D							  RogueSprite;
		public GraphVertex<Site, Coordinate>		  MouseHoverSite;
		public StartAStarSearchDelegate				  StartAStarSearchDelegate;
		public FramesPerSecondCounter				  Fps;
		public VertexPositionColor[]				  SiteTriangleVertices;
		public VertexPositionColor[]				  DelaunayLineVertices;
		public VertexPositionColor[]				  BorderLineVertices;
		public Dictionary<Keys[], KeyHandlerDelegate> KeyHandlers;

		public MapConfig()
		{}
	}
}