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

#if UIMGUI_ENABLE_IMPLOT
			context.ImPlotContext = ImPlotNET.ImPlot.CreateContext();
#endif
#if UIMGUI_ENABLE_IMPLOT3D
			context.ImPlot3DContext = ImPlot3DNET.ImPlot3D.CreateContext();
#endif
#if UIMGUI_ENABLE_IMNODES
			context.ImNodesContext = imnodesNET.imnodes.CreateContext();
#endif
#if UIMGUI_ENABLE_IMNODES_R
			ImNodesRNET.ImNodesR.SetImGuiContext(imGuiContext);
			context.ImNodesRContext = ImNodesRNET.ImNodesR.CreateContext();
			ImNodesRNET.ImNodesR.SetContext(context.ImNodesRContext);
#endif

			return context;
		}

		public static void DestroyContext(Context context)
		{
			if (context == null)
				return;

#if UIMGUI_ENABLE_IMPLOT
			ImPlotNET.ImPlot.DestroyContext(context.ImPlotContext);
#endif
#if UIMGUI_ENABLE_IMPLOT3D
			ImPlot3DNET.ImPlot3D.DestroyContext(context.ImPlot3DContext);
#endif
#if UIMGUI_ENABLE_IMNODES
			imnodesNET.imnodes.DestroyContext(context.ImNodesContext);
#endif
#if UIMGUI_ENABLE_IMNODES_R
			if (context.ImNodesRContext != IntPtr.Zero)
			{
				ImNodesRNET.ImNodesR.SetContext(context.ImNodesRContext);
				ImNodesRNET.ImNodesR.FreeContext(context.ImNodesRContext);
				context.ImNodesRContext = IntPtr.Zero;
			}

			ImNodesRNET.ImNodesR.SetContext(IntPtr.Zero);
			ImNodesRNET.ImNodesR.SetImGuiContext(IntPtr.Zero);
#endif

			ImGui.DestroyContext(context.ImGuiContext);
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
			ImNodesRNET.ImNodesR.SetContext(context?.ImNodesRContext ?? IntPtr.Zero);
#endif
#if UIMGUI_ENABLE_CIMCTE
			CimCTENET.CimCTE.SetImGuiContext(context?.ImGuiContext ?? IntPtr.Zero);
#endif
		}
	}
}
