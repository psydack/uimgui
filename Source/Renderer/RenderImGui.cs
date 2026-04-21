using UnityEngine.Rendering;
#if HAS_URP
using UnityEngine.Rendering.Universal;
using UnityEngine;
#endif
#if HAS_URP_17 && HAS_URP
using UnityEngine.Rendering.RenderGraphModule;
#endif

namespace UImGui.Renderer
{
#if HAS_URP
	[CreateAssetMenu(menuName = "Dear ImGui/Render ImGui")]
	public class RenderImGui : ScriptableRendererFeature
	{
		private class CommandBufferPass : ScriptableRenderPass
		{
			public CommandBuffer commandBuffer;
			public global::UImGui.UImGui uImGui;

#if HAS_URP_17
			public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
			{
				if (uImGui == null) return;

				using var builder = renderGraph.AddUnsafePass<PassData>("UImGui CommandBuffer Pass", out var passData);
				var resourceData = frameData.Get<UniversalResourceData>();
				passData.ColorTarget = resourceData.activeColorTexture;
				passData.UImGui = uImGui;
				builder.UseTexture(passData.ColorTarget, AccessFlags.Write);
				builder.AllowPassCulling(false);
				builder.SetRenderFunc((PassData data, UnsafeGraphContext ctx) =>
				{
					ctx.cmd.SetRenderTarget(data.ColorTarget);
					var nativeCommandBuffer = CommandBufferHelpers.GetNativeCommandBuffer(ctx.cmd);
					data.UImGui.RenderDrawData(nativeCommandBuffer);
				});
			}

			private class PassData
			{
				public TextureHandle ColorTarget;
				public global::UImGui.UImGui UImGui;
			}
#else
			public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
			{
				context.ExecuteCommandBuffer(commandBuffer);
			}
#endif
		}

		[HideInInspector]
		public Camera Camera;
		[HideInInspector]
		public global::UImGui.UImGui UImGui;
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
			_commandBufferPass.uImGui = UImGui;

			renderer.EnqueuePass(_commandBufferPass);
		}

	}
#else
	[UnityEngine.CreateAssetMenu(menuName = "Dear ImGui/Render ImGui")]
	public class RenderImGui : UnityEngine.ScriptableObject
	{
		public CommandBuffer CommandBuffer;
	}
#endif
}
