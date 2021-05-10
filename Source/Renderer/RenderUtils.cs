using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace UImGui.Renderer
{
	internal static partial class RenderUtils
	{
		public static bool IsUsingURP()
		{
			var currentRP = GraphicsSettings.currentRenderPipeline;
#if HAS_URP
			return currentRP is UniversalRenderPipelineAsset;
#else
			return false;
#endif
		}

		public static CommandBuffer GetCommandBuffer(string name)
		{
#if HAS_URP
			return CommandBufferPool.Get(name);
#else
            return new CommandBuffer { name = name };
#endif
		}

		public static void ReleaseCommandBuffer(CommandBuffer commandBuffer)
		{
#if HAS_URP
			CommandBufferPool.Release(commandBuffer);
#else
			commandBuffer.Release();
#endif
		}
	}
}