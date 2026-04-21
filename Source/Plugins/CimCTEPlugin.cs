#if UIMGUI_ENABLE_CIMCTE
using UnityEngine;

namespace UImGui
{
	internal sealed class CimCTEPlugin : IOptionalPlugin
	{
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		static void Register() => PluginRegistry.Register(new CimCTEPlugin());

		public void CreateContext(Context ctx) { }

		public void SetCurrentContext(Context ctx)
			=> CimCTENET.CimCTE.SetImGuiContext(ctx?.ImGuiContext ?? System.IntPtr.Zero);

		public void DestroyContext(Context ctx) { }
	}
}
#endif
