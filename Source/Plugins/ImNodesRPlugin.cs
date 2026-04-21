#if UIMGUI_ENABLE_IMNODES_R
using System;
using UnityEngine;

namespace UImGui
{
	internal sealed class ImNodesRPlugin : IOptionalPlugin
	{
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		static void Register() => PluginRegistry.Register(new ImNodesRPlugin());

		public void CreateContext(Context ctx)
		{
			ImNodesRNET.ImNodesR.SetImGuiContext(ctx.ImGuiContext);
			ctx.ImNodesRContext = ImNodesRNET.ImNodesR.CreateContext();
			ImNodesRNET.ImNodesR.SetContext(ctx.ImNodesRContext);
		}

		public void SetCurrentContext(Context ctx)
		{
			ImNodesRNET.ImNodesR.SetImGuiContext(ctx?.ImGuiContext ?? IntPtr.Zero);
			ImNodesRNET.ImNodesR.SetContext(ctx?.ImNodesRContext ?? IntPtr.Zero);
		}

		public void DestroyContext(Context ctx)
		{
			if (ctx.ImNodesRContext != IntPtr.Zero)
			{
				ImNodesRNET.ImNodesR.SetContext(ctx.ImNodesRContext);
				ImNodesRNET.ImNodesR.FreeContext(ctx.ImNodesRContext);
				ctx.ImNodesRContext = IntPtr.Zero;
			}
			ImNodesRNET.ImNodesR.SetContext(IntPtr.Zero);
			ImNodesRNET.ImNodesR.SetImGuiContext(IntPtr.Zero);
		}
	}
}
#endif
