using ImGuiNET;
using System;
using UImGui.Texture;
using UnityEngine;
using UTexture = UnityEngine.Texture;

namespace UImGui
{
	public static class UImGuiUtility
	{
		public static IntPtr GetTextureId(UTexture texture) => Context?.TextureManager.GetTextureId(texture) ?? IntPtr.Zero;
		internal static SpriteInfo GetSpriteInfo(Sprite sprite) => Context?.TextureManager.GetSpriteInfo(sprite) ?? null;

		public static Context Context { get; internal set; }

		#region Events
		public static event Action<UImGui> Layout;
		public static event Action<UImGui> OnInitialize;
		public static event Action<UImGui> OnDeinitialize;
		internal static void DoLayout(UImGui uimgui) => Layout?.Invoke(uimgui);
		internal static void DoOnInitialize(UImGui uimgui) => OnInitialize?.Invoke(uimgui);
		internal static void DoOnDeinitialize(UImGui uimgui) => OnDeinitialize?.Invoke(uimgui);
		#endregion

		public static unsafe Context CreateContext()
		{
			IntPtr imGuiContext = ImGui.CreateContext();
			var context = new Context
			{
				ImGuiContext = imGuiContext,
				TextureManager = new TextureManager()
			};

			PluginRegistry.CreateContextAll(context);

			return context;
		}

		public static void DestroyContext(Context context)
		{
			if (context == null)
				return;

			PluginRegistry.DestroyContextAll(context);

			ImGui.DestroyContext(context.ImGuiContext);
		}

		public static void SetCurrentContext(Context context)
		{
			Context = context;
			ImGui.SetCurrentContext(context?.ImGuiContext ?? IntPtr.Zero);

			PluginRegistry.SetCurrentContextAll(context);
		}
	}
}
