using ImGuiNET;
using System;

namespace UImGui
{
	internal static class UImGuiUtility
	{
		public static event Action Layout; // Global/default Layout event, each DearImGui instance also has a private one.
		internal static void DoLayout() => Layout?.Invoke();

		internal static Context Context;

		internal static Context CreateContext()
		{
			return new Context
			{
				Value = ImGui.CreateContext(),
				TextureManager = new Texture.TextureManager()
			};
		}

		internal static void DestroyContext(Context context)
		{
			ImGui.DestroyContext(context.Value);
		}

		internal static void SetCurrentContext(Context context)
		{
			Context = context;
			ImGui.SetCurrentContext(context?.Value ?? IntPtr.Zero);
		}
	}
}