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
	internal class TextureManager
	{
		private Texture2D _atlasTexture;

		private readonly Dictionary<IntPtr, UTexture> _textures = new Dictionary<IntPtr, UTexture>();
		private readonly Dictionary<UTexture, IntPtr> _textureIds = new Dictionary<UTexture, IntPtr>();
		private readonly Dictionary<Sprite, SpriteInfo> _spriteData = new Dictionary<Sprite, SpriteInfo>();

		private readonly HashSet<IntPtr> _allocatedGlyphRangeArrays = new HashSet<IntPtr>();

		public unsafe void Initialize(ImGuiIOPtr io)
		{
			ImFontAtlasPtr atlasPtr = io.Fonts;
			atlasPtr.GetTexDataAsRGBA32(out byte* pixels, out int width, out int height, out int bytesPerPixel);

			_atlasTexture = new Texture2D(width, height, TextureFormat.RGBA32, false, false)
			{
				filterMode = FilterMode.Point
			};

			// TODO: Remove collections and make native array manually.
			NativeArray<byte> srcData = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<byte>(pixels, width * height * bytesPerPixel, Allocator.None);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
			NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref srcData, AtomicSafetyHandle.GetTempMemoryHandle());
#endif
			// Invert y while copying the atlas texture.
			NativeArray<byte> dstData = _atlasTexture.GetRawTextureData<byte>();
			int stride = width * bytesPerPixel;
			for (int y = 0; y < height; ++y)
			{
				NativeArray<byte>.Copy(srcData, y * stride, dstData, (height - y - 1) * stride, stride);
			}

			_atlasTexture.Apply();
		}

		public void Shutdown()
		{
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
			IntPtr id = RegisterTexture(_atlasTexture);
			io.Fonts.SetTexID(id);
		}

		public bool TryGetTexture(IntPtr id, out UTexture texture)
		{
			return _textures.TryGetValue(id, out texture);
		}

		public IntPtr GetTextureId(UTexture texture)
		{
			return _textureIds.TryGetValue(texture, out IntPtr id) ? id : RegisterTexture(texture);
		}

		public SpriteInfo GetSpriteInfo(Sprite sprite)
		{
			if (!_spriteData.TryGetValue(sprite, out SpriteInfo spriteInfo))
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
			IntPtr id = texture.GetNativeTexturePtr();
			_textures[id] = texture;
			_textureIds[texture] = id;

			return id;
		}

		public void BuildFontAtlas(ImGuiIOPtr io, in FontAtlasConfigAsset settings, FontInitializerEvent custom)
		{
			if (io.Fonts.IsBuilt())
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
					io.Fonts.AddFontDefault();
				}

				io.Fonts.Build();
				return;
			}

			// Add fonts from config asset.
			for (int fontIndex = 0; fontIndex < settings.Fonts.Length; fontIndex++)
			{
				FontDefinition fontDefinition = settings.Fonts[fontIndex];
				string fontPath = System.IO.Path.Combine(Application.streamingAssetsPath, fontDefinition.Path);
				if (!System.IO.File.Exists(fontPath))
				{
					Debug.Log($"Font file not found: {fontPath}");
					continue;
				}

				unsafe
				{
					ImFontConfig fontConfig = default;
					ImFontConfigPtr fontConfigPtr = new ImFontConfigPtr(&fontConfig);

					fontDefinition.Config.ApplyTo(fontConfigPtr);
					fontConfigPtr.GlyphRanges = AllocateGlyphRangeArray(fontDefinition.Config);

					// TODO: Add check if is TTF File.
					io.Fonts.AddFontFromFileTTF(fontPath, fontDefinition.Config.SizeInPixels, fontConfigPtr);
				}
			}

			if (io.Fonts.Fonts.Size == 0)
			{
				io.Fonts.AddFontDefault();
			}

			io.Fonts.Build();
		}

		public unsafe void DestroyFontAtlas(ImGuiIOPtr io)
		{
			FreeGlyphRangeArrays();

			io.Fonts.Clear(); // Previous FontDefault reference no longer valid.
			io.NativePtr->FontDefault = default; // NULL uses Fonts[0].
		}

		private unsafe IntPtr AllocateGlyphRangeArray(in FontConfig fontConfig)
		{
			List<ushort> values = fontConfig.BuildRanges();
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
