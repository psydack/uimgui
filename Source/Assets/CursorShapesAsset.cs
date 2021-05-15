using ImGuiNET;
using System;
using UnityEngine;

namespace UImGui.Assets
{
	// TODO: Implement animated cursor.
	[CreateAssetMenu(menuName = "Dear ImGui/Cursor Shapes")]
	internal sealed class CursorShapesAsset : ScriptableObject
	{
		[Serializable]
		internal struct CursorShape
		{
			public Texture2D Texture;
			public Vector2 Hotspot;
		}

		[Tooltip("Default.")]
		public CursorShape Arrow;

		[Tooltip("When hovering over InputText, etc.")]
		public CursorShape TextInput;

		[Tooltip("(Unused by ImGui functions)")]
		public CursorShape ResizeAll;

		[Tooltip("When hovering over an horizontal border")]
		public CursorShape ResizeNS;

		[Tooltip("When hovering over a vertical border or a column")]
		public CursorShape ResizeEW;

		[Tooltip("When hovering over the bottom-left corner of a window")]
		public CursorShape ResizeNESW;

		[Tooltip("When hovering over the bottom-right corner of a window")]
		public CursorShape ResizeNWSE;

		[Tooltip("(Unused by ImGui functions. Use for e.g. hyperlinks)")]
		public CursorShape Hand;

		[Tooltip("When hovering something with disabled interaction. Usually a crossed circle.")]
		public CursorShape NotAllowed;

		public ref CursorShape this[ImGuiMouseCursor cursor]
		{
			get
			{
				switch (cursor)
				{
					case ImGuiMouseCursor.Arrow: return ref Arrow;
					case ImGuiMouseCursor.TextInput: return ref TextInput;
					case ImGuiMouseCursor.ResizeAll: return ref ResizeAll;
					case ImGuiMouseCursor.ResizeEW: return ref ResizeEW;
					case ImGuiMouseCursor.ResizeNS: return ref ResizeNS;
					case ImGuiMouseCursor.ResizeNESW: return ref ResizeNESW;
					case ImGuiMouseCursor.ResizeNWSE: return ref ResizeNWSE;
					case ImGuiMouseCursor.Hand: return ref Hand;
					case ImGuiMouseCursor.NotAllowed: return ref NotAllowed;
					default: return ref Arrow;
				}
			}
		}
	}
}
