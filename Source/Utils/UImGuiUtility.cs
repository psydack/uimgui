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

		internal static Context Context;

		#region Events
		public static event Action<UImGui> Layout;
		public static event Action<UImGui> OnInitialize;
		public static event Action<UImGui> OnDeinitialize;
		internal static void DoLayout(UImGui uimgui) => Layout?.Invoke(uimgui);
		internal static void DoOnInitialize(UImGui uimgui) => OnInitialize?.Invoke(uimgui);
		internal static void DoOnDeinitialize(UImGui uimgui) => OnDeinitialize?.Invoke(uimgui);
		#endregion

		internal static unsafe Context CreateContext()
		{
			return new Context
			{
				ImGuiContext = ImGui.CreateContext(),
#if UIMGUI_IMPLOT
				ImPlotContext = ImPlotNET.ImPlot.CreateContext(),
#endif
#if UIMGUI_IMNODES
				ImNodesContext = new IntPtr(imnodesNET.imnodes.CreateContext()),
#endif
				TextureManager = new TextureManager()
			};
		}

		internal static void DestroyContext(Context context)
		{
			ImGui.DestroyContext(context.ImGuiContext);

#if UIMGUI_IMPLOT
			ImPlotNET.ImPlot.DestroyContext(context.ImPlotContext);
#endif
#if UIMGUI_IMNODES
			imnodesNET.imnodes.DestroyContext(context.ImNodesContext);
#endif
		}

		internal static void SetCurrentContext(Context context)
		{
			Context = context;
			ImGui.SetCurrentContext(context?.ImGuiContext ?? IntPtr.Zero);

#if UIMGUI_IMPLOT
			ImPlotNET.ImPlot.SetImGuiContext(context?.ImGuiContext ?? IntPtr.Zero);
#endif
#if UIMGUI_IMGUIZMO
			ImGuizmoNET.ImGuizmo.SetImGuiContext(context?.ImGuiContext ?? IntPtr.Zero);
#endif
#if UIMGUI_IMNODES
			imnodesNET.imnodes.SetImGuiContext(context?.ImGuiContext ?? IntPtr.Zero);
#endif
		}
	}
}