#if HAS_HDRP
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace UImGui.Renderer
{
	public class RenderImGuiPass : CustomPass
	{
		private UImGui[] _uimguis;

		protected override void Setup(ScriptableRenderContext renderContext, CommandBuffer cmd)
		{
			_uimguis = UnityEngine.Object.FindObjectsOfType<UImGui>();
		}

		protected override void Execute(CustomPassContext context)
		{
			for (int uindex = 0; uindex < _uimguis.Length; uindex++)
			{
				UImGui uimgui = _uimguis[uindex];
				CommandBuffer cb = uimgui.CommandBuffer;

				if (cb == null) return;

				CoreUtils.SetRenderTarget(cb, context.cameraColorBuffer);
				context.renderContext.ExecuteCommandBuffer(cb);
				context.renderContext.Submit();
			}
		}

		protected override void Cleanup() { }
	}
}
#endif