#if UIMGUI_ENABLE_IMPLOT
using System;

namespace UImGui
{
	internal sealed class ImPlotPlugin : IOptionalPlugin
	{
		public void Create(Context context)
		{
			context.ImPlotContext = ImPlotNET.ImPlot.CreateContext();
		}

		public void SetCurrent(Context context)
		{
			ImPlotNET.ImPlot.SetImGuiContext(context?.ImGuiContext ?? IntPtr.Zero);
		}

		public void Destroy(Context context)
		{
			if (context?.ImPlotContext == IntPtr.Zero)
			{
				return;
			}

			ImPlotNET.ImPlot.DestroyContext(context.ImPlotContext);
			context.ImPlotContext = IntPtr.Zero;
		}
	}
}
#endif
