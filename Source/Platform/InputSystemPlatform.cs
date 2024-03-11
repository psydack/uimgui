#if HAS_INPUTSYSTEM
using ImGuiNET;
using System;
using System.Collections.Generic;
using UImGui.Assets;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace UImGui.Platform
{
    // Implemented features:
    // [x] Platform: Clipboard support.
    // [x] Platform: Mouse cursor shape and visibility. Disable with io.ConfigFlags |= ImGuiConfigFlags.NoMouseCursorChange.
    // [x] Platform: Keyboard arrays indexed using InputSystem.Key codes, e.g. ImGui.IsKeyPressed(Key.Space).
    // [x] Platform: Gamepad support. Enabled with io.ConfigFlags |= ImGuiConfigFlags.NavEnableGamepad.
    // [~] Platform: IME support.
    // [~] Platform: INI settings support.

    /// <summary>
    /// Platform bindings for ImGui in Unity in charge of: mouse/keyboard/gamepad inputs, cursor shape, timing, windowing.
    /// </summary>
    internal sealed class InputSystemPlatform : PlatformBase
    {
        private readonly List<char> _textInput = new();

        private readonly List<KeyControl> _keyControls = new();

        private Keyboard _keyboard;

        public InputSystemPlatform(CursorShapesAsset cursorShapes, IniSettingsAsset iniSettings)
            : base(cursorShapes, iniSettings)
        {
        }

        private static void UpdateMouse(ImGuiIOPtr io, Mouse mouse)
        {
            if (mouse == null)
            {
                return;
            }

            // Set Unity mouse position if requested.
            if (io.WantSetMousePos)
            {
                mouse.WarpCursorPosition(Utils.ImGuiToScreen(io.MousePos));
            }

            io.MousePos = Utils.ScreenToImGui(mouse.position.ReadValue());

            var mouseScroll = mouse.scroll.ReadValue() / 120f;
            io.MouseWheel = mouseScroll.y;
            io.MouseWheelH = mouseScroll.x;

            io.MouseDown[0] = mouse.leftButton.isPressed;
            io.MouseDown[1] = mouse.rightButton.isPressed;
            io.MouseDown[2] = mouse.middleButton.isPressed;
        }

        private static void UpdateGamepad(ImGuiIOPtr io, Gamepad gamepad)
        {
            io.BackendFlags = gamepad == null ? io.BackendFlags & ~ImGuiBackendFlags.HasGamepad : io.BackendFlags | ImGuiBackendFlags.HasGamepad;

            if (gamepad == null || (io.ConfigFlags & ImGuiConfigFlags.NavEnableGamepad) == 0)
            {
                return;
            }

            // TO DO: Confirm it's working. NOT TESTED

            io.AddKeyAnalogEvent(ImGuiKey.GamepadStart, gamepad.aButton.IsPressed(), gamepad.aButton.ReadValue()); // A / Cross
            io.AddKeyAnalogEvent(ImGuiKey.GamepadBack, gamepad.bButton.IsPressed(), gamepad.bButton.ReadValue()); // A / Cross

            io.AddKeyEvent(ImGuiKey.GamepadFaceDown, gamepad.buttonSouth.IsPressed()); // A / Cross
            io.AddKeyEvent(ImGuiKey.GamepadFaceRight, gamepad.buttonEast.IsPressed()); // B / Circle
            io.AddKeyEvent(ImGuiKey.GamepadFaceLeft, gamepad.buttonWest.IsPressed()); // X / Square
            io.AddKeyEvent(ImGuiKey.GamepadFaceUp, gamepad.buttonNorth.IsPressed()); // Y / Triangle

            io.AddKeyAnalogEvent(ImGuiKey.GamepadDpadDown, gamepad.dpad.down.IsPressed(), gamepad.dpad.down.ReadValue());
            io.AddKeyAnalogEvent(ImGuiKey.GamepadDpadRight, gamepad.dpad.right.IsPressed(), gamepad.dpad.right.ReadValue());
            io.AddKeyAnalogEvent(ImGuiKey.GamepadDpadLeft, gamepad.dpad.left.IsPressed(), gamepad.dpad.left.ReadValue());
            io.AddKeyAnalogEvent(ImGuiKey.GamepadDpadUp, gamepad.dpad.up.IsPressed(), gamepad.dpad.up.ReadValue());

            io.AddKeyAnalogEvent(ImGuiKey.GamepadL1, gamepad.leftShoulder.IsPressed(), gamepad.leftShoulder.ReadValue()); // LB / L1
            io.AddKeyAnalogEvent(ImGuiKey.GamepadL2, gamepad.leftTrigger.IsPressed(), gamepad.leftTrigger.ReadValue()); // LB / L2
            io.AddKeyAnalogEvent(ImGuiKey.GamepadR1, gamepad.rightShoulder.IsPressed(), gamepad.rightShoulder.ReadValue()); // RB / R1
            io.AddKeyAnalogEvent(ImGuiKey.GamepadR2, gamepad.rightTrigger.IsPressed(), gamepad.rightTrigger.ReadValue()); // RB / R2

            io.AddKeyAnalogEvent(ImGuiKey.GamepadLStickDown, gamepad.leftStick.down.IsPressed(), gamepad.leftStick.down.ReadValue());
            io.AddKeyAnalogEvent(ImGuiKey.GamepadLStickLeft, gamepad.leftStick.left.IsPressed(), gamepad.leftStick.left.ReadValue());
            io.AddKeyAnalogEvent(ImGuiKey.GamepadLStickRight, gamepad.leftStick.right.IsPressed(), gamepad.leftStick.right.ReadValue());
            io.AddKeyAnalogEvent(ImGuiKey.GamepadLStickUp, gamepad.leftStick.up.IsPressed(), gamepad.leftStick.up.ReadValue());
            io.AddKeyAnalogEvent(ImGuiKey.GamepadL3, gamepad.leftStickButton.IsPressed(), gamepad.leftStickButton.ReadValue());

            io.AddKeyAnalogEvent(ImGuiKey.GamepadRStickDown, gamepad.rightStick.down.IsPressed(), gamepad.rightStick.down.ReadValue());
            io.AddKeyAnalogEvent(ImGuiKey.GamepadRStickLeft, gamepad.rightStick.left.IsPressed(), gamepad.rightStick.left.ReadValue());
            io.AddKeyAnalogEvent(ImGuiKey.GamepadRStickRight, gamepad.rightStick.right.IsPressed(), gamepad.rightStick.right.ReadValue());
            io.AddKeyAnalogEvent(ImGuiKey.GamepadRStickUp, gamepad.rightStick.up.IsPressed(), gamepad.rightStick.up.ReadValue());
            io.AddKeyAnalogEvent(ImGuiKey.GamepadR3, gamepad.rightStickButton.IsPressed(), gamepad.rightStickButton.ReadValue());
        }

        private void SetupKeyboard(Keyboard keyboard)
        {
            if (_keyboard != null)
            {
                _keyboard.onTextInput -= _textInput.Add;
            }

            _keyboard = keyboard;
            _keyControls.Clear();

            // Map and store new keys by assigning io.KeyMap and setting value of array.
            _keyboard.onTextInput += _textInput.Add;
        }

        private void UpdateKeyboard(ImGuiIOPtr io, Keyboard keyboard)
        {
            if (keyboard == null)
            {
                return;
            }

            // BUG: mod key make everything slow. Go to line
            for (int keyIndex = 0; keyIndex < Keyboard.KeyCount; keyIndex++)
            {
                Key key = (Key)keyIndex;
                if (TryMapKeys(key, out ImGuiKey imguikey))
                {
                    KeyControl keyControl = keyboard[key];
                    io.AddKeyEvent(imguikey, keyControl.IsPressed());
                }
            }

            io.KeyShift = keyboard[Key.LeftShift].isPressed || keyboard[Key.RightShift].isPressed;
            io.KeyCtrl = keyboard[Key.LeftCtrl].isPressed || keyboard[Key.RightCtrl].isPressed;
            io.KeyAlt = keyboard[Key.LeftAlt].isPressed || keyboard[Key.RightAlt].isPressed;
            io.KeySuper = keyboard[Key.LeftMeta].isPressed || keyboard[Key.RightMeta].isPressed;

            // Text input.
            for (int i = 0, iMax = _textInput.Count; i < iMax; ++i)
            {
                io.AddInputCharacter(_textInput[i]);
            }

            _textInput.Clear();
        }

        private bool TryMapKeys(Key key, out ImGuiKey imguikey)
        {
            static ImGuiKey KeyToImGuiKeyShortcut(Key keyToConvert, Key startKey1, ImGuiKey startKey2)
            {
                int changeFromStart1 = (int)keyToConvert - (int)startKey1;
                return startKey2 + changeFromStart1;
            }

            imguikey = key switch
            {
                >= Key.F1 and <= Key.F12 => KeyToImGuiKeyShortcut(key, Key.F1, ImGuiKey.F1),
                >= Key.Numpad0 and <= Key.Numpad9 => KeyToImGuiKeyShortcut(key, Key.Numpad0, ImGuiKey.Keypad0),
                >= Key.A and <= Key.Z => KeyToImGuiKeyShortcut(key, Key.A, ImGuiKey.A),
                >= Key.Digit1 and <= Key.Digit9 => KeyToImGuiKeyShortcut(key, Key.Digit1, ImGuiKey._1),
                Key.Digit0 => ImGuiKey._0,
                // BUG: mod keys make everything slow. 
                // Key.LeftShift or Key.RightShift => ImGuiKey.ModShift,
                // Key.LeftCtrl or Key.RightCtrl => ImGuiKey.ModCtrl,
                // Key.LeftAlt or Key.RightAlt => ImGuiKey.ModAlt,
                Key.LeftWindows or Key.RightWindows => ImGuiKey.ModSuper,
                Key.ContextMenu => ImGuiKey.Menu,
                Key.UpArrow => ImGuiKey.UpArrow,
                Key.DownArrow => ImGuiKey.DownArrow,
                Key.LeftArrow => ImGuiKey.LeftArrow,
                Key.RightArrow => ImGuiKey.RightArrow,
                Key.Enter => ImGuiKey.Enter,
                Key.Escape => ImGuiKey.Escape,
                Key.Space => ImGuiKey.Space,
                Key.Tab => ImGuiKey.Tab,
                Key.Backspace => ImGuiKey.Backspace,
                Key.Insert => ImGuiKey.Insert,
                Key.Delete => ImGuiKey.Delete,
                Key.PageUp => ImGuiKey.PageUp,
                Key.PageDown => ImGuiKey.PageDown,
                Key.Home => ImGuiKey.Home,
                Key.End => ImGuiKey.End,
                Key.CapsLock => ImGuiKey.CapsLock,
                Key.ScrollLock => ImGuiKey.ScrollLock,
                Key.PrintScreen => ImGuiKey.PrintScreen,
                Key.Pause => ImGuiKey.Pause,
                Key.NumLock => ImGuiKey.NumLock,
                Key.NumpadDivide => ImGuiKey.KeypadDivide,
                Key.NumpadMultiply => ImGuiKey.KeypadMultiply,
                Key.NumpadMinus => ImGuiKey.KeypadSubtract,
                Key.NumpadPlus => ImGuiKey.KeypadAdd,
                Key.NumpadPeriod => ImGuiKey.KeypadDecimal,
                Key.NumpadEnter => ImGuiKey.KeypadEnter,
                Key.NumpadEquals => ImGuiKey.KeypadEqual,
                Key.Backquote => ImGuiKey.GraveAccent,
                Key.Minus => ImGuiKey.Minus,
                Key.Equals => ImGuiKey.Equal,
                Key.LeftBracket => ImGuiKey.LeftBracket,
                Key.RightBracket => ImGuiKey.RightBracket,
                Key.Semicolon => ImGuiKey.Semicolon,
                Key.Quote => ImGuiKey.Apostrophe,
                Key.Comma => ImGuiKey.Comma,
                Key.Period => ImGuiKey.Period,
                Key.Slash => ImGuiKey.Slash,
                Key.Backslash => ImGuiKey.Backslash,
                _ => ImGuiKey.None
            };

            return imguikey != ImGuiKey.None;
        }

        private void OnDeviceChange(InputDevice device, InputDeviceChange change)
        {
            if (device is Keyboard keyboard)
            {
                // Keyboard layout change, remap main keys.
                if (change == InputDeviceChange.ConfigurationChanged)
                {
                    SetupKeyboard(keyboard);
                }

                // Keyboard device changed, setup again.
                if (Keyboard.current != _keyboard)
                {
                    SetupKeyboard(Keyboard.current);
                }
            }
        }

        #region Overrides of PlatformBase

        public override bool Initialize(ImGuiIOPtr io, UIOConfig config, string platformName)
        {
            InputSystem.onDeviceChange += OnDeviceChange;
            base.Initialize(io, config, platformName);

            io.BackendFlags |= ImGuiBackendFlags.HasMouseCursors;

            unsafe
            {
                PlatformCallbacks.SetClipboardFunctions(PlatformCallbacks.GetClipboardTextCallback, PlatformCallbacks.SetClipboardTextCallback);
            }

            SetupKeyboard(Keyboard.current);

            return true;
        }

        public override void Shutdown(ImGuiIOPtr io)
        {
            base.Shutdown(io);
            InputSystem.onDeviceChange -= OnDeviceChange;
        }

        public override void PrepareFrame(ImGuiIOPtr io, Rect displayRect)
        {
            base.PrepareFrame(io, displayRect);

            try
            {
                UpdateKeyboard(io, Keyboard.current);
                UpdateMouse(io, Mouse.current);
                UpdateCursor(io, ImGui.GetMouseCursor());
                UpdateGamepad(io, Gamepad.current);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        #endregion
    }
}
#endif