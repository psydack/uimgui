using ImGuiNET;
using NumericsConverter;
using System;
using System.Runtime.InteropServices;
using UImGui.Assets;
using UImGui.Texture;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;
using NVector4 = System.Numerics.Vector4;
using Object = UnityEngine.Object;

// TODO: switch from using ComputeBuffer to GraphicsBuffer
// starting from 2020.1 API that takes ComputeBuffer can also take GraphicsBuffer
// https://docs.unity3d.com/2020.1/Documentation/ScriptReference/GraphicsBuffer.Target.html

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
		private readonly int[] _drawArgs = new int[] { 0, 1, 0, 0, 0 }; // Used to build argument buffer.

		private Material _material;

		private ComputeBuffer _vertexBuffer; // GPU buffer for vertex data.
		private GraphicsBuffer _indexBuffer; // GPU buffer for indexes.
		private ComputeBuffer _argumentsBuffer; // GPU buffer for draw arguments.

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
			// Supports ImDrawCmd::VtxOffset to output large meshes while still using 16-bits indices.
			io.BackendFlags |= ImGuiBackendFlags.RendererHasVtxOffset;

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
			System.Numerics.Vector2 fbSize = drawData.DisplaySize * drawData.FramebufferScale;

			// Avoid rendering when minimized.
			if (fbSize.X <= 0f || fbSize.Y <= 0f || drawData.TotalVtxCount == 0) return;

			Constants.UpdateBuffersMarker.Begin();
			UpdateBuffers(drawData);
			Constants.UpdateBuffersMarker.End();

			cmd.BeginSample(Constants.ExecuteDrawCommandsMarker);

			Constants.CreateDrawComandsMarker.Begin();
			CreateDrawCommands(cmd, drawData, fbSize.ToUnity());
			Constants.CreateDrawComandsMarker.End();

			cmd.EndSample(Constants.ExecuteDrawCommandsMarker);
		}

		private void CreateOrResizeVtxBuffer(ref ComputeBuffer buffer, int count)
		{
			buffer?.Release();

			unsafe
			{
				int num = (((count - 1) / 256) + 1) * 256;
				buffer = new ComputeBuffer(num, sizeof(ImDrawVert));
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

		private void CreateOrResizeArgBuffer(ref ComputeBuffer buffer, int count)
		{
			buffer?.Release();
			unsafe
			{
				int num = (((count - 1) / 256) + 1) * 256;
				buffer = new ComputeBuffer(num, sizeof(int), ComputeBufferType.IndirectArguments);
			}
		}

		private unsafe void UpdateBuffers(ImDrawDataPtr drawData)
		{
			int drawArgCount = 0; // nr of drawArgs is the same as the nr of ImDrawCmd
			for (int n = 0, nMax = drawData.CmdListsCount; n < nMax; ++n)
			{
				drawArgCount += drawData.CmdListsRange[n].CmdBuffer.Size;
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
			int vtxOf = 0;
			int idxOf = 0;
			int argOf = 0;
			for (int n = 0, nMax = drawData.CmdListsCount; n < nMax; ++n)
			{
				ImDrawListPtr drawList = drawData.CmdListsRange[n];
				NativeArray<ImDrawVert> vtxArray = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<ImDrawVert>(
					(void*)drawList.VtxBuffer.Data, drawList.VtxBuffer.Size, Allocator.None);
				NativeArray<ushort> idxArray = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<ushort>(
					(void*)drawList.IdxBuffer.Data, drawList.IdxBuffer.Size, Allocator.None);

#if ENABLE_UNITY_COLLECTIONS_CHECKS
				NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref vtxArray, AtomicSafetyHandle.GetTempMemoryHandle());
				NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref idxArray, AtomicSafetyHandle.GetTempMemoryHandle());
#endif
				// Upload vertex/index data.
				_vertexBuffer.SetData(vtxArray, 0, vtxOf, vtxArray.Length);
				_indexBuffer.SetData(idxArray, 0, idxOf, idxArray.Length);

				// Arguments for indexed draw.
				_drawArgs[3] = vtxOf; // Base vertex location.
				for (int i = 0, iMax = drawList.CmdBuffer.Size; i < iMax; ++i)
				{
					ImDrawCmdPtr cmd = drawList.CmdBuffer[i];
					_drawArgs[0] = (int)cmd.ElemCount; // Index count per instance.
					_drawArgs[2] = idxOf + (int)cmd.IdxOffset; // Start index location.
					_argumentsBuffer.SetData(_drawArgs, 0, argOf, 5);

					argOf += 5; // 5 int for each command.
				}
				vtxOf += vtxArray.Length;
				idxOf += idxArray.Length;
			}
		}

		private void CreateDrawCommands(CommandBuffer cmd, ImDrawDataPtr drawData, Vector2 fbSize)
		{
			IntPtr prevTextureId = IntPtr.Zero;
			NVector4 clipOffst = new NVector4(drawData.DisplayPos.X, drawData.DisplayPos.Y,
				drawData.DisplayPos.X, drawData.DisplayPos.Y);
			Vector4 clipScale = new Vector4(drawData.FramebufferScale.X, drawData.FramebufferScale.Y,
				drawData.FramebufferScale.X, drawData.FramebufferScale.Y);

			_material.SetBuffer(_verticesID, _vertexBuffer); // Bind vertex buffer.

			cmd.SetViewport(new Rect(0f, 0f, fbSize.x, fbSize.y));
			cmd.SetViewProjectionMatrices(
				Matrix4x4.Translate(new Vector3(0.5f / fbSize.x, 0.5f / fbSize.y, 0f)), // Small adjustment to improve text.
				Matrix4x4.Ortho(0f, fbSize.x, fbSize.y, 0f, 0f, 1f));

			int vtxOf = 0;
			int argOf = 0;
			for (int commandListIndex = 0, nMax = drawData.CmdListsCount; commandListIndex < nMax; ++commandListIndex)
			{
				ImDrawListPtr drawList = drawData.CmdListsRange[commandListIndex];
				for (int commandIndex = 0, iMax = drawList.CmdBuffer.Size; commandIndex < iMax; ++commandIndex, argOf += 5 * 4)
				{
					ImDrawCmdPtr drawCmd = drawList.CmdBuffer[commandIndex];
					if (drawCmd.UserCallback != IntPtr.Zero)
					{
						UserDrawCallback userDrawCallback = Marshal.GetDelegateForFunctionPointer<UserDrawCallback>(drawCmd.UserCallback);
						userDrawCallback(drawList, drawCmd);
					}
					else
					{
						// Project scissor rectangle into framebuffer space and skip if fully outside.
						Vector4 clipSize = (drawCmd.ClipRect - clipOffst).ToUnity();
						Vector4 clip = Vector4.Scale(clipSize, clipScale);

						if (clip.x >= fbSize.x || clip.y >= fbSize.y || clip.z < 0f || clip.w < 0f) continue;

						if (prevTextureId != drawCmd.TextureId)
						{
							prevTextureId = drawCmd.TextureId;

							// TODO: Implement ImDrawCmdPtr.GetTexID().
							bool hasTexture = _textureManager.TryGetTexture(prevTextureId, out UnityEngine.Texture texture);
							Assert.IsTrue(hasTexture, $"Texture {prevTextureId} does not exist. Try to use UImGuiUtility.GetTextureID().");

							_materialProperties.SetTexture(_textureID, texture);
						}

						// Base vertex location not automatically added to SV_VertexID.
						_materialProperties.SetInt(_baseVertexID, vtxOf + (int)drawCmd.VtxOffset);

						cmd.EnableScissorRect(new Rect(clip.x, fbSize.y - clip.w, clip.z - clip.x, clip.w - clip.y)); // Invert y.
						cmd.DrawProceduralIndirect(_indexBuffer, Matrix4x4.identity, _material, -1,
							MeshTopology.Triangles, _argumentsBuffer, argOf, _materialProperties);
					}
				}
				vtxOf += drawList.VtxBuffer.Size;
			}
			cmd.DisableScissorRect();
		}
	}
}
