# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

UImGui (`com.psydack.uimgui`) is a Unity UPM package that wraps [ImGui.NET](https://github.com/mellinoe/ImGui.NET) for use in Unity projects. It supports Dear ImGui 1.90.1 with optional extensions (ImPlot, ImNodes, ImGuizmo), all three Unity render pipelines (Built-in, URP, HDRP), and both Unity input systems.

This is a **Unity package**, not a standalone C# project. There is no build script to run directly — it is compiled by the Unity Editor when added to a project.

## Repository layout

```
Source/           Core runtime C# code
  UImGui.cs       Main MonoBehaviour — entry point, owns the frame loop
  Utils/          UImGuiUtility (static context/events), RenderUtility, Constants
  Platform/       IPlatform, InputManagerPlatform, InputSystemPlatform, PlatformBase
  Renderer/       IRenderer, RendererMesh, RendererProcedural, RenderImGui (URP feature), RenderImGuiHDPass (HDRP)
  Assets/         ScriptableObject assets: ShaderResourcesAsset, StyleAsset, FontAtlasConfigAsset, CursorShapesAsset, IniSettingsAsset
  Data/           Context, UIOConfig, Font/, Shader/, SpriteInfo, Range
  Texture/        TextureManager — maps Unity textures ↔ ImGui texture IDs
  Freetype/       ImFreetype, ImFreetypeNative — FreeType font rasteriser bindings
  Events/         FontInitializerEvent — UnityEvent wrapper for custom font setup
Plugins/          Native DLLs and managed DLLs for imgui, imguizmo, imnodes, implot
  System.Runtime.CompilerServices.Unsafe.dll
Resources/        Default shaders and assets referenced at runtime
Sample/           Demo MonoBehaviours showing usage patterns
Editor/           Editor-only assembly (UImGui.Editor.asmdef)
```

## Architecture

The central class is `UImGui` (MonoBehaviour). Each instance owns:
- A `Context` (wraps ImGui/ImPlot/ImNodes contexts + `TextureManager`)
- An `IRenderer` (Mesh or Procedural)
- An `IPlatform` (InputManager or InputSystem)
- A `CommandBuffer` submitted each frame

**Frame loop** (`DoUpdate`):
1. `TextureManager.PrepareFrame` — sync texture registrations
2. `IPlatform.PrepareFrame` — feed mouse/keyboard/time to `ImGuiIO`
3. `ImGui.NewFrame` / layout events fired (`UImGuiUtility.Layout` global + instance `Layout`)
4. `ImGui.Render` → `IRenderer.RenderDrawLists` → writes into `CommandBuffer`

**Render pipeline integration:**
- Built-in: `CommandBuffer` injected via `Camera.AddCommandBuffer(CameraEvent.AfterEverything)`
- URP: `RenderImGui` (`ScriptableRendererFeature`) executes the buffer
- HDRP: `RenderImGuiHDPass` (`CustomPass`) executes the buffer; `Update()` is skipped and `DoUpdate` is called from the pass instead

**Compile-time defines** (set automatically via `versionDefines` in `UImGui.asmdef`):
- `HAS_URP` — `com.unity.render-pipelines.universal >= 7.0.0`
- `HAS_HDRP` — `com.unity.render-pipelines.high-definition >= 7.0.0`
- `HAS_INPUTSYSTEM` — `com.unity.inputsystem >= 1.0.0`

**User-facing defines** (set in Project Settings → Player → Script Define Symbols):
- `UIMGUI_REMOVE_IMPLOT` — exclude ImPlot
- `UIMGUI_REMOVE_IMNODES` — exclude ImNodes
- `UIMGUI_REMOVE_IMGUIZMO` — exclude ImGuizmo
- `UIMGUI_REMOVE_UNSAFE_DLL` — exclude bundled `System.Runtime.CompilerServices.Unsafe.dll` when another package already provides it

## Usage patterns

**Global events** (requires `Do Global Events = true` on the `UImGui` component):
```cs
UImGuiUtility.Layout += OnLayout;
UImGuiUtility.OnInitialize += OnInitialize;
UImGuiUtility.OnDeinitialize += OnDeinitialize;
```

**Per-instance events:**
```cs
_uimGuiInstance.Layout += OnLayout;
```

**Texture display:**
```cs
IntPtr id = UImGuiUtility.GetTextureId(myTexture);
ImGui.Image(id, new Vector2(w, h));
```

**Custom fonts** — assign a method matching `void MyMethod(ImGuiIOPtr io)` to the `Font Custom Initializer` field on the `UImGui` component.

## Known issues to be aware of

- `System.Runtime.CompilerServices.Unsafe.dll` conflicts with other packages — use `UIMGUI_REMOVE_UNSAFE_DLL` to resolve.
- Font atlas crash has no fix; always use the callback approach for custom fonts.
- ImPlot is partially broken.
- `allowUnsafeCode = true` is required by the assembly definition.

## Versioning policy (required)

Every delivered change must increment the version in `package.json` using SemVer based on the impact of the modification:
- **MAJOR** (`X.0.0`): breaking API/behavior changes or compatibility breaks.
- **MINOR** (`x.Y.0`): backward-compatible new features.
- **PATCH** (`x.y.Z`): backward-compatible fixes, docs-only updates, and internal improvements.

Do not ship changes without updating `package.json` version accordingly.
