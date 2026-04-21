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
		System.Numerics.Vector2 _nodePos = new System.Numerics.Vector2(50, 50);
		bool _nodeSelected;
#endif

#if UIMGUI_ENABLE_IMGUIZMO
		private readonly float[] _gizmoView = new float[16];
		private readonly float[] _gizmoProjection = new float[16];
		private readonly float[] _gizmoMatrix = new float[16];
#endif

#if UIMGUI_ENABLE_IMGUIZMO_QUAT
		// x,y,z,w quaternion — identity
		System.Numerics.Vector4 _rotation = new System.Numerics.Vector4(0, 0, 0, 1);
#endif

#if UIMGUI_ENABLE_IMGUIZMO
		private void Awake()
		{
			InitGizmoMatrices();
		}
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

#if UIMGUI_ENABLE_IMGUIZMO
			if (ImGui.Begin("ImGuizmo Demo"))
			{
				ImGuizmo.BeginFrame();
				var pos = ImGui.GetWindowPos();
				var size = ImGui.GetWindowSize();
				ImGuizmo.SetRect(pos.X, pos.Y, size.X, size.Y);
				ImGuizmo.Manipulate(ref _gizmoView[0], ref _gizmoProjection[0], OPERATION.TRANSLATE, MODE.LOCAL, ref _gizmoMatrix[0]);
				ImGuizmo.DrawGrid(ref _gizmoView[0], ref _gizmoProjection[0], ref _gizmoMatrix[0], 10f);
				ImGui.End();
			}
#endif

#if UIMGUI_ENABLE_IMNODES_R
			if (ImGui.Begin("Nodes R Sample"))
			{
				ImNodesR.SetContext(UImGuiUtility.Context.ImNodesRContext);
				ImNodesR.BeginCanvas();
				if (ImNodesR.BeginNode(new System.IntPtr(1), "Node R", ref _nodePos, ref _nodeSelected))
				{
					ImGui.TextUnformatted("cimnodes_r smoke node");
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

#if UIMGUI_ENABLE_IMGUIZMO
		private void InitGizmoMatrices()
		{
			_gizmoMatrix[0] = 1f;
			_gizmoMatrix[5] = 1f;
			_gizmoMatrix[10] = 1f;
			_gizmoMatrix[15] = 1f;

			var eye = new System.Numerics.Vector3(2f, 2f, 5f);
			var center = System.Numerics.Vector3.Zero;
			var up = System.Numerics.Vector3.UnitY;
			var z = System.Numerics.Vector3.Normalize(eye - center);
			var x = System.Numerics.Vector3.Normalize(System.Numerics.Vector3.Cross(up, z));
			var y = System.Numerics.Vector3.Cross(z, x);

			_gizmoView[0] = x.X;
			_gizmoView[1] = y.X;
			_gizmoView[2] = z.X;
			_gizmoView[3] = 0f;
			_gizmoView[4] = x.Y;
			_gizmoView[5] = y.Y;
			_gizmoView[6] = z.Y;
			_gizmoView[7] = 0f;
			_gizmoView[8] = x.Z;
			_gizmoView[9] = y.Z;
			_gizmoView[10] = z.Z;
			_gizmoView[11] = 0f;
			_gizmoView[12] = -System.Numerics.Vector3.Dot(x, eye);
			_gizmoView[13] = -System.Numerics.Vector3.Dot(y, eye);
			_gizmoView[14] = -System.Numerics.Vector3.Dot(z, eye);
			_gizmoView[15] = 1f;

			float fovY = (float)(System.Math.PI / 4.0);
			float aspect = 1280f / 720f;
			float near = 0.1f;
			float far = 100f;
			float f = 1f / (float)System.Math.Tan(fovY / 2f);
			_gizmoProjection[0] = f / aspect;
			_gizmoProjection[5] = f;
			_gizmoProjection[10] = (far + near) / (near - far);
			_gizmoProjection[11] = -1f;
			_gizmoProjection[14] = (2f * far * near) / (near - far);
		}
#endif
	}
}
