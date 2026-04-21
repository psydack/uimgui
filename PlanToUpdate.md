# UImGui — Maintenance & Update Guide

> Living document. Update this file whenever a version bump is completed.

---

## Table of Contents

1. [Current Versions](#current-versions)
2. [Architecture Overview](#architecture-overview)
3. [How to Update: Dear ImGui / ImGui.NET](#how-to-update-dear-imgui--imguinet)
4. [How to Update: Unity / URP Version](#how-to-update-unity--urp-version)
5. [How to Update: Optional Libraries](#how-to-update-optional-libraries)
6. [Adding a New Optional Library](#adding-a-new-optional-library)
7. [Plugin Binary Structure](#plugin-binary-structure)
8. [Known Issues & TODOs](#known-issues--todos)
9. [Crash / Error Reference](#crash--error-reference)

---

## Current Versions

| Dependency | Version | Where defined |
|------------|---------|---------------|
| Dear ImGui | 1.92.7 | `package.json` description |
| ImGui.NET.4Unity | latest | `../ImGui.NET.4Unity/` (sibling repo) |
| Unity minimum | 2022.3 | `package.json` |
| URP minimum | 7.0.0 | `UImGui.asmdef` versionDefines |
| URP RenderGraph path | 17.0.0 | `UImGui.asmdef` → `HAS_URP_17` |
| com.psydack.uimgui | 6.0.0 | `package.json` |

---

## Architecture Overview

```
UImGui (MonoBehaviour)
│
├── Context  ──────────────────────── IntPtrs for ImGui + optional lib contexts
│
├── IRenderer  ────────────────────── RendererMesh | RendererProcedural
│   └── RenderDrawLists(CommandBuffer, ImDrawDataPtr)
│
├── IPlatform  ────────────────────── InputManagerPlatform | InputSystemPlatform
│   └── PrepareFrame(ImGuiIOPtr, Rect)
│
├── TextureManager  ───────────────── Texture ↔ IntPtr bidirectional map
│
└── RenderFeature / HDRP Pass ─────── Injects CommandBuffer into render pipeline
    (RenderImGui.cs / RenderImGuiHDPass.cs)
```

### Render pipeline paths

| Pipeline | Entry point | Compile flag |
|----------|-------------|--------------|
| Built-in | `Camera.AddCommandBuffer(CameraEvent.AfterEverything)` | _(none)_ |
| URP < 17 | `ScriptableRenderPass.Execute()` | `HAS_URP` |
| URP ≥ 17 | `RecordRenderGraph()` + `UnsafeGraphContext` | `HAS_URP_17` |
| HDRP | `CustomPass.Execute()` | `HAS_HDRP` |

### Opt-in library system

Each optional library is controlled by a scripting define symbol `UIMGUI_ENABLE_<NAME>`.  
Both the managed `.NET.dll` **and** all native binaries carry `defineConstraints: [UIMGUI_ENABLE_<NAME>]` in their `.meta` files — Unity silently skips them when the symbol is absent.

| Define | Library | Has own context? |
|--------|---------|-----------------|
| `UIMGUI_ENABLE_IMPLOT` | ImPlot | Yes (`ImPlotContext`) |
| `UIMGUI_ENABLE_IMNODES` | ImNodes | Yes (`ImNodesContext`) |
| `UIMGUI_ENABLE_IMGUIZMO` | ImGuizmo | No |
| `UIMGUI_ENABLE_IMPLOT3D` | ImPlot3D | Yes (`ImPlot3DContext`) |
| `UIMGUI_ENABLE_IMNODES_R` | ImNodesR | Yes (`ImNodesRContext`) |
| `UIMGUI_ENABLE_IMGUIZMO_QUAT` | ImGuizmoQuat | No |
| `UIMGUI_ENABLE_CIMCTE` | CimCTE | No |

Libraries without their own context only expose `SetImGuiContext` — they do not need a field in `Context.cs`.

---

## How to Update: Dear ImGui / ImGui.NET

Source repo: `../ImGui.NET.4Unity/` (sibling directory on disk)

### Step-by-step

1. **Pull the source repo:**
   ```
   cd ../ImGui.NET.4Unity && git pull
   ```

2. **Close the Unity Editor.** The Editor locks native binaries on Windows; copying over a locked `.dll` silently fails or corrupts the meta state.

3. **Copy managed DLLs** (safe while Unity is open — not locked):
   ```
   Plugins/imgui/ImGui.NET.dll          ← deps/imgui/ImGui.NET.dll
   Plugins/implot/ImPlot.NET.dll        ← deps/implot/ImPlot.NET.dll
   Plugins/imnodes/ImNodes.NET.dll      ← deps/imnodes/ImNodes.NET.dll
   ... (same for each enabled optional lib)
   ```

4. **Copy native binaries** (Unity must be closed):
   ```
   Plugins/imgui/win-x64/cimgui.dll     ← deps/cimgui/win-x64/cimgui.dll
   Plugins/imgui/win-x86/cimgui.dll     ← deps/cimgui/win-x86/cimgui.dll
   Plugins/imgui/win-arm64/cimgui.dll   ← deps/cimgui/win-arm64/cimgui.dll
   Plugins/imgui/linux-x64/cimgui.so    ← deps/cimgui/linux-x64/cimgui.so
   Plugins/imgui/osx/cimgui.dylib       ← deps/cimgui/osx/cimgui.dylib
   ```
   Repeat for each optional library under `Plugins/implot/`, `Plugins/imnodes/`, etc.

5. **Reopen Unity.** Check the Console for `EntryPointNotFoundException` on recompile.

6. **Update version strings** in `Editor/Editors/UImGuiEditor.cs` → `CheckRequirements()`.

7. **Update `package.json`** version and description if bumping the package version.

### Verification checklist

- [ ] Inspector on UImGui component shows new `ImGui.GetVersion()` value
- [ ] Enter Play mode — no `EntryPointNotFoundException` in Console
- [ ] `ShowDemoWindow` — all enabled lib windows render without errors
- [ ] If IL2CPP build is required: test a stripped build

---

## How to Update: Unity / URP Version

### When the Unity Editor version changes

1. Open the project in the new Editor.
2. Accept any API upgrade prompts.
3. Check Console for compile errors. Common break points:
   - `NativeArray` / `Unsafe` namespace changes
   - `CommandBuffer` API changes (rare)
   - Input System API (`Keyboard`, `Gamepad`) if `com.unity.inputsystem` bumps major
4. Fix errors in `Source/Platform/InputSystemPlatform.cs` for Input System changes.
5. Enter Play mode and verify ImGui renders.

### When the URP version changes

Check these files **in order** after upgrading the URP package:

| File | What to verify |
|------|----------------|
| `UImGui.asmdef` | Add new `versionDefine` for the new URP major if it breaks API |
| `Source/Renderer/RenderImGui.cs` | `RecordRenderGraph` / `UnsafeGraphContext` / `ContextContainer` signatures |
| `Source/Renderer/RendererMesh.cs` | `CommandBuffer` usage |
| `Source/Platform/PlatformBase.cs` | ~line 66 — URP render scale via `UniversalRenderPipeline.asset.renderScale` |
| `Resources/Shaders/PassesUniversal.hlsl` | SRP Batcher `CBUFFER`, include paths |
| `DearImGui-Mesh.shader` | `PackageRequirements` minimum URP version |
| `DearImGui-Procedural.shader` | Same |

**Adding a new URP breakpoint** (example: URP 18 introduces a breaking API):

1. Add to `UImGui.asmdef`:
   ```json
   {
     "name": "com.unity.render-pipelines.universal",
     "expression": "18.0.0",
     "define": "HAS_URP_18"
   }
   ```
2. In `RenderImGui.cs`, add `#if HAS_URP_18` path for the new API.
3. Keep `HAS_URP_17` path as fallback until URP 17 is deprecated.
4. Update `Editor/UImGui.Editor.asmdef` if editor-side URP types change.

### When the HDRP version changes

- `Source/Renderer/RenderImGuiHDPass.cs` — verify `CustomPass.Execute` signature.
- `DearImGui-Mesh.shader` — `PassesHD.hlsl` include and `PackageRequirements`.

---

## How to Update: Optional Libraries

Each library follows the same pattern. Example: ImPlot 0.17 → 0.18.

1. Close Unity.
2. Copy new `cimplot.dll/so/dylib` to `Plugins/implot/<platform>/`.
3. Copy new `ImPlot.NET.dll` to `Plugins/implot/`.
4. Reopen Unity.
5. If the managed C# API changed (renamed/removed methods), update:
   - `Source/Utils/UImGuiUtility.cs` — `CreateContext`, `DestroyContext`, `SetCurrentContext`
   - `Sample/ShowDemoWindow.cs` — demo usage
   - `Editor/Editors/UImGuiEditor.cs` — version string in `CheckRequirements()`
6. Commit: `feat(implot): update to 0.18`

---

## Adding a New Optional Library

Checklist for adding e.g. `libfoo`:

### 1. Choose define symbol
`UIMGUI_ENABLE_LIBFOO`

### 2. Create plugin folder
```
Plugins/libfoo/
  LibFoo.NET.dll            ← managed wrapper
  LibFoo.NET.dll.meta       ← defineConstraints: [UIMGUI_ENABLE_LIBFOO]
  win-x64/cimfoo.dll
  win-x64/cimfoo.dll.meta   ← defineConstraints: [UIMGUI_ENABLE_LIBFOO]
  win-x86/   win-arm64/   linux-x64/   osx/   ← same .meta pattern
```

Native binary meta template:
```yaml
defineConstraints:
- UIMGUI_ENABLE_LIBFOO
isPreloaded: 0
isOverridable: 0
isExplicitlyReferenced: 0
```

### 3. Context.cs — add IntPtr only if library has CreateContext/DestroyContext
```csharp
public IntPtr LibFooContext;
```
Libraries that only have `SetImGuiContext` (e.g. ImGuizmo, CimCTE) do **not** need this.

### 4. UImGuiUtility.cs
```csharp
// CreateContext():
#if UIMGUI_ENABLE_LIBFOO
    LibFooContext = LibFooNET.LibFoo.CreateContext(),
#endif

// SetCurrentContext():
#if UIMGUI_ENABLE_LIBFOO
    LibFooNET.LibFoo.SetImGuiContext(context?.ImGuiContext ?? IntPtr.Zero);
#endif

// DestroyContext():
#if UIMGUI_ENABLE_LIBFOO
    LibFooNET.LibFoo.DestroyContext(context.LibFooContext);
#endif
```

### 5. UImGuiEditor.cs — CheckRequirements()
```csharp
#if UIMGUI_ENABLE_LIBFOO
    EditorGUILayout.LabelField("LibFoo: 1.0 (enabled)");
#else
    EditorGUILayout.LabelField("LibFoo: disabled");
#endif
```

### 6. ShowDemoWindow.cs — add demo block
```csharp
#if UIMGUI_ENABLE_LIBFOO
    if (ImGui.Begin("LibFoo Sample"))
    {
        // minimal usage
        ImGui.End();
    }
#endif
```

### 7. README.md — two places
- Add row to Features table
- Add row to Directives table

---

## Plugin Binary Structure

```
Plugins/<libname>/
  <LibName>.NET.dll       ← managed C# wrapper
  <LibName>.NET.dll.meta  ← defineConstraints: [UIMGUI_ENABLE_<NAME>]
  win-x64/
    c<libname>.dll
    c<libname>.dll.meta   ← defineConstraints: [UIMGUI_ENABLE_<NAME>]
  win-x86/
    c<libname>.dll
    c<libname>.dll.meta
  win-arm64/
    c<libname>.dll
    c<libname>.dll.meta
  linux-x64/
    c<libname>.so
    c<libname>.so.meta
  osx/
    c<libname>.dylib
    c<libname>.dylib.meta
```

**Android / iOS** native binaries are not currently shipped. To add mobile support:
```
android/arm64/c<libname>.so
android/arm7/c<libname>.so
android/x86_64/c<libname>.so
ios/c<libname>.a
```
Each needs a `.meta` with correct `platformData` entries for `Android` and `iPhone`.

---

## Known Issues & TODOs

| Location | Issue | Priority |
|----------|-------|----------|
| `Source/UImGui.cs:17` | TODO: verify multithread safety | Low |
| `Source/Texture/TextureManager.cs:48` | TODO: remove `Unity.Collections` dependency from font atlas init | Medium |
| `Source/Renderer/RendererMesh.cs:216` | TODO: implement `ImDrawCmdPtr.GetTexID()` — fallback used | Medium |
| `Source/Renderer/RendererProcedural.cs:234` | TODO: same `GetTexID` issue | Medium |
| `Source/Data/UIOConfig.cs:64` | FIXME: `ConfigDockingAlwaysTabBar` causes auto-sizing regression | Low |
| `Source/Platform/InputSystemPlatform.cs:69` | Gamepad support untested | Low |
| `Resources/Shaders/DearImGui-Procedural.shader` | D3D11/Xbox `BaseVertex` workaround hardcoded — verify on other GPU backends | Low |
| General | No Android/iOS native binaries | Medium |
| General | Font atlas crash — no fix; always use the `FontCustomInitializer` callback | Known |
| General | ImPlot has intermittent rendering issues | Known |

---

## Crash / Error Reference

### `EntryPointNotFoundException: igGetIO_Nil`

**Meaning:** The native `cimgui.dll` (win-x64) is not loading.

| Cause | Fix |
|-------|-----|
| `defineConstraints` in `Plugins/imgui/win-x64/cimgui.dll.meta` is non-empty | Reset to `defineConstraints: []` |
| Unity Editor locked the DLL during a failed copy | Close Unity, recopy the DLL from `ImGui.NET.4Unity/deps/cimgui/win-x64/`, reopen |
| DLL version mismatch | Compare file sizes; source should be larger if newer |

---

### `Multiple precompiled assemblies with the same name System.Runtime.CompilerServices.Unsafe.dll`

**Fix:** Add `UIMGUI_REMOVE_UNSAFE_DLL` to *Project Settings → Player → Script Define Symbols*.

---

### `RenderGraph Execution error` (URP)

Wraps an inner exception — expand it in the Console. Usually caused by an `EntryPointNotFoundException` from a missing native DLL. Follow the fix above.

---

### Inspector: "Platform not available"

**Cause:** The selected `Platform Type` requires a package that is not installed (e.g., Input System selected but `com.unity.inputsystem` absent).  
**Fix:** Install the package or switch to Input Manager.

---

### ImGui renders nothing / black screen (URP)

**Cause:** `RenderImGui` feature not added to the active renderer asset, or not assigned to the UImGui component.  
**Fix:**
1. *Tools → UImGui → Add Render Feature to URP* (adds to all renderers in the active URP asset).
2. Assign the created `RenderImGui.asset` to the `Render Feature` field on the UImGui component.
3. If using multiple cameras, each camera's renderer needs the feature.
