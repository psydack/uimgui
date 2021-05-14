using Unity.Profiling;

namespace UImGui
{
	public static class Constants
	{
		public static readonly ProfilerMarker PrepareFrameMarker = new ProfilerMarker("DearImGui.PrepareFrame");
		public static readonly ProfilerMarker LayoutfMarker = new ProfilerMarker("DearImGui.Layout");
		public static readonly ProfilerMarker DrawListMarker = new ProfilerMarker("DearImGui.RenderDrawLists");

		public static readonly uint Version = (0 << 16) | (0 << 8) | (5);
	}
}