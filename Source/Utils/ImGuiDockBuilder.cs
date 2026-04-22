using ImGuiNET;
using System.Runtime.InteropServices;
using UnityEngine;

namespace UImGui
{
	public static unsafe class ImGuiDockBuilder
	{
		[DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
		private static extern uint igDockBuilderAddNode(uint nodeId, ImGuiDockNodeFlags flags);

		[DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
		private static extern void igDockBuilderRemoveNode(uint nodeId);

		[DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
		private static extern void igDockBuilderRemoveNodeChildNodes(uint nodeId);

		[DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
		private static extern void igDockBuilderRemoveNodeDockedWindows(uint nodeId, bool clearSettingsRefs);

		[DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
		private static extern void igDockBuilderSetNodePos(uint nodeId, System.Numerics.Vector2 pos);

		[DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
		private static extern void igDockBuilderSetNodeSize(uint nodeId, System.Numerics.Vector2 size);

		[DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
		private static extern uint igDockBuilderSplitNode(uint nodeId, ImGuiDir splitDir, float sizeRatioForDir, uint* outIdAtDir, uint* outIdAtOppositeDir);

		[DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
		private static extern void igDockBuilderDockWindow(byte* windowName, uint nodeId);

		[DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
		private static extern void igDockBuilderFinish(uint nodeId);

		public static uint AddNode(uint nodeId = 0, ImGuiDockNodeFlags flags = ImGuiDockNodeFlags.None)
			=> igDockBuilderAddNode(nodeId, flags);

		public static void RemoveNode(uint nodeId)
			=> igDockBuilderRemoveNode(nodeId);

		public static void RemoveNodeChildNodes(uint nodeId)
			=> igDockBuilderRemoveNodeChildNodes(nodeId);

		public static void RemoveNodeDockedWindows(uint nodeId, bool clearSettingsRefs = true)
			=> igDockBuilderRemoveNodeDockedWindows(nodeId, clearSettingsRefs);

		public static void SetNodePos(uint nodeId, Vector2 pos)
			=> igDockBuilderSetNodePos(nodeId, pos.AsNumerics());

		public static void SetNodeSize(uint nodeId, Vector2 size)
			=> igDockBuilderSetNodeSize(nodeId, size.AsNumerics());

		public static void SetNodePosSize(uint nodeId, Vector2 pos, Vector2 size)
		{
			SetNodePos(nodeId, pos);
			SetNodeSize(nodeId, size);
		}

		public static uint SplitNode(uint nodeId, ImGuiDir splitDir, float sizeRatioForDir, out uint outIdAtDir, out uint outIdAtOppositeDir)
		{
			uint idAtDir, idAtOppositeDir;
			uint result = igDockBuilderSplitNode(nodeId, splitDir, sizeRatioForDir, &idAtDir, &idAtOppositeDir);
			outIdAtDir = idAtDir;
			outIdAtOppositeDir = idAtOppositeDir;
			return result;
		}

		public static void DockWindow(string windowName, uint nodeId)
		{
			int byteCount = System.Text.Encoding.UTF8.GetByteCount(windowName);
			byte* nativeName = stackalloc byte[byteCount + 1];
			Utils.GetUtf8(windowName, nativeName, byteCount);
			nativeName[byteCount] = 0;
			igDockBuilderDockWindow(nativeName, nodeId);
		}

		public static void Finish(uint nodeId)
			=> igDockBuilderFinish(nodeId);
	}
}
