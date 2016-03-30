using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Input;

namespace vMap.MonoGame
{
	public class DefaultKeyHandler : IKeyHandler
	{
		public KeyboardState PreviousKeyboardState { get; set; }
		public KeyboardState CurrentKeyboardState { get; set; }
		public Dictionary<Keys[], KeyHandlerDelegate> KeyHandlers { get; set; }
		public Keys[] PressedKeys { get; set; }
		
		public void HandleKeys()
		{
			this.PreviousKeyboardState = this.CurrentKeyboardState;
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
		}
	}
}