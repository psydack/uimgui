#if UIMGUI_ENABLE_IMPLOT
using UnityEngine;

namespace UImGui
{
	internal sealed class ImPlotPlugin : IOptionalPlugin
	{
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		static void Register() => PluginRegistry.Register(new ImPlotPlugin());

		public void CreateContext(Context ctx)
			=> ctx.ImPlotContext = ImPlotNET.ImPlot.CreateContext();

		public void SetCurrentContext(Context ctx)
			=> ImPlotNET.ImPlot.SetImGuiContext(ctx?.ImGuiContext ?? System.IntPtr.Zero);

		public void DestroyContext(Context ctx)
			=> ImPlotNET.ImPlot.DestroyContext(ctx.ImPlotContext);
	}
}
#endif
