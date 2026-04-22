#if UIMGUI_ENABLE_IMGUIZMO
using UnityEngine;

namespace UImGui
{
	internal sealed class ImGuizmoPlugin : IOptionalPlugin
	{
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		static void Register() => PluginRegistry.Register(new ImGuizmoPlugin());

		public void CreateContext(Context ctx) { }

		public void SetCurrentContext(Context ctx)
			=> ImGuizmoNET.ImGuizmo.SetImGuiContext(ctx?.ImGuiContext ?? System.IntPtr.Zero);

		public void DestroyContext(Context ctx) { }
	}
}
#endif
