using ImGuiNET;
using System;
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
	/// <summary>
	/// Renderer bindings in charge of producing instructions for rendering ImGui draw data.
	/// Uses DrawProceduralIndirect to build geometry from a buffer of vertex data.
	/// </summary>
	/// <remarks>Requires shader model 4.5 level hardware.</remarks>
	internal sealed class RendererProcedural : IRenderer
	{
		private readonly Shader _shader;
		private readonly int _textureID;
		private readonly int _verticesID;
		private readonly int _baseVertexID;
		private readonly TextureManager _textureManager;

		private readonly MaterialPropertyBlock _materialProperties = new MaterialPropertyBlock();

		private Material _material;

		private GraphicsBuffer _vertexBuffer; // GPU buffer for vertex data.
		private GraphicsBuffer _indexBuffer; // GPU buffer for indexes.
		private GraphicsBuffer _argumentsBuffer; // GPU buffer for draw arguments.

		public RendererProcedural(ShaderResourcesAsset resources, TextureManager texManager)
		{
			if (SystemInfo.graphicsShaderLevel < 45)
			{
				throw new System.Exception("Device not supported.");
			}

			_shader = resources.Shader.Procedural;
			_textureManager = texManager;
			_textureID = Shader.PropertyToID(resources.PropertyNames.Texture);
			_verticesID = Shader.PropertyToID(resources.PropertyNames.Vertices);
			_baseVertexID = Shader.PropertyToID(resources.PropertyNames.BaseVertex);
		}

		public void Initialize(ImGuiIOPtr io)
		{
			io.SetBackendRendererName("Unity Procedural");
			// Supports large meshes and the explicit texture backend expected by current ImGui.NET/cimgui.
			io.BackendFlags |= ImGuiBackendFlags.RendererHasVtxOffset | ImGuiBackendFlags.RendererHasTextures;

			_material = new Material(_shader)
			{
				hideFlags = HideFlags.HideAndDontSave & ~HideFlags.DontUnloadUnusedAsset
			};
		}

		public void Shutdown(ImGuiIOPtr io)
		{
			io.SetBackendRendererName(null);

			if (_material != null) { Object.Destroy(_material); _material = null; }
			_vertexBuffer?.Release(); _vertexBuffer = null;
			_indexBuffer?.Release(); _indexBuffer = null;
			_argumentsBuffer?.Release(); _argumentsBuffer = null;
		}

		public void RenderDrawLists(CommandBuffer cmd, ImDrawDataPtr drawData)
		{
			var framebufferOutputSize = (drawData.DisplaySize * drawData.FramebufferScale).ToUnity();

			// Avoid rendering when minimized.
			if (framebufferOutputSize.x <= 0f || framebufferOutputSize.y <= 0f || drawData.TotalVtxCount == 0) return;

			Constants.UpdateBuffersMarker.Begin();
			UpdateBuffers(drawData);
			Constants.UpdateBuffersMarker.End();

			cmd.BeginSample(Constants.ExecuteDrawCommandsMarker);

			Constants.CreateDrawCommandsProceduralMarker.Begin();
			CreateDrawCommands(cmd, drawData, framebufferOutputSize);
			Constants.CreateDrawCommandsProceduralMarker.End();

			cmd.EndSample(Constants.ExecuteDrawCommandsMarker);
		}

		private void CreateOrResizeVtxBuffer(ref GraphicsBuffer buffer, int count)
		{
			buffer?.Release();

			unsafe
			{
				int num = (((count - 1) / 256) + 1) * 256;
				buffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, num, sizeof(ImDrawVert));
			}
		}

		private void CreateOrResizeIdxBuffer(ref GraphicsBuffer buffer, int count)
		{
			buffer?.Release();

			unsafe
			{
				int num = (((count - 1) / 256) + 1) * 256;
				buffer = new GraphicsBuffer(GraphicsBuffer.Target.Index, num, sizeof(ushort));
			}
		}

		private void CreateOrResizeArgBuffer(ref GraphicsBuffer buffer, int count)
		{
			buffer?.Release();
			unsafe
			{
				int num = (((count - 1) / 256) + 1) * 256;
				buffer = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, num, sizeof(int));
			}
		}

		private unsafe void UpdateBuffers(ImDrawDataPtr drawData)
		{
			int drawArgCount = 0; // nr of drawArgs is the same as the nr of ImDrawCmd
			for (int n = 0, nMax = drawData.CmdListsCount; n < nMax; ++n)
			{
				drawArgCount += drawData.CmdLists[n].CmdBuffer.Size;
			}

			// create or resize vertex/index buffers
			if (_vertexBuffer == null || _vertexBuffer.count < drawData.TotalVtxCount)
			{
				CreateOrResizeVtxBuffer(ref _vertexBuffer, drawData.TotalVtxCount);
			}

			if (_indexBuffer == null || _indexBuffer.count < drawData.TotalIdxCount)
			{
				CreateOrResizeIdxBuffer(ref _indexBuffer, drawData.TotalIdxCount);
			}

			if (_argumentsBuffer == null || _argumentsBuffer.count < drawArgCount * 5)
			{
				CreateOrResizeArgBuffer(ref _argumentsBuffer, drawArgCount * 5);
			}

			// upload vertex/index data into buffers
			int vertexOffset = 0;
			int indexOffset = 0;
			int argumentOffset = 0;
			for (int n = 0, nMax = drawData.CmdListsCount; n < nMax; ++n)
			{
				var drawList = drawData.CmdLists[n];
				var vtxArray = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<ImDrawVert>(
					(void*)drawList.VtxBuffer.Data, drawList.VtxBuffer.Size, Allocator.None);
				var idxArray = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<ushort>(
					(void*)drawList.IdxBuffer.Data, drawList.IdxBuffer.Size, Allocator.None);

#if ENABLE_UNITY_COLLECTIONS_CHECKS
				NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref vtxArray, AtomicSafetyHandle.GetTempMemoryHandle());
				NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref idxArray, AtomicSafetyHandle.GetTempMemoryHandle());
#endif
				// Upload vertex/index data.
				_vertexBuffer.SetData(vtxArray, 0, vertexOffset, vtxArray.Length);
				_indexBuffer.SetData(idxArray, 0, indexOffset, idxArray.Length);

				// Arguments for indexed draw.
				for (int meshIndex = 0, iMax = drawList.CmdBuffer.Size; meshIndex < iMax; ++meshIndex)
				{
					var cmd = drawList.CmdBuffer[meshIndex];
					var drawArgs = new int[]
					{
						(int)cmd.ElemCount,
						1,
						indexOffset + (int)cmd.IdxOffset,
						vertexOffset,
						0
					};
					_argumentsBuffer.SetData(drawArgs, 0, argumentOffset, 5);
					argumentOffset += 5; // 5 int for each command.
				}
				vertexOffset += vtxArray.Length;
				indexOffset += idxArray.Length;
			}
		}

		private void CreateDrawCommands(CommandBuffer cmd, ImDrawDataPtr drawData, Vector2 framebufferOutputSize)
		{
			IntPtr prevTextureId = IntPtr.Zero;
			var clipOffset = new Num.Vector4(drawData.DisplayPos.X, drawData.DisplayPos.Y,
				drawData.DisplayPos.X, drawData.DisplayPos.Y);
			var clipScale = new Num.Vector4(drawData.FramebufferScale.X, drawData.FramebufferScale.Y,
				drawData.FramebufferScale.X, drawData.FramebufferScale.Y);

			_material.SetBuffer(_verticesID, _vertexBuffer); // Bind vertex buffer.

			cmd.SetViewport(new Rect(0f, 0f, framebufferOutputSize.x, framebufferOutputSize.y));
			cmd.SetViewProjectionMatrices(
				Matrix4x4.Translate(new Vector3(0.5f / framebufferOutputSize.x, 0.5f / framebufferOutputSize.y, 0f)), // Small adjustment to improve text.
				Matrix4x4.Ortho(0f, framebufferOutputSize.x, framebufferOutputSize.y, 0f, 0f, 1f));

			int vertexOffset = 0;
			int argumentOffset = 0;
			for (int commandListIndex = 0, nMax = drawData.CmdListsCount; commandListIndex < nMax; ++commandListIndex)
			{
				var drawList = drawData.CmdLists[commandListIndex];
				for (int commandIndex = 0, iMax = drawList.CmdBuffer.Size; commandIndex < iMax; ++commandIndex, argumentOffset += 5 * 4)
				{
					var drawCmd = drawList.CmdBuffer[commandIndex];
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

						// Base vertex location not automatically added to SV_VertexID.
						_materialProperties.SetInt(_baseVertexID, vertexOffset + (int)drawCmd.VtxOffset);

						cmd.EnableScissorRect(new Rect(clip.X, framebufferOutputSize.y - clip.W, clip.Z - clip.X, clip.W - clip.Y)); // Invert y.
						cmd.DrawProceduralIndirect(_indexBuffer, Matrix4x4.identity, _material, -1,
							MeshTopology.Triangles, _argumentsBuffer, argumentOffset, _materialProperties);
					}
				}
				vertexOffset += drawList.VtxBuffer.Size;
			}
			cmd.DisableScissorRect();
		}
	}
}
