using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;

namespace vMap.MonoGame
{
	public interface IKeyHandler
	{
		KeyboardState PreviousKeyboardState { get; set; }
		KeyboardState CurrentKeyboardState { get; set; }
		Keys[] PressedKeys { get; }
		Dictionary<Keys[], KeyHandlerDelegate> KeyHandlers { get; set; }
		void HandleKeys();
	}
}