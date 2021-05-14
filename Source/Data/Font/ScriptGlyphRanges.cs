using System;

namespace UImGui
{
	[Flags]
	internal enum ScriptGlyphRanges
	{
		Default = 1 << 0,
		Cyrillic = 1 << 1,
		Japanese = 1 << 2,
		Korean = 1 << 3,
		Thai = 1 << 4,
		Vietnamese = 1 << 5,
		ChineseSimplified = 1 << 6,
		ChineseFull = 1 << 7,
		Custom = 1 << 8,
	}
}
