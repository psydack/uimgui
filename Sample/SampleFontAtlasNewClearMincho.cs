using ImGuiNET;
using UnityEngine;

namespace UImGui
{
	public sealed class SampleFontAtlasNewClearMincho : MonoBehaviour
	{
		[SerializeField]
		private bool _showWindow = true;

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
			if (!_showWindow)
			{
				return;
			}

			if (!ImGui.Begin("NewClear Mincho Font Atlas Sample", ref _showWindow))
			{
				ImGui.End();
				return;
			}

			ImGui.Text("NewClear-mincho.ttf");
			ImGui.Text("English: Hello");
			ImGui.Text("Japanese: \u3053\u3093\u306B\u3061\u306F");
			ImGui.Text("Chinese: \u4E2D\u6587");
			ImGui.Text("Korean: \uC548\uB155\uD558\uC138\uC694");
			ImGui.Text("Thai: \u0E44\u0E17\u0E22");
			ImGui.End();
		}
	}
}
