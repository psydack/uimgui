using NUnit.Framework;
using System.Collections.Generic;

namespace UImGui.Tests
{
	internal class FontConfigTests
	{
		[Test]
		public void BuildRanges_Default_ContainsAscii()
		{
			var cfg = new FontConfig { GlyphRanges = ScriptGlyphRanges.Default };
			List<ushort> ranges = cfg.BuildRanges();
			Assert.IsTrue(ranges.Count >= 2, "Default ranges must contain at least one pair.");
		}

		[Test]
		public void BuildRanges_None_ReturnsEmpty()
		{
			var cfg = new FontConfig { GlyphRanges = ScriptGlyphRanges.None };
			List<ushort> ranges = cfg.BuildRanges();
			Assert.AreEqual(0, ranges.Count);
		}

		[Test]
		public void BuildRanges_Japanese_ContainsCJK()
		{
			var cfg = new FontConfig { GlyphRanges = ScriptGlyphRanges.Japanese };
			List<ushort> ranges = cfg.BuildRanges();
			bool hasCjk = false;
			for (int i = 0; i + 1 < ranges.Count; i += 2)
			{
				if (ranges[i] <= 0x4E00 && ranges[i + 1] >= 0x9FFF)
					hasCjk = true;
			}
			Assert.IsTrue(hasCjk, "Japanese ranges must cover CJK Unified Ideographs.");
		}

		[Test]
		public void BuildRanges_JapanesePlusChinese_NoDuplicateCjk()
		{
			var cfg = new FontConfig
			{
				GlyphRanges = ScriptGlyphRanges.Japanese | ScriptGlyphRanges.ChineseSimplified
			};
			List<ushort> ranges = cfg.BuildRanges();
			int cjkCount = 0;
			for (int i = 0; i + 1 < ranges.Count; i += 2)
			{
				if (ranges[i] == 0x4E00 && ranges[i + 1] == 0x9FFF)
					cjkCount++;
			}
			Assert.AreEqual(1, cjkCount, "CJK base range must appear exactly once.");
		}

		[Test]
		public void BuildRanges_Custom_Null_DoesNotThrow()
		{
			var cfg = new FontConfig
			{
				GlyphRanges = ScriptGlyphRanges.Custom,
				CustomGlyphRanges = null
			};
			Assert.DoesNotThrow(() => cfg.BuildRanges());
		}
	}
}
