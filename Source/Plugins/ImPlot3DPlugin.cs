#if UIMGUI_ENABLE_IMPLOT3D
using System;

namespace UImGui
{
	internal sealed class ImPlot3DPlugin : IOptionalPlugin
	{
		public void Create(Context context)
		{
			context.ImPlot3DContext = ImPlot3DNET.ImPlot3D.CreateContext();
		}

		public void SetCurrent(Context context)
		{
			ImPlot3DNET.ImPlot3D.SetImGuiContext(context?.ImGuiContext ?? IntPtr.Zero);
		}

		public void Destroy(Context context)
		{
			if (context?.ImPlot3DContext == IntPtr.Zero)
			{
				return;
			}

			ImPlot3DNET.ImPlot3D.DestroyContext(context.ImPlot3DContext);
			context.ImPlot3DContext = IntPtr.Zero;
		}
	}
}
#endif
