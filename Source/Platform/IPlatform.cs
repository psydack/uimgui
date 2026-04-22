using ImGuiNET;
using UnityEngine;

namespace UImGui.Platform
{
	/// <summary>
	/// Platform bindings for ImGui in Unity in charge of: mouse/keyboard/gamepad inputs, cursor shape, timing, windowing.
	/// </summary>
	public interface IPlatform
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="io"></param>
		/// <returns></returns>
		bool Initialize(ImGuiIOPtr io, UIOConfig config, string platformName);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="io"></param>
		void Shutdown(ImGuiIOPtr io);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="io"></param>
		/// <param name="displayRect"></param>
		void PrepareFrame(ImGuiIOPtr io, Rect displayRect);
	}
}
