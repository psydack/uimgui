using ImGuiNET;
using System.Collections.Generic;
using UnityEngine;

namespace UImGui
{
	[System.Serializable]
	public struct FontConfig
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
			im.FontNo = (uint)FontNo;
			im.SizePixels = SizeInPixels;
			im.OversampleH = (sbyte)OversampleH;
			im.OversampleV = (sbyte)OversampleV;
			im.PixelSnapH = PixelSnapH;
			im.GlyphExtraAdvanceX = GlyphExtraSpacing.x;
			im.GlyphOffset = GlyphOffset.AsNumerics();
			im.GlyphMinAdvanceX = GlyphMinAdvanceX;
			im.GlyphMaxAdvanceX = GlyphMaxAdvanceX;
			im.MergeMode = MergeMode;
			im.FontLoaderFlags = FontBuilderFlags;
			im.RasterizerMultiply = RasterizerMultiply;
			im.EllipsisChar = EllipsisChar;

			// setting GlyphRanges requires allocating memory so it is not done here
			// use BuildRanges to get a List with the values, then allocate memory and copy
			// (see TextureManager)
		}

		public void SetFrom(ImFontConfigPtr im)
		{
			FontDataOwnedByAtlas = im.FontDataOwnedByAtlas;
			FontNo = (int)im.FontNo;
			SizeInPixels = im.SizePixels;
			OversampleH = im.OversampleH;
			OversampleV = im.OversampleV;
			PixelSnapH = im.PixelSnapH;
			GlyphExtraSpacing = new Vector2(im.GlyphExtraAdvanceX, 0f);
			var glyphOffset = im.GlyphOffset;
			GlyphOffset = glyphOffset.AsUnity();
			GlyphMinAdvanceX = im.GlyphMinAdvanceX;
			GlyphMaxAdvanceX = im.GlyphMaxAdvanceX;
			MergeMode = im.MergeMode;
			FontBuilderFlags = im.FontLoaderFlags;
			RasterizerMultiply = im.RasterizerMultiply;
			EllipsisChar = (char)im.EllipsisChar;

			// no good way to set GlyphRanges, do manually
		}

		public unsafe List<ushort> BuildRanges()
		{
			ImFontAtlas* atlas = null;
			var ranges = new List<ushort>();
			ScriptGlyphRanges selected = GlyphRanges & ScriptGlyphRanges.Everything;
			// Stale serialized assets may have out-of-range bits that mask to zero;
			// fall back to Default only in that case. An explicit None (GlyphRanges == 0)
			// means "let the font decide its own ranges" and must be honoured.
			if (selected == ScriptGlyphRanges.None && GlyphRanges != ScriptGlyphRanges.None)
				selected = ScriptGlyphRanges.Default;

			void AddRangePtr(ushort* r)
			{
				while (*r != 0)
				{
					ranges.Add(*r++);
				}
			};

			void AddUnicodeRange(ushort start, ushort end)
			{
				ranges.Add(start);
				ranges.Add(end);
			}

			// Always include default Latin/ASCII when any built-in script is selected
			// (mirrors pre-6.1.1 behaviour — Custom-only is the one exception).
			if ((selected & ~ScriptGlyphRanges.Custom) != 0)
				AddRangePtr(ImGuiNative.ImFontAtlas_GetGlyphRangesDefault(atlas));

			if ((selected & ScriptGlyphRanges.Cyrillic) != 0)
			{
				AddUnicodeRange(0x0400, 0x052F);
				AddUnicodeRange(0x2DE0, 0x2DFF);
				AddUnicodeRange(0xA640, 0xA69F);
			}

			if ((selected & ScriptGlyphRanges.Japanese) != 0)
			{
				AddUnicodeRange(0x3000, 0x303F); // CJK punctuation (、。…)
				AddUnicodeRange(0x3040, 0x30FF);
				AddUnicodeRange(0x31F0, 0x31FF);
				AddUnicodeRange(0xFF66, 0xFF9F);
				AddUnicodeRange(0x4E00, 0x9FFF); // CJK base
			}

			if ((selected & ScriptGlyphRanges.Korean) != 0)
			{
				AddUnicodeRange(0x1100, 0x11FF);
				AddUnicodeRange(0x3130, 0x318F);
				AddUnicodeRange(0xAC00, 0xD7AF);
			}

			if ((selected & ScriptGlyphRanges.Thai) != 0)
				AddUnicodeRange(0x0E00, 0x0E7F);

			if ((selected & ScriptGlyphRanges.Vietnamese) != 0)
			{
				AddUnicodeRange(0x0102, 0x0103);
				AddUnicodeRange(0x0110, 0x0111);
				AddUnicodeRange(0x0128, 0x0129);
				AddUnicodeRange(0x0168, 0x0169);
				AddUnicodeRange(0x01A0, 0x01A1);
				AddUnicodeRange(0x01AF, 0x01B0);
				AddUnicodeRange(0x1EA0, 0x1EF9);
			}

			// Chinese: only add CJK base if Japanese didn't already include it.
			bool hasCjkBase = (selected & ScriptGlyphRanges.Japanese) != 0;

			if ((selected & ScriptGlyphRanges.ChineseSimplified) != 0)
			{
				AddUnicodeRange(0x3000, 0x303F);
				AddUnicodeRange(0x3400, 0x4DBF);
				if (!hasCjkBase) AddUnicodeRange(0x4E00, 0x9FFF);
			}

			if ((selected & ScriptGlyphRanges.ChineseFull) != 0)
			{
				AddUnicodeRange(0x3000, 0x303F);
				if ((selected & ScriptGlyphRanges.ChineseSimplified) == 0) AddUnicodeRange(0x3400, 0x4DBF);
				if (!hasCjkBase) AddUnicodeRange(0x4E00, 0x9FFF);
				AddUnicodeRange(0xF900, 0xFAFF);
			}

			if ((selected & ScriptGlyphRanges.Custom) != 0 && CustomGlyphRanges != null)
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
