using UImGui.Assets;
using UnityEngine;

namespace UImGui.Platform
{
	internal static class PlatformUtility
	{
#if UNITY_EDITOR
		public static bool IsAvailable(InputType type)
		{
			switch (type)
			{
				case InputType.InputManager:
					return true;
#if HAS_INPUTSYSTEM
				case InputType.InputSystem:
					return true;
#endif
				default:
					return false;
			}
		}
#endif

		internal static IPlatform Create(InputType type, CursorShapesAsset cursors, IniSettingsAsset iniSettings)
		{
			switch (type)
			{
				case InputType.InputManager:
					return new InputManagerPlatform(cursors, iniSettings);
#if HAS_INPUTSYSTEM
				case InputType.InputSystem:
					return new InputSystemPlatform(cursors, iniSettings);
#endif
				default:
					Debug.LogError($"[DearImGui] {type} platform not available.");
					return null;
			}
		}
	}
}