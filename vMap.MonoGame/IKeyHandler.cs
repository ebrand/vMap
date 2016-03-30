using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Input;

namespace vMap.MonoGame
{
	public delegate void KeyHandlerDelegate();

	public interface IKeyHandler
	{
		KeyboardState PreviousKeyboardState { get; set; }
		KeyboardState CurrentKeyboardState { get; set; }
		Keys[] PressedKeys { get; }
		Dictionary<Keys[], KeyHandlerDelegate> KeyHandlers { get; set; }
		void HandleKeys();
	}
}