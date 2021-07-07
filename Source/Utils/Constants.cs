using Unity.Profiling;

namespace UImGui
{
	internal static class Constants
	{
		public static readonly uint Version = (3 << 16) | (2 << 8) | (4);

		internal static readonly string UImGuiCommandBuffer = "UImGui";

		// TODO: Test all profile markers.
		internal static readonly ProfilerMarker PrepareFrameMarker = new ProfilerMarker("UImGui.PrepareFrame");
		internal static readonly ProfilerMarker LayoutMarker = new ProfilerMarker("UImGui.Layout");
		internal static readonly ProfilerMarker DrawListMarker = new ProfilerMarker("UImGui.RenderDrawLists");

		internal static readonly ProfilerMarker UpdateMeshMarker = new ProfilerMarker("UImGui.RendererMesh.UpdateMesh");
		internal static readonly ProfilerMarker CreateDrawCommandsMarker = new ProfilerMarker("UImGui.RendererMesh.CreateDrawCommands");

		internal static readonly ProfilerMarker UpdateBuffersMarker = new ProfilerMarker("UImGui.RendererProcedural.UpdateBuffers");
		internal static readonly ProfilerMarker CreateDrawComandsMarker = new ProfilerMarker("UImGui.RendererProcedural.CreateDrawCommands");

		internal static readonly string ExecuteDrawCommandsMarker = "UImGui.ExecuteDrawCommands";
	}
}