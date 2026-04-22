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

			ImGui.Text("English:  Hello, World!");
			ImGui.Spacing();

			ImGui.TextDisabled("-- Japanese scripts --");
			// hiragana: konnichiwa, sekai (hello, world)
			ImGui.Text("Hiragana:  \u3053\u3093\u306b\u3061\u306f\u3001\u305b\u304b\u3044\uff01");
			// katakana: haro warudo (hello world)
			ImGui.Text("Katakana:  \u30cf\u30ed\u30fc\u30fb\u30ef\u30fc\u30eb\u30c9");
			// kanji: sekai (world), nihongo (Japanese), sakura (cherry blossom)
			ImGui.Text("Kanji:     \u4e16\u754c \u30fb \u65e5\u672c\u8a9e \u30fb \u685c");
			// mixed natural Japanese: sekai yo, konnichiwa
			ImGui.Text("Mixed:     \u4e16\u754c\u3088\u3001\u3053\u3093\u306b\u3061\u306f\uff01");
			ImGui.Spacing();

			ImGui.TextDisabled("-- Other scripts --");
			ImGui.Text("Cyrillic:  \u041f\u0440\u0438\u0432\u0435\u0442 \u043c\u0438\u0440");
			ImGui.End();
		}
	}
}
