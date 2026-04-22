using UImGui.Assets;
using UImGui.Renderer;
using UImGui.Texture;
using UnityEngine.Assertions;
using UnityEngine.Rendering;
#if HAS_URP
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.Rendering.Universal;
#endif
#if HAS_HDRP
using UnityEngine.Rendering.HighDefinition;
#endif

namespace UImGui
{
	internal static class RenderUtility
	{
		public static IRenderer Create(RenderType type, ShaderResourcesAsset shaders, TextureManager textures)
		{
			Assert.IsNotNull(shaders, "Shaders not assigned.");

#if UNITY_WEBGL
			// SV_VertexID is not supported on WebGL/GLES 2.0 — force Mesh renderer.
			type = RenderType.Mesh;
#endif

			switch (type)
			{
#if UNITY_2020_1_OR_NEWER
				case RenderType.Mesh:
					return new RendererMesh(shaders, textures);
#endif
				case RenderType.Procedural:
					return new RendererProcedural(shaders, textures);
				default:
					return null;
			}
		}

		public static bool IsUsingURP()
		{
			var currentRP = GraphicsSettings.currentRenderPipeline;
#if HAS_URP
			return currentRP is UniversalRenderPipelineAsset;
#else
			return false;
#endif
		}

#if HAS_URP
		public static RenderImGui FindRenderFeatureInCurrentPipeline()
		{
			if (GraphicsSettings.currentRenderPipeline is not UniversalRenderPipelineAsset pipeline)
				return null;

			var rendererDataListField = typeof(UniversalRenderPipelineAsset).GetField(
				"m_RendererDataList",
				BindingFlags.Instance | BindingFlags.NonPublic);
			if (rendererDataListField?.GetValue(pipeline) is not ScriptableRendererData[] rendererDataList)
				return null;

			var rendererFeaturesField = typeof(ScriptableRendererData).GetField(
				"m_RendererFeatures",
				BindingFlags.Instance | BindingFlags.NonPublic);
			if (rendererFeaturesField == null)
				return null;

			foreach (var rendererData in rendererDataList)
			{
				if (rendererData == null)
					continue;

				if (rendererFeaturesField.GetValue(rendererData) is not List<ScriptableRendererFeature> rendererFeatures)
					continue;

				foreach (var feature in rendererFeatures)
				{
					if (feature is RenderImGui renderImGui)
						return renderImGui;
				}
			}

			return null;
		}
#endif

		public static bool IsUsingHDRP()
		{
			var currentRP = GraphicsSettings.currentRenderPipeline;

#if HAS_HDRP
			return currentRP is HDRenderPipelineAsset;
#else
			return false;
#endif
		}

		public static CommandBuffer GetCommandBuffer(string name)
		{
#if HAS_URP || HAS_HDRP
			return CommandBufferPool.Get(name);
#else
			return new CommandBuffer { name = name };
#endif
		}

		public static void ReleaseCommandBuffer(CommandBuffer commandBuffer)
		{
#if HAS_URP || HAS_HDRP
			CommandBufferPool.Release(commandBuffer);
#else
			commandBuffer.Release();
#endif
		}
	}
}
