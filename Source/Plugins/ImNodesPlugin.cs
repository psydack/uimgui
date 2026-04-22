#if UIMGUI_ENABLE_IMNODES
using System;

namespace UImGui
{
	internal sealed class ImNodesPlugin : IOptionalPlugin
	{
		public void Create(Context context)
		{
			context.ImNodesContext = imnodesNET.imnodes.CreateContext();
		}

		public void SetCurrent(Context context)
		{
			imnodesNET.imnodes.SetImGuiContext(context?.ImGuiContext ?? IntPtr.Zero);
		}

		public void Destroy(Context context)
		{
			if (context?.ImNodesContext == IntPtr.Zero)
			{
				return;
			}

			imnodesNET.imnodes.DestroyContext(context.ImNodesContext);
			context.ImNodesContext = IntPtr.Zero;
		}
	}
}
#endif
