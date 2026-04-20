using UnityEngine.Rendering;
#if HAS_URP
using UnityEngine.Rendering.Universal;
using UnityEngine;
#endif
#if UNITY_6_0_OR_NEWER && HAS_URP
using UnityEngine.Rendering.RenderGraphModule;
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
		public RenderPassEvent RenderPassEvent = RenderPassEvent.AfterRenderingTransparents;

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

#if UNITY_6_0_OR_NEWER
		public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
		{
			if (CommandBuffer == null) return;
			var cameraData = frameData.Get<UniversalCameraData>();
			if (Camera != cameraData.camera) return;

			using var builder = renderGraph.AddUnsafePass<PassData>("UImGui CommandBuffer Pass", out var passData);
			passData.CommandBuffer = CommandBuffer;
			builder.AllowPassCulling(false);
			builder.SetRenderFunc((PassData data, UnsafeGraphContext ctx) =>
			{
				ctx.cmd.ExecuteCommandBuffer(data.CommandBuffer);
			});
		}

		private class PassData
		{
			public CommandBuffer CommandBuffer;
		}
#endif
	}
#else
	public class RenderImGui : UnityEngine.ScriptableObject
	{
		public CommandBuffer CommandBuffer;
	}
#endif
}
