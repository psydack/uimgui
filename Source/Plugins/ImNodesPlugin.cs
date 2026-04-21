#if UIMGUI_ENABLE_IMNODES
using UnityEngine;

namespace UImGui
{
	internal sealed class ImNodesPlugin : IOptionalPlugin
	{
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		static void Register() => PluginRegistry.Register(new ImNodesPlugin());

		public void CreateContext(Context ctx)
			=> ctx.ImNodesContext = imnodesNET.imnodes.CreateContext();

		public void SetCurrentContext(Context ctx)
			=> imnodesNET.imnodes.SetImGuiContext(ctx?.ImGuiContext ?? System.IntPtr.Zero);

		public void DestroyContext(Context ctx)
			=> imnodesNET.imnodes.DestroyContext(ctx.ImNodesContext);
	}
}
#endif
