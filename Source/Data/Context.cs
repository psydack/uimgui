using System;
using UImGui.Texture;

namespace UImGui
{
	internal sealed class Context
	{
		public IntPtr ImGuiContext;
		public IntPtr ImNodesContext;
		public IntPtr ImPlotContext;
		public TextureManager TextureManager;
	}
}