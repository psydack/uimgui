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
	// TODO: Write documentation for methods
	public class TextureManager
	{
		private Texture2D _atlasTexture;

		private readonly Dictionary<IntPtr, UTexture> _textures = new Dictionary<IntPtr, UTexture>();
		private readonly Dictionary<UTexture, IntPtr> _textureIds = new Dictionary<UTexture, IntPtr>();
		private readonly Dictionary<Sprite, SpriteInfo> _spriteData = new Dictionary<Sprite, SpriteInfo>();

		private readonly HashSet<IntPtr> _allocatedGlyphRangeArrays = new HashSet<IntPtr>();

		public bool HasValidAtlas => _atlasTexture != null;

		public unsafe void Initialize(ImGuiIOPtr io)
		{
			var atlasPtr = io.Fonts;
			atlasPtr.RendererHasTextures = true;
			var texData = atlasPtr.TexData;
			byte* pixels = (byte*)texData.Pixels;
			int width = texData.Width;
			int height = texData.Height;
			int bytesPerPixel = texData.BytesPerPixel;

			if (pixels == null || width <= 0 || height <= 0 || bytesPerPixel <= 0)
			{
				Debug.LogError("[UImGui] Font atlas texture data is invalid. ImGui font atlas was not built.");
				return;
			}

			_atlasTexture = new Texture2D(width, height, TextureFormat.RGBA32, false, false)
			{
				filterMode = FilterMode.Point
			};

			// TODO: Remove collections and make native array manually.
			var srcData = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<byte>(pixels, width * height * bytesPerPixel, Allocator.None);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
			NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref srcData, AtomicSafetyHandle.GetTempMemoryHandle());
#endif
			// Invert y while copying the atlas texture.
			var dstData = _atlasTexture.GetRawTextureData<byte>();
			int stride = width * bytesPerPixel;
			for (int y = 0; y < height; ++y)
			{
				NativeArray<byte>.Copy(srcData, y * stride, dstData, (height - y - 1) * stride, stride);
			}

			_atlasTexture.Apply();
		}

		public void Shutdown()
		{
			FreeGlyphRangeArrays();

			_textures.Clear();
			_textureIds.Clear();
			_spriteData.Clear();

			if (_atlasTexture != null)
			{
				UnityEngine.Object.Destroy(_atlasTexture);
				_atlasTexture = null;
			}
		}

		public void PrepareFrame(ImGuiIOPtr io)
		{
			if (_atlasTexture == null)
			{
				Initialize(io);
			}

			if (_atlasTexture == null)
			{
				return;
			}

			io.Fonts.RendererHasTextures = true;
			IntPtr id = RegisterTexture(_atlasTexture);
			io.Fonts.TexData.SetTexID(id);
			io.Fonts.TexData.SetStatus(ImTextureStatus.OK);
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

			return _textureIds.TryGetValue(texture, out IntPtr id) ? id : RegisterTexture(texture);
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
			{
				return IntPtr.Zero;
			}

			IntPtr id = texture.GetNativeTexturePtr();
			_textures[id] = texture;
			_textureIds[texture] = id;

			return id;
		}

		public void BuildFontAtlas(ImGuiIOPtr io, in FontAtlasConfigAsset settings, FontInitializerEvent custom)
		{
			if (io.Fonts.TexIsBuilt)
			{
				DestroyFontAtlas(io);
			}

			if (!io.MouseDrawCursor)
			{
				io.Fonts.Flags |= ImFontAtlasFlags.NoMouseCursors;
			}

			if (settings == null)
			{
				if (custom.GetPersistentEventCount() > 0)
				{
					custom.Invoke(io);
				}
				else
				{
					io.Fonts.AddFontDefaultBitmap();
				}

				return;
			}

			// Add fonts from config asset.
			for (int fontIndex = 0; fontIndex < settings.Fonts.Length; fontIndex++)
			{
				var fontDefinition = settings.Fonts[fontIndex];
				string fontPath = System.IO.Path.Combine(Application.streamingAssetsPath, fontDefinition.Path);
				if (!System.IO.File.Exists(fontPath))
				{
					Debug.Log($"Font file not found: {fontPath}");
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
			{
				io.Fonts.AddFontDefaultBitmap();
			}

		}

		public unsafe void DestroyFontAtlas(ImGuiIOPtr io)
		{
			FreeGlyphRangeArrays();

			io.Fonts.Clear(); // Previous FontDefault reference no longer valid.
			io.NativePtr->FontDefault = default; // NULL uses Fonts[0].
		}

		private unsafe IntPtr AllocateGlyphRangeArray(in FontConfig fontConfig)
		{
			var values = fontConfig.BuildRanges();
			if (values.Count == 0)
			{
				return IntPtr.Zero;
			}

			int byteCount = sizeof(ushort) * (values.Count + 1); // terminating zero.
			ushort* ranges = (ushort*)Marshal.AllocHGlobal(byteCount);
			_allocatedGlyphRangeArrays.Add((IntPtr)ranges);

			for (int i = 0; i < values.Count; ++i)
			{
				ranges[i] = values[i];
			}
			ranges[values.Count] = 0;

			return (IntPtr)ranges;
		}

		private unsafe void FreeGlyphRangeArrays()
		{
			foreach (IntPtr range in _allocatedGlyphRangeArrays)
			{
				Marshal.FreeHGlobal(range);
			}

			_allocatedGlyphRangeArrays.Clear();
		}
	}
}
