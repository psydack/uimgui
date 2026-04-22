#if UIMGUI_ENABLE_IMNODES_R
using System;

namespace UImGui
{
	internal sealed class ImNodesRPlugin : IOptionalPlugin
	{
		public void Create(Context context)
		{
			ImNodesRNET.ImNodesR.SetImGuiContext(context.ImGuiContext);
			context.ImNodesRContext = ImNodesRNET.ImNodesR.CreateContext();
			ImNodesRNET.ImNodesR.SetContext(context.ImNodesRContext);
		}

		public void SetCurrent(Context context)
		{
			ImNodesRNET.ImNodesR.SetImGuiContext(context?.ImGuiContext ?? IntPtr.Zero);
			ImNodesRNET.ImNodesR.SetContext(context?.ImNodesRContext ?? IntPtr.Zero);
		}

		public void Destroy(Context context)
		{
			if (context?.ImNodesRContext != IntPtr.Zero)
			{
				ImNodesRNET.ImNodesR.SetContext(context.ImNodesRContext);
				ImNodesRNET.ImNodesR.FreeContext(context.ImNodesRContext);
				context.ImNodesRContext = IntPtr.Zero;
			}

			ImNodesRNET.ImNodesR.SetContext(IntPtr.Zero);
			ImNodesRNET.ImNodesR.SetImGuiContext(IntPtr.Zero);
		}
	}
}
#endif
