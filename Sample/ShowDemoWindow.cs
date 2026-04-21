using ImGuiNET;
#if UIMGUI_ENABLE_IMNODES
using imnodesNET;
#endif
#if UIMGUI_ENABLE_IMPLOT
using ImPlotNET;
using System.Linq;
#endif
#if UIMGUI_ENABLE_IMGUIZMO
using ImGuizmoNET;
#endif
#if UIMGUI_ENABLE_IMPLOT3D
using ImPlot3DNET;
#endif
#if UIMGUI_ENABLE_IMNODES_R
using ImNodesRNET;
using System;
#endif
using UnityEngine;

namespace UImGui
{
	public class ShowDemoWindow : MonoBehaviour
	{
#if UIMGUI_ENABLE_IMPLOT
		[SerializeField]
		float[] _barValues = Enumerable.Range(1, 10).Select(x => (x * x) * 1.0f).ToArray();
		[SerializeField]
		float[] _xValues = Enumerable.Range(1, 10).Select(x => (x * x) * 1.0f).ToArray();
		[SerializeField]
		float[] _yValues = Enumerable.Range(1, 10).Select(x => (x * x) * 1.0f).ToArray();
#endif

#if UIMGUI_ENABLE_IMPLOT3D
		float[] _xs3D = { 0, 1, 2, 3, 4 };
		float[] _ys3D = { 0, 1, 0, 1, 0 };
		float[] _zs3D = { 0, 0, 1, 1, 2 };
#endif

#if UIMGUI_ENABLE_IMNODES_R
		CanvasState _canvas;
		System.Numerics.Vector2 _nodePos = new System.Numerics.Vector2(50, 50);
		bool _nodeSelected;
#endif

#if UIMGUI_ENABLE_IMGUIZMO_QUAT
		// x,y,z,w quaternion — identity
		System.Numerics.Vector4 _rotation = new System.Numerics.Vector4(0, 0, 0, 1);
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
#if UIMGUI_ENABLE_IMPLOT
			if (ImGui.Begin("Plot Window Sample"))
			{
				var initialWindowSize = UnityEngine.Vector2.one * 200;
				ImGui.SetNextWindowSize(initialWindowSize.AsNumerics(), ImGuiCond.Once);
				ImPlot.BeginPlot("Plot test");
				ImPlot.PlotBars("My Bar Plot", ref _barValues[0], _barValues.Length + 1);
				ImPlot.PlotLine("My Line Plot", ref _yValues[0], _yValues.Length);
				ImPlot.EndPlot();

				ImGui.End();
			}
#endif

#if UIMGUI_ENABLE_IMNODES
			if (ImGui.Begin("Nodes Window Sample"))
			{
				var initialWindowSize = UnityEngine.Vector2.one * 300;
				ImGui.SetNextWindowSize(initialWindowSize.AsNumerics(), ImGuiCond.Once);
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

#if UIMGUI_ENABLE_IMPLOT3D
			if (ImGui.Begin("3D Plot Sample"))
			{
				if (ImPlot3D.BeginPlot("3D Line"))
				{
					ImPlot3D.PlotLine("Helix", ref _xs3D[0], ref _ys3D[0], ref _zs3D[0], _xs3D.Length);
					ImPlot3D.EndPlot();
				}
				ImGui.End();
			}
#endif

#if UIMGUI_ENABLE_IMNODES_R
			// CanvasState._Impl is initialized lazily by the library on first BeginCanvas.
			if (ImGui.Begin("Nodes R Sample"))
			{
				ImNodesR.BeginCanvas(ref _canvas);
				var nodeId = new IntPtr(1);
				if (ImNodesR.BeginNode(nodeId, "My Node", ref _nodePos, ref _nodeSelected))
				{
					if (ImNodesR.BeginInputSlot("Input", 1))
					{
						ImGui.Text("in");
						ImNodesR.EndSlot();
					}
					if (ImNodesR.BeginOutputSlot("Output", 1))
					{
						ImGui.Indent(40);
						ImGui.Text("out");
						ImNodesR.EndSlot();
					}
					ImNodesR.EndNode();
				}
				ImNodesR.EndCanvas();
				ImGui.End();
			}
#endif

#if UIMGUI_ENABLE_IMGUIZMO_QUAT
			if (ImGui.Begin("Gizmo Quat Sample"))
			{
				ImGuizmoQuatNET.ImGuizmoQuat.gizmo3D("##rot", ref _rotation);
				ImGui.Text($"Q  {_rotation.X:F2}  {_rotation.Y:F2}  {_rotation.Z:F2}  {_rotation.W:F2}");
				ImGui.End();
			}
#endif

#if UIMGUI_ENABLE_CIMCTE
			// TextEditor is a C++ class wrapped via TextEditorPtr. Allocate it with
			// Marshal.AllocHGlobal + placement-new (no factory in current bindings).
			if (ImGui.Begin("CimCTE Sample"))
			{
				ImGui.Text("CimCTE available — use CimCTENET.TextEditorPtr for the full API.");
				ImGui.End();
			}
#endif

			ImGui.ShowDemoWindow();
		}
	}
}
