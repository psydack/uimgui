using ImGuiNET;
using System.Text;
using UImGui.Platform;
using UImGui.Renderer;
using UnityEditor;
using UnityEngine;

namespace UImGui.Editor
{
	[CustomEditor(typeof(UImGui))]
	internal class UImGuiEditor : UnityEditor.Editor
	{
		private SerializedProperty _doGlobalEvents;
		private SerializedProperty _camera;
		private SerializedProperty _renderFeature;
		private SerializedProperty _renderer;
		private SerializedProperty _platform;
		private SerializedProperty _initialConfiguration;
		private SerializedProperty _fontAtlasConfiguration;
		private SerializedProperty _fontCustomInitializer;
		private SerializedProperty _iniSettings;
		private SerializedProperty _shaders;
		private SerializedProperty _style;
		private SerializedProperty _cursorShapes;
		private readonly StringBuilder _messages = new StringBuilder();

		private bool usingImNodes = true;
		private bool usingImGuizmo = true;
		private bool usingImPlot = true;

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			CheckRequirements();

			EditorGUI.BeginChangeCheck();

			EditorGUILayout.PropertyField(_doGlobalEvents);
			if (RenderUtility.IsUsingURP())
			{
				EditorGUILayout.PropertyField(_renderFeature);
			}

			EditorGUILayout.PropertyField(_camera);
			EditorGUILayout.PropertyField(_renderer);
			EditorGUILayout.PropertyField(_platform);
			EditorGUILayout.PropertyField(_initialConfiguration);
			EditorGUILayout.PropertyField(_fontAtlasConfiguration);
			EditorGUILayout.PropertyField(_fontCustomInitializer);
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
			_doGlobalEvents = serializedObject.FindProperty("_doGlobalEvents");
			_camera = serializedObject.FindProperty("_camera");
			_renderFeature = serializedObject.FindProperty("_renderFeature");
			_renderer = serializedObject.FindProperty("_rendererType");
			_platform = serializedObject.FindProperty("_platformType");
			_initialConfiguration = serializedObject.FindProperty("_initialConfiguration");
			_fontAtlasConfiguration = serializedObject.FindProperty("_fontAtlasConfiguration");
			_fontCustomInitializer = serializedObject.FindProperty("_fontCustomInitializer");
			_iniSettings = serializedObject.FindProperty("_iniSettings");
			_shaders = serializedObject.FindProperty("_shaders");
			_style = serializedObject.FindProperty("_style");
			_cursorShapes = serializedObject.FindProperty("_cursorShapes");

#if UIMGUI_REMOVE_IMNODES
			usingImNodes = false;
#endif
#if UIMGUI_REMOVE_IMGUIZMO
			usingImGuizmo = false;
#endif
#if UIMGUI_REMOVE_IMPLOT
			usingImPlot = false;
#endif
		}

		private void CheckRequirements()
		{
			var textImGui = $"ImGUI: {ImGui.GetVersion()}";
			var textImNodes = $"ImNodes: { (usingImNodes ? "0.4 - 2021-07-09" : "disabled") }";
			var textImGuizmo = $"ImGuizmo: { (usingImGuizmo ? "?? - 2021-07-09" : "disabled") }";
			var textImPlot = $"ImPlot: { (usingImPlot ? "0.10 - 2021-07-09" : "disabled") }";

			EditorGUILayout.LabelField(textImGui);
			EditorGUILayout.LabelField(textImNodes);
			EditorGUILayout.LabelField(textImGuizmo);
			EditorGUILayout.LabelField(textImPlot);
			EditorGUILayout.Space();

			_messages.Clear();
			if (_camera.objectReferenceValue == null)
			{
				_messages.AppendLine("Must assign a Camera.");
			}

			if (RenderUtility.IsUsingURP() && _renderFeature.objectReferenceValue == null)
			{
				_messages.AppendLine("Must assign a RenderFeature when using the URP.");
			}

#if !UNITY_2020_1_OR_NEWER
			if ((RenderType)_renderer.enumValueIndex == RenderType.Mesh)
			{
				_messages.AppendLine("Use procedural.");
			}
#endif

			SerializedProperty configFlags = _initialConfiguration.FindPropertyRelative("ImGuiConfig");
			if (!PlatformUtility.IsAvailable((InputType)_platform.enumValueIndex))
			{
				_messages.AppendLine("Platform not available.");
			}
			else if ((InputType)_platform.enumValueIndex != InputType.InputSystem &&
				(configFlags.intValue & (int)ImGuiConfigFlags.NavEnableSetMousePos) != 0)
			{
				_messages.AppendLine("Will not work NavEnableSetPos with InputManager.");
			}

			if ((configFlags.intValue & (int)ImGuiConfigFlags.ViewportsEnable) != 0)
			{
				_messages.AppendLine("Unity hasn't support different viewports.");
			}

			if (_shaders.objectReferenceValue == null || _style.objectReferenceValue == null)
			{
				_messages.AppendLine("Must assign a Shader Asset and a Style Asset in configuration section.");
			}

			if (_messages.Length > 0)
			{
				EditorGUILayout.HelpBox(_messages.ToString(), MessageType.Error);
			}
		}
	}
}
