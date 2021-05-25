using ImGuiNET;
using System;
using UImGui.Texture;
using UnityEngine;
using UTexture = UnityEngine.Texture;

namespace UImGui
{
	internal static class UImGuiUtility
	{
		public static event Action Layout; // Global/default Layout event, each DearImGui instance also has a private one.
		internal static void DoLayout() => Layout?.Invoke();

		public static IntPtr GetTextureId(UTexture texture) => Context?.TextureManager.GetTextureId(texture) ?? IntPtr.Zero;
		internal static SpriteInfo GetSpriteInfo(Sprite sprite) => Context?.TextureManager.GetSpriteInfo(sprite) ?? null;

		internal static Context Context;

		internal static Context CreateContext()
		{
			return new Context
			{
				Value = ImGui.CreateContext(),
				TextureManager = new TextureManager()
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