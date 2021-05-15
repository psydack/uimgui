using ImGuiNET;
using UnityEngine.Rendering;

namespace UImGui.Renderer
{
	/// <summary>
	/// TODO: Write
	/// </summary>
	internal interface IRenderer
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="io"></param>
		void Initialize(ImGuiIOPtr io);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="io"></param>
		void Shutdown(ImGuiIOPtr io);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="commandBuffer"></param>
		/// <param name="drawData"></param>
		void RenderDrawLists(CommandBuffer commandBuffer, ImDrawDataPtr drawData);
	}
}