using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UImGui.Assets;
using UImGui.Events;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UTexture = UnityEngine.Texture;

namespace UImGui.Texture
{
	public class TextureManager
	{
		private readonly Dictionary<IntPtr, UTexture> _textures = new Dictionary<IntPtr, UTexture>();
		private readonly Dictionary<UTexture, IntPtr> _textureIds = new Dictionary<UTexture, IntPtr>();
		private readonly Dictionary<Sprite, SpriteInfo> _spriteData = new Dictionary<Sprite, SpriteInfo>();
		private readonly HashSet<IntPtr> _allocatedGlyphRangeArrays = new HashSet<IntPtr>();

		// Tracks IDs created by the backend (UploadTexture). DestroyTexture only destroys these.
		private readonly HashSet<IntPtr> _backendOwnedIds = new HashSet<IntPtr>();

		public struct Diagnostics
		{
			public int CreatedCount;
			public int UpdatedCount;
			public int DestroyedCount;
			public int UserRegisteredCount;
		}

		private Diagnostics _diagnostics;
		public Diagnostics GetDiagnostics() => _diagnostics;

#if UNITY_INCLUDE_TESTS || DEVELOPMENT_BUILD
		public bool HasCreatedBackendTextures => _backendOwnedIds.Count > 0;
#endif

		public unsafe void UpdateTextures(ImDrawDataPtr drawData)
		{
			if (drawData.NativePtr == null || drawData.NativePtr->Textures == null)
				return;

			var textures = drawData.Textures;
			for (int i = 0; i < textures.Size; i++)
			{
				var texData = textures[i];
				switch (texData.Status)
				{
					case ImTextureStatus.WantCreate:
						UploadTexture(texData);
						break;
					case ImTextureStatus.WantUpdates:
						UpdateTexture(texData);
						break;
					case ImTextureStatus.WantDestroy when texData.UnusedFrames > 0:
						DestroyTexture(texData);
						break;
				}
			}
		}

		private unsafe void UploadTexture(ImTextureDataPtr texData)
		{
			Constants.TextureUploadMarker.Begin();
			try
			{
				byte* pixels = (byte*)texData.Pixels;
				int width = texData.Width;
				int height = texData.Height;
				int bytesPerPixel = texData.BytesPerPixel;

				if (pixels == null || width <= 0 || height <= 0 || bytesPerPixel <= 0)
				{
					Debug.LogError("[UImGui] Texture data invalid — atlas was not built.");
					return;
				}

				var tex2d = new Texture2D(width, height, TextureFormat.RGBA32, false, false)
				{
					filterMode = FilterMode.Point
				};

				var dstData = tex2d.GetRawTextureData<byte>();
				CopyPixelsToTexture2D(pixels, width, height, bytesPerPixel, dstData);

				if (bytesPerPixel != 4 && bytesPerPixel != 1)
				{
					Debug.LogError($"[UImGui] Unsupported texture format BytesPerPixel={bytesPerPixel}.");
					UnityEngine.Object.Destroy(tex2d);
					return;
				}

				tex2d.Apply();
				IntPtr id = RegisterTexture(tex2d);
				_backendOwnedIds.Add(id);
				texData.SetTexID(id);
				texData.SetStatus(ImTextureStatus.OK);
				_diagnostics.CreatedCount++;
			}
			finally
			{
				Constants.TextureUploadMarker.End();
			}
		}

		private unsafe void UpdateTexture(ImTextureDataPtr texData)
		{
			Constants.TextureUpdateMarker.Begin();
			try
			{
				IntPtr id = texData.GetTexID();
				if (id == IntPtr.Zero || !_textures.TryGetValue(id, out UTexture texture) || texture is not Texture2D tex2d)
				{
					Debug.LogError("[UImGui] WantUpdates: no existing texture found.");
					return;
				}

				byte* pixels = (byte*)texData.Pixels;
				int width = texData.Width;
				int height = texData.Height;
				int bytesPerPixel = texData.BytesPerPixel;

				if (pixels == null || width <= 0 || height <= 0 || bytesPerPixel <= 0)
				{
					Debug.LogError("[UImGui] WantUpdates: texture data is invalid.");
					return;
				}

				// Size changed — destroy and recreate.
				if (tex2d.width != width || tex2d.height != height)
				{
					_textures.Remove(id);
					_textureIds.Remove(tex2d);
					_backendOwnedIds.Remove(id);
					UnityEngine.Object.Destroy(tex2d);

					tex2d = new Texture2D(width, height, TextureFormat.RGBA32, false, false)
					{
						filterMode = FilterMode.Point
					};
					id = RegisterTexture(tex2d);
					_backendOwnedIds.Add(id);
					texData.SetTexID(id);
				}

				var dstData = tex2d.GetRawTextureData<byte>();
				CopyPixelsToTexture2D(pixels, width, height, bytesPerPixel, dstData);

				if (bytesPerPixel != 4 && bytesPerPixel != 1)
				{
					Debug.LogError($"[UImGui] WantUpdates: unsupported texture format BytesPerPixel={bytesPerPixel}.");
					return;
				}

				tex2d.Apply();
				texData.SetStatus(ImTextureStatus.OK);
				_diagnostics.UpdatedCount++;
			}
			finally
			{
				Constants.TextureUpdateMarker.End();
			}
		}

		// Y-flips and copies ImGui pixel data into a Unity texture's raw data buffer.
		private static unsafe void CopyPixelsToTexture2D(byte* pixels, int width, int height, int bytesPerPixel, NativeArray<byte> dstData)
		{
			var srcData = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<byte>(
				pixels, width * height * bytesPerPixel, Allocator.None);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
			NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref srcData, AtomicSafetyHandle.GetTempMemoryHandle());
#endif
			if (bytesPerPixel == 4)
			{
				int stride = width * bytesPerPixel;
				for (int y = 0; y < height; ++y)
					NativeArray<byte>.Copy(srcData, y * stride, dstData, (height - y - 1) * stride, stride);
			}
			else if (bytesPerPixel == 1)
			{
				for (int y = 0; y < height; ++y)
				{
					int srcRow = y * width;
					int dstRow = (height - y - 1) * width * 4;
					for (int x = 0; x < width; ++x)
					{
						byte a = srcData[srcRow + x];
						int d = dstRow + x * 4;
						dstData[d] = 255; dstData[d + 1] = 255; dstData[d + 2] = 255; dstData[d + 3] = a;
					}
				}
			}
		}

		private void DestroyTexture(ImTextureDataPtr texData)
		{
			Constants.TextureDestroyMarker.Begin();
			try
			{
				IntPtr id = texData.GetTexID();
				if (id == IntPtr.Zero || !_backendOwnedIds.Contains(id))
				{
					// Safety: never destroy user-registered textures.
					texData.SetStatus(ImTextureStatus.Destroyed);
					return;
				}

				if (_textures.TryGetValue(id, out UTexture tex))
				{
					_textures.Remove(id);
					_textureIds.Remove(tex);
					if (tex is Texture2D tex2d)
						UnityEngine.Object.Destroy(tex2d);
				}

				_backendOwnedIds.Remove(id);
				texData.SetTexID(IntPtr.Zero);
				texData.SetStatus(ImTextureStatus.Destroyed);
				_diagnostics.DestroyedCount++;
			}
			finally
			{
				Constants.TextureDestroyMarker.End();
			}
		}

		public void Shutdown()
		{
			FreeGlyphRangeArrays();

			foreach (var tex in _textureIds.Keys)
			{
				if (tex is Texture2D tex2d)
					UnityEngine.Object.Destroy(tex2d);
			}

			_textures.Clear();
			_textureIds.Clear();
			_backendOwnedIds.Clear();
			_spriteData.Clear();
		}

		public bool TryGetTexture(IntPtr id, out UTexture texture)
		{
			return _textures.TryGetValue(id, out texture);
		}

		public IntPtr GetTextureId(UTexture texture)
		{
			if (texture == null)
			{
				Debug.LogWarning("[UImGui] Cannot register a null texture.");
				return IntPtr.Zero;
			}

			if (_textureIds.TryGetValue(texture, out IntPtr id))
				return id;

			id = RegisterTexture(texture);
			_diagnostics.UserRegisteredCount++;
			return id;
		}

		public SpriteInfo GetSpriteInfo(Sprite sprite)
		{
			if (sprite == null)
			{
				Debug.LogWarning("[UImGui] Cannot get sprite info for a null sprite.");
				return null;
			}

			if (!_spriteData.TryGetValue(sprite, out var spriteInfo))
			{
				_spriteData[sprite] = spriteInfo = new SpriteInfo
				{
					Texture = sprite.texture,
					Size = sprite.rect.size,
					UV0 = sprite.uv[0],
					UV1 = sprite.uv[1],
				};
			}

			return spriteInfo;
		}

		private IntPtr RegisterTexture(UTexture texture)
		{
			if (texture == null)
				return IntPtr.Zero;

			IntPtr id = texture.GetNativeTexturePtr();
			_textures[id] = texture;
			_textureIds[texture] = id;
			return id;
		}

		public void BuildFontAtlas(ImGuiIOPtr io, in FontAtlasConfigAsset settings, FontInitializerEvent custom)
		{
			if (io.Fonts.TexIsBuilt)
				DestroyFontAtlas(io);

			io.Fonts.Flags |= ImFontAtlasFlags.NoBakedLines;

			if (!io.MouseDrawCursor)
				io.Fonts.Flags |= ImFontAtlasFlags.NoMouseCursors;

			if (settings == null)
			{
				if (custom.GetPersistentEventCount() > 0)
					custom.Invoke(io);

				if (io.Fonts.Fonts.Size == 0)
					io.Fonts.AddFontDefault();

				return;
			}

			for (int fontIndex = 0; fontIndex < settings.Fonts.Length; fontIndex++)
			{
				var fontDefinition = settings.Fonts[fontIndex];
				string fontPath = System.IO.Path.Combine(Application.streamingAssetsPath, fontDefinition.Path);
				if (!System.IO.File.Exists(fontPath))
				{
					Debug.LogWarning($"[UImGui] Font file not found: {fontPath}. Check that the file is in StreamingAssets.");
					continue;
				}

				string ext = System.IO.Path.GetExtension(fontPath).ToLowerInvariant();
				if (ext != ".ttf" && ext != ".otf")
				{
					Debug.LogWarning($"[UImGui] Font file '{fontPath}' is not a .ttf or .otf file and will be skipped.");
					continue;
				}

				unsafe
				{
					ImFontConfig fontConfig = default;
					var fontConfigPtr = new ImFontConfigPtr(&fontConfig);

					fontDefinition.Config.ApplyTo(fontConfigPtr);
					fontConfigPtr.GlyphRanges = AllocateGlyphRangeArray(fontDefinition.Config);

					io.Fonts.AddFontFromFileTTF(fontPath, fontDefinition.Config.SizeInPixels, fontConfigPtr);
				}
			}

			if (io.Fonts.Fonts.Size == 0)
				io.Fonts.AddFontDefault();
		}

		public unsafe void DestroyFontAtlas(ImGuiIOPtr io)
		{
			FreeGlyphRangeArrays();
			io.Fonts.Clear();
			io.NativePtr->FontDefault = default;
		}

		private unsafe IntPtr AllocateGlyphRangeArray(in FontConfig fontConfig)
		{
			var values = fontConfig.BuildRanges();
			if (values.Count == 0)
				return IntPtr.Zero;

			int byteCount = sizeof(ushort) * (values.Count + 1);
			ushort* ranges = (ushort*)Marshal.AllocHGlobal(byteCount);
			_allocatedGlyphRangeArrays.Add((IntPtr)ranges);

			for (int i = 0; i < values.Count; ++i)
				ranges[i] = values[i];
			ranges[values.Count] = 0;

			return (IntPtr)ranges;
		}

		private unsafe void FreeGlyphRangeArrays()
		{
			foreach (IntPtr range in _allocatedGlyphRangeArrays)
				Marshal.FreeHGlobal(range);
			_allocatedGlyphRangeArrays.Clear();
		}
	}
}
