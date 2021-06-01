using ImGuiNET;
using UnityEngine;

namespace UImGui
{
	public class ShowDemoWindow : MonoBehaviour
	{
		private void OnEnable()
		{
			UImGuiUtility.Layout += OnLayout;
		}

		private void OnDisable()
		{
			UImGuiUtility.Layout -= OnLayout;
		}

		private void OnLayout(UImGui uImGui)
		{
			ImGui.ShowDemoWindow();
		}
	}
}