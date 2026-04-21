using ImGuiNET;
using System;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using Num = System.Numerics;

namespace UImGui
{
	internal static unsafe class Utils
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static Vector2 ScreenToImGui(in Vector2 point)
		{
			var displaySize = ImGui.GetIO().DisplaySize;
			return new Vector2(point.x, displaySize.Y - point.y);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static Num.Vector2 ScreenToImGuiNumerics(in Vector2 point)
		{
			var imguiPoint = ScreenToImGui(point);
			return imguiPoint.AsNumerics();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static Vector2 ImGuiToScreen(in Num.Vector2 point)
		{
			var localPoint = point;
			var unityPoint = localPoint.AsUnity();
			return ImGuiToScreen(unityPoint);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static Vector2 ImGuiToScreen(in Vector2 point)
		{
			var displaySize = ImGui.GetIO().DisplaySize;
			return new Vector2(point.x, displaySize.Y - point.y);
		}

		internal static string StringFromPtr(byte* ptr)
		{
			int characters = 0;
			while (ptr[characters] != 0)
			{
				characters++;
			}

			return Encoding.UTF8.GetString(ptr, characters);
		}

		internal static int GetUtf8(string text, byte* utf8Bytes, int utf8ByteCount)
		{
			fixed (char* utf16Ptr = text)
			{
				return Encoding.UTF8.GetBytes(utf16Ptr, text.Length, utf8Bytes, utf8ByteCount);
			}
		}

		internal static int GetUtf8(string text, int start, int length, byte* utf8Bytes, int utf8ByteCount)
		{
			if (start < 0 || length < 0 || start + length > text.Length)
			{
				throw new ArgumentOutOfRangeException();
			}

			fixed (char* utf16Ptr = text)
			{
				return Encoding.UTF8.GetBytes(utf16Ptr + start, length, utf8Bytes, utf8ByteCount);
			}
		}
	}
}
