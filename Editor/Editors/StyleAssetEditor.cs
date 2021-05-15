using ImGuiNET;
using UImGui.Assets;
using UnityEditor;
using UnityEngine;

namespace UImGui.Editor
{
	[CustomEditor(typeof(StyleAsset))]
	internal class StyleAssetEditor : UnityEditor.Editor
	{
		private bool _showColors;

		public override void OnInspectorGUI()
		{
			StyleAsset styleAsset = target as StyleAsset;

			bool hasContext = ImGui.GetCurrentContext() != System.IntPtr.Zero;
			if (!hasContext)
			{
				EditorGUILayout.HelpBox("Can't save or apply Style.\n"
					+ "No active ImGui context.", MessageType.Warning, true);
			}

			if (hasContext)
			{
				ImGuiStylePtr style = ImGui.GetStyle();

				GUILayout.BeginHorizontal();
				if (GUILayout.Button("Apply"))
				{
					styleAsset.ApplyTo(style);
				}

				if (GUILayout.Button("Save"))
				{
					bool displayDialog = EditorUtility.DisplayDialog(
						"Save Style",
						"Do you want to save the current style to this asset?",
						"Ok", "Cancel");
					if (displayDialog)
					{
						styleAsset.SetFrom(style);
						EditorUtility.SetDirty(target);
					}
				}
				GUILayout.EndHorizontal();
			}

			DrawDefaultInspector();

			bool changed = false;
			_showColors = EditorGUILayout.Foldout(_showColors, "Colors", true);
			if (_showColors)
			{
				for (int indexColumn = 0; indexColumn < (int)ImGuiCol.COUNT; ++indexColumn)
				{
					Color indexColor = styleAsset.Colors[indexColumn];
					string colorName = ImGui.GetStyleColorName((ImGuiCol)indexColumn);
					Color newColor = EditorGUILayout.ColorField(colorName, indexColor);
					changed |= newColor != indexColor;
					styleAsset.Colors[indexColumn] = newColor;
				}
			}

			if (changed)
			{
				EditorUtility.SetDirty(target);
			}
		}
	}
}
