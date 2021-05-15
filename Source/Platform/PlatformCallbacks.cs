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

	// TODO: Implement (check) this.
#if IMGUI_FEATURE_CUSTOM_ASSERT
	//[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	//internal unsafe delegate void LogAssertCallback(byte* condition, byte* file, int line);

	//internal delegate void LogAssertSafeCallback(string condition, string file, int line);

	//[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	//internal delegate void DebugBreakCallback();

	//internal unsafe struct CustomAssertData
	//{
	//	public IntPtr LogAssertFn;
	//	public IntPtr DebugBreakFn;
	//}
#endif
	#endregion

	internal unsafe class PlatformCallbacks
	{
		private static GetClipboardTextCallback _getClipboardText;
		private static SetClipboardTextCallback _setClipboardText;

		// TODO: Implement (check) this.
#if IMGUI_FEATURE_CUSTOM_ASSERT
		//private static LogAssertCallback _logAssert;
		//private static DebugBreakCallback _debugBreak;
#endif

		public static void SetClipboardFunctions(GetClipboardTextCallback getCb, SetClipboardTextCallback setCb)
		{
			_getClipboardText = getCb;
			_setClipboardText = setCb;
		}

		// TODO: Implement (check) this.
#if IMGUI_FEATURE_CUSTOM_ASSERT
		//public static void SetClipboardFunctions(GetClipboardTextCallback getCb, SetClipboardTextCallback setCb,
		//	LogAssertCallback logCb, DebugBreakCallback debugBreakCb)
		//{
		//	_getClipboardText = getCb;
		//	_setClipboardText = setCb;
		//	_logAssert = logCb;
		//	_debugBreak = debugBreakCb;
		//}
#endif
		public void Assign(ImGuiIOPtr io)
		{
			io.SetClipboardTextFn = Marshal.GetFunctionPointerForDelegate(_setClipboardText);
			io.GetClipboardTextFn = Marshal.GetFunctionPointerForDelegate(_getClipboardText);

			// TODO: Implement (check) this.
#if IMGUI_FEATURE_CUSTOM_ASSERT
			//io.SetBackendPlatformUserData<CustomAssertData>(new CustomAssertData
			//{
			//	LogAssertFn = Marshal.GetFunctionPointerForDelegate(_logAssert),
			//	DebugBreakFn = Marshal.GetFunctionPointerForDelegate(_debugBreak),
			//});
#endif
		}

		public void Unset(ImGuiIOPtr io)
		{
			io.SetClipboardTextFn = IntPtr.Zero;
			io.GetClipboardTextFn = IntPtr.Zero;

			// TODO: Implement (check) this.
#if IMGUI_FEATURE_CUSTOM_ASSERT
			//io.SetBackendPlatformUserData<CustomAssertData>(null);
#endif
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

		// TODO: Implement (check) this.
#if IMGUI_FEATURE_CUSTOM_ASSERT
		//public static LogAssertSafeCallback LogAssert
		//{
		//	set => _logAssert = (condition, file, line) =>
		//	{
		//		try { value(Util.StringFromPtr(condition), Util.StringFromPtr(file), line); }
		//		catch (Exception ex) { Debug.LogException(ex); }
		//	};
		//}

		//public static DebugBreakCallback DebugBreak
		//{
		//	set => _debugBreak = () =>
		//	{
		//		try { value(); }
		//		catch (Exception ex) { Debug.LogException(ex); }
		//	};
		//}
#endif
	}
}