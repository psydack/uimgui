#if UIMGUI_ENABLE_IMGUIZMO_QUAT
using UnityEngine;

namespace UImGui
{
	internal sealed class ImGuizmoQuatPlugin : IOptionalPlugin
	{
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		static void Register() => PluginRegistry.Register(new ImGuizmoQuatPlugin());

		public void CreateContext(Context ctx) { }

		public void SetCurrentContext(Context ctx)
			=> ImGuizmoQuatNET.ImGuizmoQuat.SetImGuiContext(ctx?.ImGuiContext ?? System.IntPtr.Zero);

		public void DestroyContext(Context ctx) { }
	}
}
#endif
