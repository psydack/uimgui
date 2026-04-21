using ImGuiNET;
using UnityEngine;

namespace UImGui
{
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

			string fontPath = System.IO.Path.Combine(
				Application.streamingAssetsPath, "NewClear-mincho.ttf");
			bool fontReady = System.IO.File.Exists(fontPath);

			if (!fontReady)
			{
				ImGui.TextColored(new System.Numerics.Vector4(1f, 0.4f, 0.1f, 1f), "Font NOT in StreamingAssets!");
				ImGui.TextWrapped("Copy NewClear-mincho.ttf from the package Resources/ folder to Assets/StreamingAssets/.");
				ImGui.End();
				return;
			}

			ImGui.TextColored(new System.Numerics.Vector4(0.2f, 1f, 0.2f, 1f), "Font loaded.");
			ImGui.Separator();
			ImGui.Text("English: Hello world");
			ImGui.Text("Japanese: \u3053\u3093\u306b\u3061\u306f");
			ImGui.Text("Cyrillic: \u041f\u0440\u0438\u0432\u0435\u0442");
			ImGui.End();
		}
	}
}
