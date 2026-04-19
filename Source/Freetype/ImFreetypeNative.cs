using System;
using System.Runtime.InteropServices;

namespace UImGui
{
	internal static unsafe partial class ImFreetypeNative
	{
		[DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr GetBuilderForFreeType();

		[DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
		public static extern void SetAllocatorFunctions(IntPtr allocFunc, IntPtr freeFunc, IntPtr userData);
	}
}