using System;
using UImGui.Texture;

namespace UImGui
{
	public sealed class Context
	{
		public IntPtr ImGuiContext;
		public IntPtr ImPlotContext;
		public IntPtr ImPlot3DContext;
		public IntPtr ImNodesContext;
		public IntPtr ImNodesRContext;
		public TextureManager TextureManager;
	}
}