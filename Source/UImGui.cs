using System;
using ImGuiNET;
using UImGui.Assets;
using UImGui.Events;
using UImGui.Platform;
using UImGui.Renderer;
using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_EDITOR && HAS_URP
using UnityEditor;
using UnityEngine.Rendering.Universal;
#endif


namespace UImGui
{
	// TODO: Check Multithread run.
	public class UImGui : MonoBehaviour
	{
		private Context _context;
		private IRenderer _renderer;
		private IPlatform _platform;
		private CommandBuffer _renderCommandBuffer;

		[SerializeField]
		private Camera _camera = null;

		[SerializeField]
		private RenderImGui _renderFeature = null;

		[SerializeField]
		private RenderType _rendererType = RenderType.Mesh;

		[SerializeField]
		private InputType _platformType =
#if HAS_INPUTSYSTEM
			InputType.InputSystem;
#else
			InputType.InputManager;
#endif

		[Tooltip("Null value uses default imgui.ini file.")]
		[SerializeField]
		private IniSettingsAsset _iniSettings = null;

		[Header("Configuration")]

		[SerializeField]
		private UIOConfig _initialConfiguration = new UIOConfig
		{
			ImGuiConfig = ImGuiConfigFlags.NavEnableKeyboard | ImGuiConfigFlags.DockingEnable,

			DoubleClickTime = 0.30f,
			DoubleClickMaxDist = 6.0f,

			DragThreshold = 6.0f,

			KeyRepeatDelay = 0.250f,
			KeyRepeatRate = 0.050f,

			FontGlobalScale = 1.0f,
			FontAllowUserScaling = false,

			DisplayFramebufferScale = Vector2.one,

			MouseDrawCursor = false,
			TextCursorBlink = false,

			ResizeFromEdges = true,
			MoveFromTitleOnly = true,
			ConfigMemoryCompactTimer = 1f,
		};

		[SerializeField]
		private FontInitializerEvent _fontCustomInitializer = new FontInitializerEvent();

		[SerializeField]
		private FontAtlasConfigAsset _fontAtlasConfiguration = null;

		[Header("Customization")]
		[SerializeField]
		private ShaderResourcesAsset _shaders = null;

		[SerializeField]
		private StyleAsset _style = null;

		[SerializeField]
		private CursorShapesAsset _cursorShapes = null;

		[SerializeField]
		private bool _doGlobalEvents = true; // Do global/default Layout event too.

		private bool _isChangingCamera = false;

		public CommandBuffer CommandBuffer => _renderCommandBuffer;

		#region Events
		public event System.Action<UImGui> Layout;
		public event System.Action<UImGui> OnInitialize;
		public event System.Action<UImGui> OnDeinitialize;
		#endregion

		public void Reload()
		{
			OnDisable();
			OnEnable();
		}

		public void SetUserData(System.IntPtr userDataPtr)
		{
			_initialConfiguration.UserData = userDataPtr;
			ImGuiIOPtr io = ImGui.GetIO();
			_initialConfiguration.ApplyTo(io);
		}

		public void SetCamera(Camera camera)
		{
			if (camera == null)
			{
				enabled = false;
				throw new System.Exception($"Fail: {camera} is null.");
			}

			if(camera == _camera)
			{
				Debug.LogWarning($"Trying to change to same camera. Camera: {camera}", camera);
				return;
			}

			_camera = camera;
			_isChangingCamera = true;
		}

		private void Awake()
		{
			_context = UImGuiUtility.CreateContext();
		}

		private void OnDestroy()
		{
			UImGuiUtility.DestroyContext(_context);
		}

		private void OnEnable()
		{
			void Fail(string reason)
			{
				enabled = false;
				throw new System.Exception($"Failed to start: {reason}.");
			}

			if (_camera == null)
			{
				Fail(nameof(_camera));
			}

			if (_renderFeature == null && RenderUtility.IsUsingURP())
			{
				Fail(nameof(_renderFeature));
			}

#if UNITY_EDITOR && HAS_URP
			if (RenderUtility.IsUsingURP())
			{
				EnsureRenderFeatureRegistered();
			}
#endif

			_renderCommandBuffer = RenderUtility.GetCommandBuffer(Constants.UImGuiCommandBuffer);

			if (RenderUtility.IsUsingURP())
			{
#if HAS_URP
				_renderFeature.Camera = _camera;
				_renderFeature.UImGui = this;
#endif
				_renderFeature.CommandBuffer = _renderCommandBuffer;
			}
			else if (!RenderUtility.IsUsingHDRP())
			{
				_camera.AddCommandBuffer(CameraEvent.AfterEverything, _renderCommandBuffer);
			}

			UImGuiUtility.SetCurrentContext(_context);

			ImGuiIOPtr io = ImGui.GetIO();

			_initialConfiguration.ApplyTo(io);
			_style?.ApplyTo(ImGui.GetStyle());

			_context.TextureManager.BuildFontAtlas(io, _fontAtlasConfiguration, _fontCustomInitializer);
			_context.TextureManager.Initialize(io);

			IPlatform platform = PlatformUtility.Create(_platformType, _cursorShapes, _iniSettings);
			SetPlatform(platform, io);
			if (_platform == null)
			{
				Fail(nameof(_platform));
			}

			SetRenderer(RenderUtility.Create(_rendererType, _shaders, _context.TextureManager), io);
			if (_renderer == null)
			{
				Fail(nameof(_renderer));
			}

			if (_doGlobalEvents)
			{
				UImGuiUtility.DoOnInitialize(this);
			}
			OnInitialize?.Invoke(this);
		}

		private void OnDisable()
		{
			UImGuiUtility.SetCurrentContext(_context);
			ImGuiIOPtr io = ImGui.GetIO();

			SetRenderer(null, io);
			SetPlatform(null, io);

			UImGuiUtility.SetCurrentContext(null);

			_context.TextureManager.Shutdown();
			_context.TextureManager.DestroyFontAtlas(io);

			if (RenderUtility.IsUsingURP())
			{
				if (_renderFeature != null)
				{
#if HAS_URP
					_renderFeature.Camera = null;
					_renderFeature.UImGui = null;
#endif
					_renderFeature.CommandBuffer = null;
				}
			}
			else if(!RenderUtility.IsUsingHDRP())
			{
				if (_camera != null)
				{
					_camera.RemoveCommandBuffer(CameraEvent.AfterEverything, _renderCommandBuffer);
				}
			}

			if (_renderCommandBuffer != null)
			{
				RenderUtility.ReleaseCommandBuffer(_renderCommandBuffer);
			}

			_renderCommandBuffer = null;

			if (_doGlobalEvents)
			{
				UImGuiUtility.DoOnDeinitialize(this);
			}
			OnDeinitialize?.Invoke(this);
		}

		private void Update()
		{
			if (RenderUtility.IsUsingHDRP())
				return; // skip update call in hdrp
			DoUpdate(this.CommandBuffer);
		}

		internal void DoUpdate(CommandBuffer buffer)
		{
			UImGuiUtility.SetCurrentContext(_context);
			ImGuiIOPtr io = ImGui.GetIO();

			Constants.PrepareFrameMarker.Begin(this);
			_context.TextureManager.PrepareFrame(io);
			if (!_context.TextureManager.HasValidAtlas)
			{
				Debug.LogError("[UImGui] Skipping frame because the font atlas texture is invalid.");
				Constants.PrepareFrameMarker.End();
				return;
			}
			_platform.PrepareFrame(io, _camera.pixelRect);
			ImGui.NewFrame();
			Constants.PrepareFrameMarker.End();

			Constants.LayoutMarker.Begin(this);
			try
			{
				if (_doGlobalEvents)
				{
					UImGuiUtility.DoLayout(this);
				}

				Layout?.Invoke(this);
			}
			finally
			{
				ImGui.Render();
				Constants.LayoutMarker.End();
			}

			Constants.DrawListMarker.Begin(this);
			if (buffer == _renderCommandBuffer)
			{
				_renderCommandBuffer.Clear();
			}
			_renderer.RenderDrawLists(buffer, ImGui.GetDrawData());
			Constants.DrawListMarker.End();

			if (_isChangingCamera)
			{
				_isChangingCamera = false;
				Reload();
			}
		}

		private void SetRenderer(IRenderer renderer, ImGuiIOPtr io)
		{
			_renderer?.Shutdown(io);
			_renderer = renderer;
			_renderer?.Initialize(io);
		}

		private void SetPlatform(IPlatform platform, ImGuiIOPtr io)
		{
			_platform?.Shutdown(io);
			_platform = platform;
			_platform?.Initialize(io, _initialConfiguration, "Unity " + _platformType.ToString());
		}

#if UNITY_EDITOR && HAS_URP
		private void EnsureRenderFeatureRegistered()
		{
			if (_renderFeature == null)
			{
				return;
			}

			if (GraphicsSettings.currentRenderPipeline is not UniversalRenderPipelineAsset pipeline)
			{
				return;
			}

			var pipelineObject = new SerializedObject(pipeline);
			var rendererDataList = pipelineObject.FindProperty("m_RendererDataList");
			if (rendererDataList == null)
			{
				return;
			}

			for (var i = 0; i < rendererDataList.arraySize; i++)
			{
				if (rendererDataList.GetArrayElementAtIndex(i).objectReferenceValue is not UniversalRendererData rendererData)
				{
					continue;
				}

				var features = rendererData.rendererFeatures;
				var changed = false;
				var found = false;

				for (var featureIndex = 0; featureIndex < features.Count; featureIndex++)
				{
					if (features[featureIndex] is not RenderImGui)
					{
						continue;
					}

					found = true;
					if (features[featureIndex] != _renderFeature)
					{
						features[featureIndex] = _renderFeature;
						changed = true;
					}
				}

				if (!found)
				{
					features.Add(_renderFeature);
					changed = true;
				}

				if (changed)
				{
					EditorUtility.SetDirty(rendererData);
					AssetDatabase.SaveAssetIfDirty(rendererData);
				}
			}
		}
#endif
	}
}
