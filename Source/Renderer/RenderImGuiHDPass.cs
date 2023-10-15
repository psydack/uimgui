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
			for (int uindex = 0; uindex < _uimguis.Length; uindex++)
			{
				UImGui uimgui = _uimguis[uindex];
				CommandBuffer cb = uimgui.CommandBuffer;

				if (cb == null) continue;

				context.renderContext.ExecuteCommandBuffer(cb);
				cb.Clear();
			}
		}

        protected override bool executeInSceneView => false;

		protected override void Cleanup() { }
	}
}
#endif