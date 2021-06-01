using AOT;
using ImGuiNET;
using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace UImGui.Platform
{
	#region Callback methods
	// TODO: Should return Utf8 byte*, how to deal with memory ownership?
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal unsafe delegate string GetClipboardTextCallback(void* user_data);
	internal delegate string GetClipboardTextSafeCallback(IntPtr user_data);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal unsafe delegate void SetClipboardTextCallback(void* user_data, byte* text);
	internal delegate void SetClipboardTextSafeCallback(IntPtr user_data, string text);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void ImeSetInputScreenPosCallback(int x, int y);
	#endregion

	internal unsafe class PlatformCallbacks
	{
		private static GetClipboardTextCallback _getClipboardText;
		private static SetClipboardTextCallback _setClipboardText;

		[MonoPInvokeCallback(typeof(GetClipboardTextCallback))]
		public static unsafe string GetClipboardTextCallback(void* user_data)
		{
			return GUIUtility.systemCopyBuffer;
		}

		[MonoPInvokeCallback(typeof(SetClipboardTextCallback))]
		public static unsafe void SetClipboardTextCallback(void* user_data, byte* text)
		{
			GUIUtility.systemCopyBuffer = Utils.StringFromPtr(text);
		}

		[MonoPInvokeCallback(typeof(ImeSetInputScreenPosCallback))]
		public static unsafe void ImeSetInputScreenPosCallback(int x, int y)
		{
			Input.compositionCursorPos = new Vector2(x, y);
		}

		public static void SetClipboardFunctions(GetClipboardTextCallback getCb, SetClipboardTextCallback setCb)
		{
			_getClipboardText = getCb;
			_setClipboardText = setCb;
		}

		public void Assign(ImGuiIOPtr io)
		{
			io.SetClipboardTextFn = Marshal.GetFunctionPointerForDelegate(_setClipboardText);
			io.GetClipboardTextFn = Marshal.GetFunctionPointerForDelegate(_getClipboardText);
		}

		public void Unset(ImGuiIOPtr io)
		{
			io.SetClipboardTextFn = IntPtr.Zero;
			io.GetClipboardTextFn = IntPtr.Zero;
		}

		public static GetClipboardTextSafeCallback GetClipboardText
		{
			// TODO: convert return string to Utf8 byte*
			set => _getClipboardText = (user_data) =>
			{
				try { return value(new IntPtr(user_data)); }
				catch (Exception ex) { Debug.LogException(ex); return null; }
			};
		}

		public static SetClipboardTextSafeCallback SetClipboardText
		{
			set => _setClipboardText = (user_data, text) =>
			{
				try { value(new IntPtr(user_data), Utils.StringFromPtr(text)); }
				catch (Exception ex) { Debug.LogException(ex); }
			};
		}
	}
}