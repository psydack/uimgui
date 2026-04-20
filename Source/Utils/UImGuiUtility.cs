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
			return new Context
			{
				ImGuiContext = ImGui.CreateContext(),
#if UIMGUI_ENABLE_IMPLOT
				ImPlotContext = ImPlotNET.ImPlot.CreateContext(),
#endif
#if UIMGUI_ENABLE_IMPLOT3D
				ImPlot3DContext = ImPlot3DNET.ImPlot3D.CreateContext(),
#endif
#if UIMGUI_ENABLE_IMNODES
				ImNodesContext = new IntPtr(imnodesNET.imnodes.CreateContext()),
#endif
#if UIMGUI_ENABLE_IMNODES_R
				ImNodesRContext = ImNodesRNET.ImNodesR.CreateContext(),
#endif
				TextureManager = new TextureManager()
			};
		}

		public static void DestroyContext(Context context)
		{
			ImGui.DestroyContext(context.ImGuiContext);

#if UIMGUI_ENABLE_IMPLOT
			ImPlotNET.ImPlot.DestroyContext(context.ImPlotContext);
#endif
#if UIMGUI_ENABLE_IMPLOT3D
			ImPlot3DNET.ImPlot3D.DestroyContext(context.ImPlot3DContext);
#endif
#if UIMGUI_ENABLE_IMNODES
			imnodesNET.imnodes.DestroyContext(context.ImNodesContext);
#endif
		}

		public static void SetCurrentContext(Context context)
		{
			Context = context;
			ImGui.SetCurrentContext(context?.ImGuiContext ?? IntPtr.Zero);

#if UIMGUI_ENABLE_IMPLOT
			ImPlotNET.ImPlot.SetImGuiContext(context?.ImGuiContext ?? IntPtr.Zero);
#endif
#if UIMGUI_ENABLE_IMPLOT3D
			ImPlot3DNET.ImPlot3D.SetImGuiContext(context?.ImGuiContext ?? IntPtr.Zero);
#endif
#if UIMGUI_ENABLE_IMGUIZMO
			ImGuizmoNET.ImGuizmo.SetImGuiContext(context?.ImGuiContext ?? IntPtr.Zero);
#endif
#if UIMGUI_ENABLE_IMGUIZMO_QUAT
			ImGuizmoQuatNET.ImGuizmoQuat.SetImGuiContext(context?.ImGuiContext ?? IntPtr.Zero);
#endif
#if UIMGUI_ENABLE_IMNODES
			imnodesNET.imnodes.SetImGuiContext(context?.ImGuiContext ?? IntPtr.Zero);
#endif
#if UIMGUI_ENABLE_IMNODES_R
			ImNodesRNET.ImNodesR.SetImGuiContext(context?.ImGuiContext ?? IntPtr.Zero);
#endif
#if UIMGUI_ENABLE_CIMCTE
			CimCTENET.CimCTE.SetImGuiContext(context?.ImGuiContext ?? IntPtr.Zero);
#endif
		}
	}
}
