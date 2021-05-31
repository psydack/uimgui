using UnityEditor;
using UnityEngine;

namespace UImGui.Editor
{
	[CustomPropertyDrawer(typeof(FontDefinition))]
	internal class FontDefinitionDrawer : PropertyDrawer
	{
		private const string EditorStreamingAssetsPath = "Assets/StreamingAssets/";

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			SerializedProperty fontPath = property.FindPropertyRelative(nameof(FontDefinition.Path));
			SerializedProperty config = property.FindPropertyRelative(nameof(FontDefinition.Config));

			float height = EditorGUIUtility.singleLineHeight; // font file asset.
			height += EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight; // path

			if (string.IsNullOrEmpty(fontPath.stringValue))
			{
				height += EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight;
			}
			else
			{
				height += EditorGUIUtility.standardVerticalSpacing + EditorGUI.GetPropertyHeight(config);
			}

			return height;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			SerializedProperty fontAsset = property.FindPropertyRelative("_fontAsset");
			SerializedProperty fontPath = property.FindPropertyRelative(nameof(FontDefinition.Path));
			SerializedProperty config = property.FindPropertyRelative(nameof(FontDefinition.Config));

			position.height = EditorGUIUtility.singleLineHeight;
			EditorGUI.PropertyField(position, fontAsset);
			fontPath.stringValue = GetStreamingAssetPath(fontAsset);

			if (string.IsNullOrEmpty(fontPath.stringValue))
			{
				position.y = position.yMax + EditorGUIUtility.standardVerticalSpacing;
				position.height = EditorGUIUtility.singleLineHeight * 2;
				EditorGUI.HelpBox(position, $"Font file must be in '{EditorStreamingAssetsPath}' folder.", MessageType.Error);
			}
			else
			{
				position.y = position.yMax + EditorGUIUtility.standardVerticalSpacing;
				position.height = EditorGUIUtility.singleLineHeight;

				EditorGUI.BeginDisabledGroup(true);
				Rect fieldPos = EditorGUI.PrefixLabel(position, new GUIContent(EditorStreamingAssetsPath));
				EditorGUI.LabelField(fieldPos, fontPath.stringValue);
				EditorGUI.EndDisabledGroup();

				position.y = position.yMax + EditorGUIUtility.standardVerticalSpacing;
				position.height = EditorGUI.GetPropertyHeight(config, config.isExpanded);
				EditorGUI.PropertyField(position, config, config.isExpanded);
			}
		}

		private string GetStreamingAssetPath(SerializedProperty property)
		{
			string path = property.objectReferenceValue != null ?
				AssetDatabase.GetAssetPath(property.objectReferenceValue.GetInstanceID()) :
				string.Empty;
			return path.StartsWith(EditorStreamingAssetsPath) ? path.Substring(EditorStreamingAssetsPath.Length) : string.Empty;
		}
	}
}
