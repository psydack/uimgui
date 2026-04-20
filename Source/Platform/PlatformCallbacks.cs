using AOT;
using ImGuiNET;
using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace UImGui.Platform
{
	#region Callback methods
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal unsafe delegate string GetClipboardTextCallback(void* userData);
	internal delegate string GetClipboardTextSafeCallback(IntPtr userData);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal unsafe delegate void SetClipboardTextCallback(void* userData, byte* text);
	internal delegate void SetClipboardTextSafeCallback(IntPtr userData, string text);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void ImeSetInputScreenPosCallback(int x, int y);
	#endregion

	internal unsafe class PlatformCallbacks
	{
		private static GetClipboardTextCallback _getClipboardText;
		private static SetClipboardTextCallback _setClipboardText;

		[MonoPInvokeCallback(typeof(GetClipboardTextCallback))]
		public static unsafe string GetClipboardTextCallback(void* userData)
		{
			return GUIUtility.systemCopyBuffer;
		}

		[MonoPInvokeCallback(typeof(SetClipboardTextCallback))]
		public static unsafe void SetClipboardTextCallback(void* userData, byte* text)
		{
			GUIUtility.systemCopyBuffer = Utils.StringFromPtr(text);
		}

		[MonoPInvokeCallback(typeof(ImeSetInputScreenPosCallback))]
		public static unsafe void ImeSetInputScreenPosCallback(int x, int y)
		{
			Input.compositionCursorPos = new Vector2(x, y);
		}

		public static void SetClipboardFunctions(GetClipboardTextCallback getCallback, SetClipboardTextCallback setCallback)
		{
			_getClipboardText = getCallback;
			_setClipboardText = setCallback;
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
			set => _getClipboardText = (userData) =>
			{
				try { return value(new IntPtr(userData)); }
				catch (Exception ex) { Debug.LogException(ex); return null; }
			};
		}

		public static SetClipboardTextSafeCallback SetClipboardText
		{
			set => _setClipboardText = (userData, text) =>
			{
				try { value(new IntPtr(userData), Utils.StringFromPtr(text)); }
				catch (Exception ex) { Debug.LogException(ex); }
			};
		}
	}
}