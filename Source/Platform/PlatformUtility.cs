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
#if !ENABLE_LEGACY_INPUT_MANAGER
					return false;
#else
					return true;
#endif
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
#if HAS_INPUTSYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
					Debug.LogWarning("[DearImGui] Input Manager is disabled in Player Settings. Falling back to Input System.");
					return new InputSystemPlatform(cursors, iniSettings);
#else
					return new InputManagerPlatform(cursors, iniSettings);
#endif
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
