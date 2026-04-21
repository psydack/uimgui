#if UNITY_2020_1_OR_NEWER
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UImGui.Assets;
using UImGui.Texture;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;
using Num = System.Numerics;

namespace UImGui.Renderer
{
	internal sealed class RendererMesh : IRenderer
	{
		// Skip all checks and validation when updating the mesh.
		private const MeshUpdateFlags NoMeshChecks = MeshUpdateFlags.DontNotifyMeshUsers |
			MeshUpdateFlags.DontRecalculateBounds |
			MeshUpdateFlags.DontResetBoneBounds |
			MeshUpdateFlags.DontValidateIndices;

		// Color sent with TexCoord1 semantics because otherwise Color attribute would be reordered to come before UVs.
		private static readonly VertexAttributeDescriptor[] _vertexAttributes = new[]
		{
			new VertexAttributeDescriptor(VertexAttribute.Position , VertexAttributeFormat.Float32, 2), // Position.
			new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2), // UV.
			new VertexAttributeDescriptor(VertexAttribute.TexCoord1, VertexAttributeFormat.UInt32 , 1), // Color.
        };

		private Material _material;
		private Mesh _mesh;

		private readonly Shader _shader;
		private readonly int _textureID;
		private readonly TextureManager _textureManager;
		private readonly MaterialPropertyBlock _materialProperties;

		private int _prevSubMeshCount = 1;  // number of sub meshes used previously

		public RendererMesh(ShaderResourcesAsset resources, TextureManager texManager)
		{
			_shader = resources.Shader.Mesh;
			_textureManager = texManager;
			_textureID = Shader.PropertyToID(resources.PropertyNames.Texture);
			_materialProperties = new MaterialPropertyBlock();
		}

		public void Initialize(ImGuiIOPtr io)
		{
			io.SetBackendRendererName("Unity Mesh");
			// Supports large meshes and the explicit texture backend expected by current ImGui.NET/cimgui.
			io.BackendFlags |= ImGuiBackendFlags.RendererHasVtxOffset | ImGuiBackendFlags.RendererHasTextures;

			_material = new Material(_shader)
			{
				hideFlags = HideFlags.HideAndDontSave & ~HideFlags.DontUnloadUnusedAsset
			};

			_mesh = new Mesh
			{
				name = "DearImGui Mesh"
			};
			_mesh.MarkDynamic();
		}

		public void Shutdown(ImGuiIOPtr io)
		{
			io.SetBackendRendererName(null);

			if (_mesh != null)
			{
				Object.Destroy(_mesh);
				_mesh = null;
			}

			if (_material != null)
			{
				Object.Destroy(_material);
				_material = null;
			}
		}

		public void RenderDrawLists(CommandBuffer commandBuffer, ImDrawDataPtr drawData)
		{
			var framebufferOutputSize = (drawData.DisplaySize * drawData.FramebufferScale).ToUnity();

			// Avoid rendering when minimized.
			if (framebufferOutputSize.x <= 0f || framebufferOutputSize.y <= 0f || drawData.TotalVtxCount == 0) return;

			Constants.UpdateMeshMarker.Begin();
			UpdateMesh(drawData);
			Constants.UpdateMeshMarker.End();

			commandBuffer.BeginSample(Constants.ExecuteDrawCommandsMarker);
			Constants.CreateDrawCommandsMarker.Begin();

			CreateDrawCommands(commandBuffer, drawData, framebufferOutputSize);

			Constants.CreateDrawCommandsMarker.End();
			commandBuffer.EndSample(Constants.ExecuteDrawCommandsMarker);
		}

		private void UpdateMesh(ImDrawDataPtr drawData)
		{
			// Number of submeshes is the same as the nr of ImDrawCmd.
			int subMeshCount = 0;
			for (int n = 0, nMax = drawData.CmdListsCount; n < nMax; ++n)
			{
				subMeshCount += drawData.CmdLists[n].CmdBuffer.Size;
			}

			if (_prevSubMeshCount != subMeshCount)
			{
				// Occasionally crashes when changing subMeshCount without clearing first.
				_mesh.Clear(true);
				_mesh.subMeshCount = _prevSubMeshCount = subMeshCount;
			}
			_mesh.SetVertexBufferParams(drawData.TotalVtxCount, _vertexAttributes);
			_mesh.SetIndexBufferParams(drawData.TotalIdxCount, IndexFormat.UInt16);

			//  Upload data into mesh.
			int vertexOffset = 0;
			int indexOffset = 0;
			var descriptors = new List<SubMeshDescriptor>();

			for (int n = 0, nMax = drawData.CmdListsCount; n < nMax; ++n)
			{
				var drawList = drawData.CmdLists[n];

				unsafe
				{
					var vtxArray = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<ImDrawVert>(
						(void*)drawList.VtxBuffer.Data, drawList.VtxBuffer.Size, Allocator.None);
					var idxArray = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<ushort>(
						(void*)drawList.IdxBuffer.Data, drawList.IdxBuffer.Size, Allocator.None);

#if ENABLE_UNITY_COLLECTIONS_CHECKS
					NativeArrayUnsafeUtility
						.SetAtomicSafetyHandle(ref vtxArray, AtomicSafetyHandle.GetTempMemoryHandle());
					NativeArrayUnsafeUtility
						.SetAtomicSafetyHandle(ref idxArray, AtomicSafetyHandle.GetTempMemoryHandle());
#endif
					// Upload vertex/index data.
					_mesh.SetVertexBufferData(vtxArray, 0, vertexOffset, vtxArray.Length, 0, NoMeshChecks);
					_mesh.SetIndexBufferData(idxArray, 0, indexOffset, idxArray.Length, NoMeshChecks);

					// Define subMeshes.
					for (int i = 0, iMax = drawList.CmdBuffer.Size; i < iMax; ++i)
					{
						var cmd = drawList.CmdBuffer[i];
						var descriptor = new SubMeshDescriptor
						{
							topology = MeshTopology.Triangles,
							indexStart = indexOffset + (int)cmd.IdxOffset,
							indexCount = (int)cmd.ElemCount,
							baseVertex = vertexOffset + (int)cmd.VtxOffset,
						};
						descriptors.Add(descriptor);
					}

					vertexOffset += vtxArray.Length;
					indexOffset += idxArray.Length;
				}
			}

			_mesh.SetSubMeshes(descriptors, NoMeshChecks);
			_mesh.UploadMeshData(false);
		}

		private void CreateDrawCommands(CommandBuffer commandBuffer, ImDrawDataPtr drawData, Vector2 framebufferOutputSize)
		{
			IntPtr prevTextureId = IntPtr.Zero;
			var clipOffset = new Num.Vector4(drawData.DisplayPos.X, drawData.DisplayPos.Y,
				drawData.DisplayPos.X, drawData.DisplayPos.Y);
			var clipScale = new Num.Vector4(drawData.FramebufferScale.X, drawData.FramebufferScale.Y,
				drawData.FramebufferScale.X, drawData.FramebufferScale.Y);

			commandBuffer.SetViewport(new Rect(0f, 0f, framebufferOutputSize.x, framebufferOutputSize.y));
			commandBuffer.SetViewProjectionMatrices(
				Matrix4x4.Translate(new Vector3(0.5f / framebufferOutputSize.x, 0.5f / framebufferOutputSize.y, 0f)), // Small adjustment to improve text.
				Matrix4x4.Ortho(0f, framebufferOutputSize.x, framebufferOutputSize.y, 0f, 0f, 1f));

			int subMeshOffset = 0;
			for (int n = 0, nMax = drawData.CmdListsCount; n < nMax; ++n)
			{
				var drawList = drawData.CmdLists[n];
				for (int i = 0, iMax = drawList.CmdBuffer.Size; i < iMax; ++i, ++subMeshOffset)
				{
					var drawCmd = drawList.CmdBuffer[i];
					if (drawCmd.UserCallback != IntPtr.Zero)
					{
						var userDrawCallback = Marshal.GetDelegateForFunctionPointer<UserDrawCallback>(drawCmd.UserCallback);
						userDrawCallback(drawList, drawCmd);
					}
					else
					{
						// Project scissor rectangle into framebuffer space and skip if fully outside.
						var clipRect = drawCmd.ClipRect;
						var clip = new Num.Vector4(
							(clipRect.X - clipOffset.X) * clipScale.X,
							(clipRect.Y - clipOffset.Y) * clipScale.Y,
							(clipRect.Z - clipOffset.Z) * clipScale.Z,
							(clipRect.W - clipOffset.W) * clipScale.W);

						if (clip.X >= framebufferOutputSize.x || clip.Y >= framebufferOutputSize.y || clip.Z < 0f || clip.W < 0f) continue;

						var textureId = drawCmd.GetTexID();
						if (prevTextureId != textureId)
						{
							prevTextureId = textureId;

							// TODO: Implement ImDrawCmdPtr.GetTexID().
							bool hasTexture = _textureManager.TryGetTexture(prevTextureId, out UnityEngine.Texture texture);
							Assert.IsTrue(hasTexture, $"Texture {prevTextureId} does not exist. Try to use UImGuiUtility.GetTextureID().");

							_materialProperties.SetTexture(_textureID, texture);
						}

						commandBuffer.EnableScissorRect(new Rect(clip.X, framebufferOutputSize.y - clip.W, clip.Z - clip.X, clip.W - clip.Y)); // Invert y.
						commandBuffer.DrawMesh(_mesh, Matrix4x4.identity, _material, subMeshOffset, -1, _materialProperties);
					}
				}
			}
			commandBuffer.DisableScissorRect();
		}
	}
}
#endif
