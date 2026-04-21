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
			_uimguis = null;
		}

		protected override void Execute(CustomPassContext context)
		{
			if (!Application.isPlaying) return;

			if (_uimguis == null || _uimguis.Length == 0)
			{
				_uimguis = Object.FindObjectsByType<UImGui>(FindObjectsSortMode.None);
			}

			if (_uimguis == null) return;

			for (int uindex = 0; uindex < _uimguis.Length; uindex++)
			{
				var uimgui = _uimguis[uindex];

				if (!uimgui || !uimgui.enabled) continue;
				if (uimgui.Camera != context.hdCamera.camera) continue;

				uimgui.DoUpdate(context.cmd);
			}
		}

		protected override bool executeInSceneView => false;

		protected override void Cleanup()
		{
			_uimguis = null;
		}
	}
}
#endif
