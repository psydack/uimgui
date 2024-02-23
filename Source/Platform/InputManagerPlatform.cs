using ImGuiNET;
using UImGui.Assets;
using UnityEngine;

namespace UImGui.Platform
{
	// TODO: Check this feature and remove from here when checked and done.
	// Implemented features:
	// [x] Platform: Clipboard support.
	// [x] Platform: Mouse cursor shape and visibility. Disable with io.ConfigFlags |= ImGuiConfigFlags.NoMouseCursorChange.
	// [x] Platform: Keyboard arrays indexed using KeyCode codes, e.g. ImGui.IsKeyPressed(KeyCode.Space).
	// [ ] Platform: Gamepad support. Enabled with io.ConfigFlags |= ImGuiConfigFlags.NavEnableGamepad.
	// [~] Platform: IME support.
	// [~] Platform: INI settings support.

	/// <summary>
	/// Platform bindings for ImGui in Unity in charge of: mouse/keyboard/gamepad inputs, cursor shape, timing, windowing.
	/// </summary>
	internal sealed class InputManagerPlatform : PlatformBase
	{
		private readonly Event _textInputEvent = new Event();

		private int[] _mainKeys;

		public InputManagerPlatform(CursorShapesAsset cursorShapes, IniSettingsAsset iniSettings) :
			base(cursorShapes, iniSettings)
		{ }

		public override bool Initialize(ImGuiIOPtr io, UIOConfig config, string platformName)
		{
			base.Initialize(io, config, platformName);

			SetupKeyboard(io);

			return true;
		}

		public override void PrepareFrame(ImGuiIOPtr io, Rect displayRect)
		{
			base.PrepareFrame(io, displayRect);

			UpdateKeyboard(io);
			UpdateMouse(io);
			UpdateCursor(io, ImGui.GetMouseCursor());
		}

		private void SetupKeyboard(ImGuiIOPtr io)
		{
			/*
			// Map and store new keys by assigning io.KeyMap and setting value of array
			_mainKeys = new int[] {
				io.KeyMap[(int)ImGuiKey.A] = (int)KeyCode.A, // For text edit CTRL+A: select all.
				io.KeyMap[(int)ImGuiKey.C] = (int)KeyCode.C, // For text edit CTRL+C: copy.
				io.KeyMap[(int)ImGuiKey.V] = (int)KeyCode.V, // For text edit CTRL+V: paste.
				io.KeyMap[(int)ImGuiKey.X] = (int)KeyCode.X, // For text edit CTRL+X: cut.
				io.KeyMap[(int)ImGuiKey.Y] = (int)KeyCode.Y, // For text edit CTRL+Y: redo.
				io.KeyMap[(int)ImGuiKey.Z] = (int)KeyCode.Z, // For text edit CTRL+Z: undo.

				io.KeyMap[(int)ImGuiKey.Tab] = (int)KeyCode.Tab,

				io.KeyMap[(int)ImGuiKey.LeftArrow] = (int)KeyCode.LeftArrow,
				io.KeyMap[(int)ImGuiKey.RightArrow] = (int)KeyCode.RightArrow,
				io.KeyMap[(int)ImGuiKey.UpArrow] = (int)KeyCode.UpArrow,
				io.KeyMap[(int)ImGuiKey.DownArrow] = (int)KeyCode.DownArrow,

				io.KeyMap[(int)ImGuiKey.PageUp] = (int)KeyCode.PageUp,
				io.KeyMap[(int)ImGuiKey.PageDown] = (int)KeyCode.PageDown,

				io.KeyMap[(int)ImGuiKey.Home] = (int)KeyCode.Home,
				io.KeyMap[(int)ImGuiKey.End] = (int)KeyCode.End,
				io.KeyMap[(int)ImGuiKey.Insert] = (int)KeyCode.Insert,
				io.KeyMap[(int)ImGuiKey.Delete] = (int)KeyCode.Delete,
				io.KeyMap[(int)ImGuiKey.Backspace] = (int)KeyCode.Backspace,

				io.KeyMap[(int)ImGuiKey.Space] = (int)KeyCode.Space,
				io.KeyMap[(int)ImGuiKey.Escape] = (int)KeyCode.Escape,
				io.KeyMap[(int)ImGuiKey.Enter] = (int)KeyCode.Return,
				io.KeyMap[(int)ImGuiKey.KeyPadEnter] = (int)KeyCode.KeypadEnter,
			};*/
		}

		private void UpdateKeyboard(ImGuiIOPtr io)
		{
			/*
			for (int keyIndex = 0; keyIndex < _mainKeys.Length; keyIndex++)
			{
				int key = _mainKeys[keyIndex];
				io.KeysDown[key] = Input.GetKey((KeyCode)key);
			}

			// Keyboard modifiers.
			io.KeyShift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
			io.KeyCtrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
			io.KeyAlt = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
			io.KeySuper = Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.RightCommand) ||
				Input.GetKey(KeyCode.LeftWindows) || Input.GetKey(KeyCode.RightWindows);

			// Text input.
			while (Event.PopEvent(_textInputEvent))
			{
				if (_textInputEvent.rawType == EventType.KeyDown &&
					_textInputEvent.character != 0 && _textInputEvent.character != '\n')
				{
					io.AddInputCharacter(_textInputEvent.character);
				}
			}
			*/
		}

		private static void UpdateMouse(ImGuiIOPtr io)
		{
			Vector2 mousePosition = Utils.ScreenToImGui(Input.mousePosition);
			io.AddMousePosEvent(mousePosition.x, mousePosition.y);
			io.AddMouseButtonEvent(0, Input.GetMouseButton(0));
			io.AddMouseButtonEvent(1, Input.GetMouseButton(1));
			io.AddMouseButtonEvent(2, Input.GetMouseButton(2));
			io.AddMouseWheelEvent(Input.mouseScrollDelta.x, Input.mouseScrollDelta.y);
		}
	}
}