using System;
using System.Runtime.InteropServices;

namespace UImGui
{
	public static unsafe partial class ImFreetype
	{
		// Hinting greatly impacts visuals (and glyph sizes).
		// - By default, hinting is enabled and the font's native hinter is preferred over the auto-hinter.
		// - When disabled, FreeType generates blurrier glyphs, more or less matches the stb_truetype.h
		// - The Default hinting mode usually looks good, but may distort glyphs in an unusual way.
		// - The Light hinting mode generates fuzzier glyphs but better matches Microsoft's rasterizer.
		// You can set those flags globaly in ImFontAtlas::FontBuilderFlags
		// You can set those flags on a per font basis in ImFontConfig::FontBuilderFlags
		internal enum BuilderFlags // RasterizerFlags
		{
			NoHinting = 1 << 0,   // Disable hinting. This generally generates 'blurrier' bitmap glyphs when the glyph are rendered in any of the anti-aliased modes.
			NoAutoHint = 1 << 1,   // Disable auto-hinter.
			ForceAutoHint = 1 << 2,   // Indicates that the auto-hinter is preferred over the font's native hinter.
			LightHinting = 1 << 3,   // A lighter hinting algorithm for gray-level modes. Many generated glyphs are fuzzier but better resemble their original shape. This is achieved by snapping glyphs to the pixel grid only vertically (Y-axis), as is done by Microsoft's ClearType and Adobe's proprietary font renderer. This preserves inter-glyph spacing in horizontal text.
			MonoHinting = 1 << 4,   // Strong hinting algorithm that should only be used for monochrome output.
			Bold = 1 << 5,   // Styling: Should we artificially embolden the font?
			Oblique = 1 << 6,   // Styling: Should we slant the font, emulating italic style?
			Monochrome = 1 << 7,   // Disable anti-aliasing. Combine this with MonoHinting for best results!
			LoadColor = 1 << 8,   // Enable FreeType color-layered glyphs
			Bitmap = 1 << 9    // Enable FreeType bitmap glyphs
		};

		// This is automatically assigned when using '#define IMGUI_ENABLE_FREETYPE'.
		// If you need to dynamically select between multiple builders:
		// - you can manually assign this builder with 'atlas->FontBuilderIO = ImGuiFreeType::GetBuilderForFreeType()'
		// - prefer deep-copying this into your own ImFontBuilderIO instance if you use hot-reloading that messes up static data.
		public static IntPtr GetBuilderForFreeType()
		{
			return ImFreetypeNative.GetBuilderForFreeType();
		}

		// Override allocators. By default ImGuiFreeType will use IM_ALLOC()/IM_FREE()
		// However, as FreeType does lots of allocations we provide a way for the user to redirect it to a separate memory heap if desired.
		public delegate void FreeType_Alloc(uint sz, IntPtr userData);
		public delegate void FreeType_Free(IntPtr ptr, IntPtr userData);

		public static void SetAllocatorFunctions(FreeType_Alloc alloc_function, FreeType_Free free_function, IntPtr user_data = default)
		{
			ImFreetypeNative.SetAllocatorFunctions(
				Marshal.GetFunctionPointerForDelegate(alloc_function),
				Marshal.GetFunctionPointerForDelegate(free_function),
				user_data
			);
		}
	}
}