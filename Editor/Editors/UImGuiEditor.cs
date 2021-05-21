using System.Text;
using UImGui.Platform;
using UnityEditor;
using UnityEngine;

namespace UImGui.Editor
{
	[CustomEditor(typeof(UImGui))]
	internal class UImGuiEditor : UnityEditor.Editor
	{
		private SerializedProperty _doGlobalLayout;
		private SerializedProperty _camera;
		private SerializedProperty _renderFeature;
		private SerializedProperty _renderer;
		private SerializedProperty _platform;
		private SerializedProperty _initialConfiguration;
		private SerializedProperty _fontAtlasConfiguration;
		private SerializedProperty _iniSettings;
		private SerializedProperty _shaders;
		private SerializedProperty _style;
		private SerializedProperty _cursorShapes;
		private readonly StringBuilder _messages = new StringBuilder();

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			CheckRequirements();

			EditorGUI.BeginChangeCheck();

			EditorGUILayout.PropertyField(_doGlobalLayout);
			if (RenderUtility.IsUsingURP())
			{
				EditorGUILayout.PropertyField(_renderFeature);
			}

			EditorGUILayout.PropertyField(_camera);
			EditorGUILayout.PropertyField(_renderer);
			EditorGUILayout.PropertyField(_platform);
			EditorGUILayout.PropertyField(_initialConfiguration);
			EditorGUILayout.PropertyField(_fontAtlasConfiguration);
			EditorGUILayout.PropertyField(_iniSettings);
			EditorGUILayout.PropertyField(_shaders);
			EditorGUILayout.PropertyField(_style);
			EditorGUILayout.PropertyField(_cursorShapes);

			bool changed = EditorGUI.EndChangeCheck();
			if (changed)
			{
				serializedObject.ApplyModifiedProperties();
			}

			if (!Application.isPlaying) return;

			bool reload = GUILayout.Button("Reload");
			if (changed || reload)
			{
				(target as UImGui)?.Reload();
			}
		}

		private void OnEnable()
		{
			_doGlobalLayout = serializedObject.FindProperty("_doGlobalLayout");
			_camera = serializedObject.FindProperty("_camera");
			_renderFeature = serializedObject.FindProperty("_renderFeature");
			_renderer = serializedObject.FindProperty("_rendererType");
			_platform = serializedObject.FindProperty("_platformType");
			_initialConfiguration = serializedObject.FindProperty("_initialConfiguration");
			_fontAtlasConfiguration = serializedObject.FindProperty("_fontAtlasConfiguration");
			_iniSettings = serializedObject.FindProperty("_iniSettings");
			_shaders = serializedObject.FindProperty("_shaders");
			_style = serializedObject.FindProperty("_style");
			_cursorShapes = serializedObject.FindProperty("_cursorShapes");
		}

		private void CheckRequirements()
		{
			_messages.Clear();
			if (_camera.objectReferenceValue == null)
			{
				_messages.AppendLine("Must assign a Camera.");
			}

			if (RenderUtility.IsUsingURP() && _renderFeature.objectReferenceValue == null)
			{
				_messages.AppendLine("Must assign a RenderFeature when using the URP.");
			}

			if (!PlatformUtility.IsAvailable((InputType)_platform.enumValueIndex))
			{
				_messages.AppendLine("Platform not available.");
			}
			// TODO: Warning for if(UIOConfig.ImGuiConfig has NavEnableSetMousePos and Input != InputSystem) wont work.
			//else if ((InputType)_platform.enumValueIndex == InputType.InputSystem && (UIOConfig)_initialConfiguration.)
			//{
			//	_messages.AppendLine("Will not work NavEnableSetPos with InputManager");
			//}

			if (_messages.Length > 0)
			{
				EditorGUILayout.HelpBox(_messages.ToString(), MessageType.Error);
			}
		}
	}
}
