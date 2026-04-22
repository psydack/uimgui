#if UIMGUI_ENABLE_IMGUIZMO_QUAT
using System;

namespace UImGui
{
	internal sealed class ImGuizmoQuatPlugin : IOptionalPlugin
	{
		public void Create(Context context)
		{
		}

		public void SetCurrent(Context context)
		{
			ImGuizmoQuatNET.ImGuizmoQuat.SetImGuiContext(context?.ImGuiContext ?? IntPtr.Zero);
		}

		public void Destroy(Context context)
		{
			ImGuizmoQuatNET.ImGuizmoQuat.SetImGuiContext(IntPtr.Zero);
		}
	}
}
#endif
