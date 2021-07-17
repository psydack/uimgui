using ImGuiNET;
using System.Collections.Generic;
using UnityEngine;

namespace UImGui
{
	[System.Serializable]
	internal struct FontConfig
	{
		[Tooltip("TTF/OTF data ownership taken by the container ImFontAtlas (will delete memory itself). (default=true)")]
		public bool FontDataOwnedByAtlas;

		[Tooltip("Index of font within TTF/OTF file. (default=0)")]
		public int FontNo;

		[Tooltip("Size in pixels for rasterizer (more or less maps to the resulting font height).")]
		public float SizeInPixels;

		[Tooltip("Rasterize at higher quality for sub-pixel positioning. " +
			"Note the difference between 2 and 3 is minimal so you can reduce this to 2 to save memory. " +
			"Read https://github.com/nothings/stb/blob/master/tests/oversample/README.md for details. (default=3)")]
		public int OversampleH;

		[Tooltip("Rasterize at higher quality for sub-pixel positioning. " +
			"This is not really useful as we don't use sub-pixel positions on the Y axis. (default=1)")]
		public int OversampleV;

		[Tooltip("Align every glyph to pixel boundary." +
			" Useful e.g. if you are merging a non-pixel aligned font with the default font.  (default=false)" +
			"If enabled, you can set OversampleH/V to 1.")]
		public bool PixelSnapH;

		[Tooltip("Extra spacing (in pixels) between glyphs. Only X axis is supported for now. (default=0, 0)")]
		public Vector2 GlyphExtraSpacing;

		[Tooltip("Offest all glyphs from this font input. (default=0, 0)")]
		public Vector2 GlyphOffset;

		[Tooltip("Glyph ranges for different writing systems.")]
		public ScriptGlyphRanges GlyphRanges;

		[Tooltip("Minimum AdvanceX for glyphs, set Min to align font icons, set both Min/Max to enforce mono-space font. (default=0, 0)")]
		public float GlyphMinAdvanceX;

		[Tooltip("Maximum AdvanceX for glyphs. (default=float_max)")]
		public float GlyphMaxAdvanceX;

		[Tooltip("Merge into previous ImFont, so you can combine multiple " +
			"inputs font into one ImFont (e.g. ASCII font + icons + Japanese glyphs). " +
			"You may want to use GlyphOffset.y when merge font of different heights. (default=false)")]
		public bool MergeMode;

		[Tooltip("Settings for custom font builder. THIS IS BUILDER IMPLEMENTATION DEPENDENT. Leave as zero if unsure. (default=0)")]
		public uint FontBuilderFlags;

		[Tooltip("Brighten (>1.0f) or darken (<1.0f) font output. " +
			"Brightening small fonts may be a good workaround to make them more readable. (default=1.0f)")]
		public float RasterizerMultiply;

		[Tooltip("Explicitly specify unicode codepoint of ellipsis character. " +
			"When fonts are being merged first specified ellipsis will be used. (default=-1)")]
		public char EllipsisChar;

		[Tooltip("User-provided list of Unicode range (2 value per range, values are inclusive).")]
		public Range[] CustomGlyphRanges;

		public unsafe void SetDefaults()
		{
			ImFontConfig* imFontConfig = ImGuiNative.ImFontConfig_ImFontConfig();
			SetFrom(imFontConfig);
			ImGuiNative.ImFontConfig_destroy(imFontConfig);
		}

		public void ApplyTo(ImFontConfigPtr im)
		{
			im.FontDataOwnedByAtlas = FontDataOwnedByAtlas;
			im.FontNo = FontNo;
			im.SizePixels = SizeInPixels;
			im.OversampleH = OversampleH;
			im.OversampleV = OversampleV;
			im.PixelSnapH = PixelSnapH;
			im.GlyphExtraSpacing = GlyphExtraSpacing;
			im.GlyphOffset = GlyphOffset;
			im.GlyphMinAdvanceX = GlyphMinAdvanceX;
			im.GlyphMaxAdvanceX = GlyphMaxAdvanceX;
			im.MergeMode = MergeMode;
			im.FontBuilderFlags = FontBuilderFlags;
			im.RasterizerMultiply = RasterizerMultiply;
			im.EllipsisChar = EllipsisChar;

			// setting GlyphRanges requires allocating memory so it is not done here
			// use BuildRanges to get a List with the values, then allocate memory and copy
			// (see TextureManager)
		}

		public void SetFrom(ImFontConfigPtr im)
		{
			FontDataOwnedByAtlas = im.FontDataOwnedByAtlas;
			FontNo = im.FontNo;
			SizeInPixels = im.SizePixels;
			OversampleH = im.OversampleH;
			OversampleV = im.OversampleV;
			PixelSnapH = im.PixelSnapH;
			GlyphExtraSpacing = im.GlyphExtraSpacing;
			GlyphOffset = im.GlyphOffset;
			GlyphMinAdvanceX = im.GlyphMinAdvanceX;
			GlyphMaxAdvanceX = im.GlyphMaxAdvanceX;
			MergeMode = im.MergeMode;
			FontBuilderFlags = im.FontBuilderFlags;
			RasterizerMultiply = im.RasterizerMultiply;
			EllipsisChar = (char)im.EllipsisChar;

			// no good way to set GlyphRanges, do manually
		}

		public unsafe List<ushort> BuildRanges()
		{
			ImFontAtlas* atlas = null;
			List<ushort> ranges = new List<ushort>();

			void AddRangePtr(ushort* r)
			{
				while (*r != 0)
				{
					ranges.Add(*r++);
				}
			};

			if ((GlyphRanges & ScriptGlyphRanges.Default) != 0)
			{
				AddRangePtr(ImGuiNative.ImFontAtlas_GetGlyphRangesDefault(atlas));
			}

			if ((GlyphRanges & ScriptGlyphRanges.Cyrillic) != 0)
			{
				AddRangePtr(ImGuiNative.ImFontAtlas_GetGlyphRangesCyrillic(atlas));
			}

			if ((GlyphRanges & ScriptGlyphRanges.Japanese) != 0)
			{
				AddRangePtr(ImGuiNative.ImFontAtlas_GetGlyphRangesJapanese(atlas));
			}

			if ((GlyphRanges & ScriptGlyphRanges.Korean) != 0)
			{
				AddRangePtr(ImGuiNative.ImFontAtlas_GetGlyphRangesKorean(atlas));
			}

			if ((GlyphRanges & ScriptGlyphRanges.Thai) != 0)
			{
				AddRangePtr(ImGuiNative.ImFontAtlas_GetGlyphRangesThai(atlas));
			}

			if ((GlyphRanges & ScriptGlyphRanges.Vietnamese) != 0)
			{
				AddRangePtr(ImGuiNative.ImFontAtlas_GetGlyphRangesVietnamese(atlas));
			}

			if ((GlyphRanges & ScriptGlyphRanges.ChineseSimplified) != 0)
			{
				AddRangePtr(ImGuiNative.ImFontAtlas_GetGlyphRangesChineseSimplifiedCommon(atlas));
			}

			if ((GlyphRanges & ScriptGlyphRanges.ChineseFull) != 0)
			{
				AddRangePtr(ImGuiNative.ImFontAtlas_GetGlyphRangesChineseFull(atlas));
			}

			if ((GlyphRanges & ScriptGlyphRanges.Custom) != 0)
			{
				foreach (Range range in CustomGlyphRanges)
				{
					ranges.AddRange(new[] { range.Start, range.End });
				}
			}

			return ranges;
		}
	}
}
