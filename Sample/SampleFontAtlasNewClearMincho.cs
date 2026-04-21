using ImGuiNET;
using UnityEngine;

namespace UImGui
{
	/// <summary>
	/// Minimal sample panel for validating the NewClear-mincho font atlas flow.
	/// This sample does not load fonts directly; it documents the expected setup.
	/// </summary>
	public sealed class SampleFontAtlasNewClearMincho : MonoBehaviour
	{
		[SerializeField]
		private bool _isOpen = true;

		private void OnEnable()
		{
			UImGuiUtility.Layout += OnLayout;
		}

		private void OnDisable()
		{
			UImGuiUtility.Layout -= OnLayout;
		}

		private void OnLayout(UImGui uimgui)
		{
			if (!ImGui.Begin("Sample Font Atlas NewClear-mincho", ref _isOpen))
			{
				ImGui.End();
				return;
			}

			ImGui.Text("Font Atlas sample status: WIP");
			ImGui.Text("Example font file: NewClear-mincho.ttf");
			ImGui.Text("Expected path: Assets/StreamingAssets/NewClear-mincho.ttf");
			ImGui.Separator();
			ImGui.Text("Validation strings:");
			ImGui.Text("English: Hello world");
			ImGui.Text("Japanese: こんにちは");
			ImGui.Text("Cyrillic: Привет");
			ImGui.End();
		}
	}
}
