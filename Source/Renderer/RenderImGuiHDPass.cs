#if HAS_HDRP
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace UImGui.Renderer
{
	public class RenderImGuiPass : CustomPass
	{
		private UImGui[] _uimguis;

		protected override void Setup(ScriptableRenderContext renderContext, CommandBuffer cmd)
		{
			_uimguis = Object.FindObjectsByType<UImGui>(FindObjectsSortMode.None);
		}

		protected override void Execute(CustomPassContext context)
		{
			if (!Application.isPlaying) return;
			if (_uimguis == null) return;

			for (int uindex = 0; uindex < _uimguis.Length; uindex++)
			{
				var uimgui = _uimguis[uindex];

				if (!uimgui || !uimgui.enabled) continue;

				uimgui.DoUpdate(context.cmd);

#if UNITY_EDITOR
				// Only draw gizmos when NOT in Game view to avoid leaking into final output (issues #67, #54)
				if (context.hdCamera.camera.cameraType != CameraType.Game)
				{
					context.renderContext.DrawGizmos(context.hdCamera.camera, GizmoSubset.PostImageEffects);
				}
#endif
			}
		}

		protected override bool executeInSceneView => false;

		protected override void Cleanup() { }
	}
}
#endif