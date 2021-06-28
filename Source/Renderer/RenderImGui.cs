using UnityEngine.Rendering;
#if HAS_URP
using UnityEngine.Rendering.Universal;
using UnityEngine;
#endif

namespace UImGui.Renderer
{
#if HAS_URP
	public class RenderImGui : ScriptableRendererFeature
	{
		private class CommandBufferPass : ScriptableRenderPass
		{
			public CommandBuffer commandBuffer;

			public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
			{
				context.ExecuteCommandBuffer(commandBuffer);
			}
		}

		[HideInInspector]
		public Camera Camera;
		public CommandBuffer CommandBuffer;
		public RenderPassEvent RenderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;

		private CommandBufferPass _commandBufferPass;

		public override void Create()
		{
			_commandBufferPass = new CommandBufferPass()
			{
				commandBuffer = CommandBuffer,
				renderPassEvent = RenderPassEvent,
			};
		}

		public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
		{
			if (CommandBuffer == null) return;
			if (Camera != renderingData.cameraData.camera) return;

			_commandBufferPass.renderPassEvent = RenderPassEvent;
			_commandBufferPass.commandBuffer = CommandBuffer;

			renderer.EnqueuePass(_commandBufferPass);
		}
	}
#else
	public class RenderImGui : UnityEngine.ScriptableObject
	{
		public CommandBuffer CommandBuffer;
	}
#endif
}
