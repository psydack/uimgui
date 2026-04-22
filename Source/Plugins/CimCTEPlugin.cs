#if UIMGUI_ENABLE_CIMCTE
using System;

namespace UImGui
{
	internal sealed class CimCTEPlugin : IOptionalPlugin
	{
		public void Create(Context context)
		{
		}

		public void SetCurrent(Context context)
		{
			CimCTENET.CimCTE.SetImGuiContext(context?.ImGuiContext ?? IntPtr.Zero);
		}

		public void Destroy(Context context)
		{
			CimCTENET.CimCTE.SetImGuiContext(IntPtr.Zero);
		}
	}
}
#endif
