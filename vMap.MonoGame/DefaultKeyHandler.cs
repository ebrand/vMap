using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Input;

namespace vMap.MonoGame
{
	public class DefaultKeyHandler : IKeyHandler
	{
		private readonly MapConfig _config;

		public KeyboardState PreviousKeyboardState { get; set; }
		public KeyboardState CurrentKeyboardState { get; set; }
		public Dictionary<Keys[], KeyHandlerDelegate> KeyHandlers { get; set; }
		public Keys[] PressedKeys { get; set; }

		public DefaultKeyHandler(MapConfig config)
		{
			_config = config;
		}
		public void HandleKeys()
		{
			this.CurrentKeyboardState = Keyboard.GetState();
			this.PressedKeys = this.CurrentKeyboardState.GetPressedKeys();

			if(this.CurrentKeyboardState == this.PreviousKeyboardState)
				return;

			KeyHandlerDelegate match = null;
			foreach (var kh in this.KeyHandlers)
			{
				if (this.PressedKeys.OrderBy(a => a).SequenceEqual(kh.Key.OrderBy(a => a)))
				{
					match = kh.Value;
					break;
				}
			}
			match?.Invoke();
			this.PreviousKeyboardState = this.CurrentKeyboardState;
		}
	}
}