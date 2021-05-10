using ImGuiNET;
using UImGui.Platform;
using UImGui.Renderer;
using UnityEngine;
using UnityEngine.Rendering;

namespace UImGui
{

	public class UImGui : MonoBehaviour
	{
		private Context _context;
		private IImGuiRenderer _renderer;
		private IImGuiPlatform _platform;
		private CommandBuffer _renderCommandBuffer;
		private bool _usingURP;

		//public event System.Action Layout;  // Layout event for *this* ImGui instance
		//[SerializeField] private bool _doGlobalLayout = true; // do global/default Layout event too

		[SerializeField]
		private Camera _camera = null;

		[SerializeField]
		private RenderUImGui _renderFeature = null;

		//[SerializeField] private RenderUtils.RenderType _rendererType = RenderUtils.RenderType.Mesh;
		//[SerializeField] private Platform.Type _platformType = Platform.Type.InputManager;

		[Header("Configuration")]

		[SerializeField]
		private IOConfig _initialConfiguration = default;
		//[SerializeField] private FontAtlasConfigAsset _fontAtlasConfiguration = null;
		//[SerializeField] private IniSettingsAsset _iniSettings = null;  // null: uses default imgui.ini file

		//[Header("Customization")]
		//[SerializeField] private ShaderResourcesAsset _shaders = null;
		//[SerializeField] private StyleAsset _style = null;
		//[SerializeField] private CursorShapesAsset _cursorShapes = null;
		//private static readonly ProfilerMarker s_prepareFramePerfMarker = new ProfilerMarker("DearImGui.PrepareFrame");
		//private static readonly ProfilerMarker s_layoutPerfMarker = new ProfilerMarker("DearImGui.Layout");
		//private static readonly ProfilerMarker s_drawListPerfMarker = new ProfilerMarker("DearImGui.RenderDrawLists");

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
			_usingURP = RenderUtils.IsUsingURP();

			if (_camera == null)
			{
				Fail(nameof(_camera));
				return;
			}

			if (_renderFeature == null && _usingURP)
			{
				Fail(nameof(_renderFeature));
				return;
			}

			_renderCommandBuffer = RenderUtils.GetCommandBuffer("UImGui");

			if (_usingURP)
			{
				_renderFeature.CommandBuffer = _renderCommandBuffer;
			}
			else
			{
				_camera.AddCommandBuffer(CameraEvent.AfterEverything, _renderCommandBuffer);
			}

			UImGuiUtility.SetCurrentContext(_context);

			ImGuiIOPtr io = ImGui.GetIO();

			_initialConfiguration.ApplyTo(io);
			//_style?.ApplyTo(ImGui.GetStyle());

			//_context.textures.BuildFontAtlas(io, _fontAtlasConfiguration);
			//_context.textures.Initialize(io);

			//SetPlatform(Platform.Create(_platformType, _cursorShapes, _iniSettings), io);
			//SetRenderer(RenderUtils.Create(_rendererType, _shaders, _context.textures), io);
			//if (_platform == null) Fail(nameof(_platform));
			//if (_renderer == null) Fail(nameof(_renderer));

			void Fail(string reason)
			{
				enabled = false;
				throw new System.Exception($"Failed to start: {reason}");
			}
		}

		private void OnDisable()
		{
			//ImGuiUn.SetUnityContext(_context);
			//ImGuiIOPtr io = ImGui.GetIO();

			//SetRenderer(null, io);
			//SetPlatform(null, io);

			//ImGuiUn.SetUnityContext(null);

			//_context.textures.Shutdown();
			//_context.textures.DestroyFontAtlas(io);

			//if (_usingURP)
			//{
			//	if (_renderFeature != null)
			//		_renderFeature.commandBuffer = null;
			//}
			//else
			//{
			//	if (_camera != null)
			//		_camera.RemoveCommandBuffer(CameraEvent.AfterEverything, _cmd);
			//}

			//if (_cmd != null)
			//	RenderUtils.ReleaseCommandBuffer(_cmd);
			//_cmd = null;
		}

		private void Reset()
		{
			//_camera = Camera.main;
			//_initialConfiguration.SetDefaults();
		}

		private void Update()
		{
			//ImGuiUn.SetUnityContext(_context);
			//ImGuiIOPtr io = ImGui.GetIO();

			//s_prepareFramePerfMarker.Begin(this);
			//_context.textures.PrepareFrame(io);
			//_platform.PrepareFrame(io, _camera.pixelRect);
			//ImGui.NewFrame();
			//s_prepareFramePerfMarker.End();

			//s_layoutPerfMarker.Begin(this);
			//try
			//{
			//	if (_doGlobalLayout)
			//		ImGuiUn.DoLayout();   // ImGuiUn.Layout: global handlers
			//	Layout?.Invoke();     // this.Layout: handlers specific to this instance
			//}
			//finally
			//{
			//	ImGui.Render();
			//	s_layoutPerfMarker.End();
			//}

			//s_drawListPerfMarker.Begin(this);
			//_cmd.Clear();
			//_renderer.RenderDrawLists(_cmd, ImGui.GetDrawData());
			//s_drawListPerfMarker.End();
		}

		private void SetRenderer(IImGuiRenderer renderer, ImGuiIOPtr io)
		{
			_renderer?.Shutdown(io);
			_renderer = renderer;
			_renderer?.Initialize(io);
		}

		private void SetPlatform(IImGuiPlatform platform, ImGuiIOPtr io)
		{
			_platform?.Shutdown(io);
			_platform = platform;
			_platform?.Initialize(io);
		}
	}
}