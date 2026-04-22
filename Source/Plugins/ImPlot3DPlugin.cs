#if UIMGUI_ENABLE_IMPLOT3D
using UnityEngine;

namespace UImGui
{
	internal sealed class ImPlot3DPlugin : IOptionalPlugin
	{
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		static void Register() => PluginRegistry.Register(new ImPlot3DPlugin());

		public void CreateContext(Context ctx)
			=> ctx.ImPlot3DContext = ImPlot3DNET.ImPlot3D.CreateContext();

		public void SetCurrentContext(Context ctx)
			=> ImPlot3DNET.ImPlot3D.SetImGuiContext(ctx?.ImGuiContext ?? System.IntPtr.Zero);

		public void DestroyContext(Context ctx)
			=> ImPlot3DNET.ImPlot3D.DestroyContext(ctx.ImPlot3DContext);
	}
}
#endif
