using UImGui.Assets;
using UnityEngine;

namespace UImGui.Platform
{
	internal static class PlatformUtility
	{
		internal static IPlatform Create(InputType type, CursorShapesAsset cursors, IniSettingsAsset iniSettings)
		{
			switch (type)
			{
				case InputType.InputManager:
					return new InputManagerPlatform(cursors, iniSettings);
				// TODO: Implement InputSystemPlatform.
#if HAS_INPUTSYSTEM
				//case InputType.InputSystem:
				//	return new InputSystemPlatform(cursors, iniSettings);
#endif
				default:
					Debug.LogError($"[DearImGui] {type} platform not available.");
					return null;
			}
		}
	}
}