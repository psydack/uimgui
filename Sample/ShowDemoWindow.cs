using ImGuiNET;
#if !UIMGUI_REMOVE_IMNODES
using imnodesNET;
#endif
#if !UIMGUI_REMOVE_IMPLOT
using ImPlotNET;
using System.Linq;
#endif
#if !UIMGUI_REMOVE_IMGUIZMO
using ImGuizmoNET;
#endif
using UnityEngine;

namespace UImGui
{
	public class ShowDemoWindow : MonoBehaviour
	{
#if !UIMGUI_REMOVE_IMPLOT
		[SerializeField]
		float[] _barValues = Enumerable.Range(1, 10).Select(x => (x * x) * 1.0f).ToArray();
		[SerializeField]
		float[] _xValues = Enumerable.Range(1, 10).Select(x => (x * x) * 1.0f).ToArray();
		[SerializeField]
		float[] _yValues = Enumerable.Range(1, 10).Select(x => (x * x) * 1.0f).ToArray();
#endif

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
#if !UIMGUI_REMOVE_IMPLOT
			if (ImGui.Begin("Plot Window Sample"))
			{
				ImGui.SetNextWindowSize(Vector2.one * 200, ImGuiCond.Once);
				ImPlot.BeginPlot("Plot test");
				ImPlot.PlotBars("My Bar Plot", ref _barValues[0], _barValues.Length + 1);
				ImPlot.PlotLine("My Line Plot", ref _xValues[0], ref _yValues[0], _xValues.Length, 0, 0);
				ImPlot.EndPlot();

				ImGui.End();
			}
#endif

#if !UIMGUI_REMOVE_IMNODES
			if (ImGui.Begin("Nodes Window Sample"))
			{
				ImGui.SetNextWindowSize(Vector2.one * 300, ImGuiCond.Once);
				imnodes.BeginNodeEditor();
				imnodes.BeginNode(1);

				imnodes.BeginNodeTitleBar();
				ImGui.TextUnformatted("simple node :)");
				imnodes.EndNodeTitleBar();

				imnodes.BeginInputAttribute(2);
				ImGui.Text("input");
				imnodes.EndInputAttribute();

				imnodes.BeginOutputAttribute(3);
				ImGui.Indent(40);
				ImGui.Text("output");
				imnodes.EndOutputAttribute();

				imnodes.EndNode();
				imnodes.EndNodeEditor();
				ImGui.End();
			}
#endif

			ImGui.ShowDemoWindow();
		}
	}
}

