using System;

namespace vMap.MonoGame
{
	public interface IMouseHandler
	{
		void HandleMouse_ControlLeftButtonClick();
		void HandleMouse_ControlRightButtonClick();
		void HandleMouse_ShiftLeftButtonClick();
		void HandleMouse_ShiftRightButtonClick();
		void HandleMouse_LeftButtonClick();
		void HandleMouse_RightButtonClick();
		void HandleMouse_ControlHover();
		void HandleMouse_ShiftHover();
		void HandleMouse_Hover();
	}
}