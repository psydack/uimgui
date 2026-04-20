using System;
using UImGui.Texture;

namespace UImGui
{
	public sealed class Context
	{
		public IntPtr ImGuiContext;
		public IntPtr ImNodesContext;
		public IntPtr ImPlotContext;
		public TextureManager TextureManager;
	}
}