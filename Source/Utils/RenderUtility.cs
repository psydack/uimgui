using UImGui.Assets;
using UImGui.Renderer;
using UImGui.Texture;
using UnityEngine.Assertions;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace UImGui
{
	internal static class RenderUtility
	{
		public static IRenderer Create(RenderType type, ShaderResourcesAsset shaders, TextureManager textures)
		{
			Assert.IsNotNull(shaders, "Shaders not assigned.");

			switch (type)
			{
				case RenderType.Mesh:
					return new RendererMesh(shaders, textures);
				case RenderType.Procedural:
					return new RendererProcedural(shaders, textures);
				default:
					return null;
			}
		}

		public static bool IsUsingURP()
		{
			RenderPipelineAsset currentRP = GraphicsSettings.currentRenderPipeline;
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