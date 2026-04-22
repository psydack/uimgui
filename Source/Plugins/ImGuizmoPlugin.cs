#if UIMGUI_ENABLE_IMGUIZMO
using System;

namespace UImGui
{
	internal sealed class ImGuizmoPlugin : IOptionalPlugin
	{
		public void Create(Context context)
		{
		}

		public void SetCurrent(Context context)
		{
			ImGuizmoNET.ImGuizmo.SetImGuiContext(context?.ImGuiContext ?? IntPtr.Zero);
		}

		public void Destroy(Context context)
		{
			ImGuizmoNET.ImGuizmo.SetImGuiContext(IntPtr.Zero);
		}
	}
}
#endif
