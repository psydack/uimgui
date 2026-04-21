# UImGui — Maintenance & Architecture Guide

> Living document. Update this file after every Unity, URP, Dear ImGui, ImGui.NET, or native plugin upgrade.
> Branch `feat/new-architecture` is the next evolution. See §11 for its plan.
> Target release: 7.0.0 (follow Claude.md Versioning policy)
> Execution branch: `feat/new-architecture`
---

## Table of Contents

1. [Current Baseline](#1-current-baseline)
2. [Non-Negotiable Invariants](#2-non-negotiable-invariants)
3. [Architecture Overview](#3-architecture-overview)
4. [Frame Lifecycle Contract](#4-frame-lifecycle-contract)
5. [Texture Backend Contract](#5-texture-backend-contract)
6. [Render Pipeline Contract](#6-render-pipeline-contract)
7. [Optional Plugin Maintenance](#7-optional-plugin-maintenance)
8. [Update Procedures](#8-update-procedures)
9. [Testing Strategy](#9-testing-strategy)
10. [Performance Roadmap](#10-performance-roadmap)
11. [feat/new-architecture Branch Plan](#11-featnew-architecture-branch-plan)
12. [README Update Procedure](#12-readme-update-procedure)
13. [Samples Expansion Plan](#13-samples-expansion-plan)
14. [Crash & Error Reference](#14-crash--error-reference)
15. [Release Checklist](#15-release-checklist)

---

## 1. Current Baseline

| Area | Value | Source |
|------|-------|--------|
| Package | `com.psydack.uimgui` 7.0.0 (target) | `package.json` |
| Unity minimum | 2022.3 | `package.json` |
| Validated project | Unity 6 / URP 17+ | this project |
| Dear ImGui | 1.92.7 | native `cimgui` binaries |
| Managed wrapper | ImGui.NET from `../ImGui.NET.4Unity` | `Plugins/imgui/ImGui.NET.dll` |
| Texture API | `drawData.Textures` (ImGui 1.92 backend) | `Source/Texture/TextureManager.cs` |
| URP integration | `RecordRenderGraph` / `UnsafeGraphContext` | `Source/Renderer/RenderImGui.cs` |
| Input default | Input System (`HAS_INPUTSYSTEM`) | `Source/Platform/InputSystemPlatform.cs` |
| Optional libraries | Disabled by default | `UIMGUI_ENABLE_*` defines |

---

## 2. Non-Negotiable Invariants

Survive any future Unity or ImGui upgrade by keeping these absolute.

- `cimgui` and `ImGui.NET.dll` must be updated as a matched pair.
- **Unity must be closed before copying native DLL/SO/DYLIB on Windows** — the editor shadow-loads binaries and the old version stays in memory until restart.
- `ImGui.NewFrame()` must happen before any layout code.
- `ImGui.Render()` must be called in a `finally` block so a failing layout callback cannot leave ImGui mid-frame.
- `TextureManager.UpdateTextures(ImGui.GetDrawData())` must run after `Render()` and before `RenderDrawData()` — even on URP 17+ where rendering is deferred to the render graph pass.
- `ImGuiBackendFlags.RendererHasTextures` must be set in every renderer's `Initialize()` or `NewFrame()` will not generate `WantCreate` texture events.
- `ImFontAtlasFlags.NoBakedLines` must be set before `NewFrame()` builds the atlas.
- External user textures registered via `GetTextureId()` must never be destroyed by `DestroyTexture()`.
- Optional plugin binaries must have matching `defineConstraints` in their `.meta` files.
- No managed allocations in per-frame hot paths unless explicitly justified.

---

## 3. Architecture Overview

```
UImGui (MonoBehaviour)
│
├── Context
│     ImGuiContext (IntPtr)
│     ImPlotContext / ImNodesContext / … (optional, IntPtr)
│     TextureManager
│
├── IPlatform ──────── InputManagerPlatform | InputSystemPlatform
│     PrepareFrame(io, pixelRect)
│
├── IRenderer ──────── RendererMesh | RendererProcedural
│     Initialize(io)  →  sets BackendFlags.RendererHasTextures
│     RenderDrawLists(CommandBuffer, drawData)
│
└── RenderPipeline ──── CommandBuffer injected per-pipeline
      Built-in:   Camera.AddCommandBuffer(AfterEverything)
      URP < 17:   ScriptableRenderPass.Execute
      URP ≥ 17:   RecordRenderGraph unsafe pass
      HDRP:       CustomPass.Execute
```

### Key design rules

- Composition, not inheritance: `UImGui` owns and coordinates; subsystems (`TextureManager`, `IRenderer`, `IPlatform`) do their own job behind interfaces.
- ScriptableObjects for all data assets (`ShaderResourcesAsset`, `StyleAsset`, etc.) — no MonoBehaviour overhead for config.
- Events (`Layout`, `OnInitialize`, `OnDeinitialize`) use C# `Action<UImGui>` — lower overhead than `UnityEvent`.
- `UImGuiUtility.Context` is the single static entry point for user code. All public API goes through it.

---

## 4. Frame Lifecycle Contract

Order is critical. Never reorder these steps.

| Step | Code | Why |
|------|------|-----|
| 1 | `UImGuiUtility.SetCurrentContext(_context)` | All ImGui calls are thread-local context calls |
| 2 | `_platform.PrepareFrame(io, camera.pixelRect)` | Feeds input and display size to `ImGuiIO` |
| 3 | `ImGui.NewFrame()` | Triggers atlas build on first frame; sets `WantCreate` in texture list |
| 4 | Layout callbacks (global + instance) | User UI code |
| 5 | `ImGui.Render()` in `finally` | Produces draw data; cannot be skipped even if layout throws |
| 6 | `TextureManager.UpdateTextures(drawData)` | Processes `WantCreate`/`WantUpdates`/`WantDestroy` before draw commands reference textures |
| 7 | `RenderDrawData(buffer)` | Null for URP 17+; render graph pass calls it later in the pipeline |

### URP 17+ split

`Update()` calls `DoUpdate(null)` — steps 1–6 run but step 7 is skipped.
The render graph pass calls `RenderDrawData(nativeCommandBuffer)` — step 7 only.
`UpdateTextures` must run in step 6 even when step 7 is deferred; otherwise the font texture is never created.

---

## 5. Texture Backend Contract

`TextureManager` owns the full lifetime of ImGui-created textures.

### Status machine

```
ImGui sets WantCreate  →  UploadTexture: create Texture2D, Y-flip copy, Apply, RegisterTexture, SetTexID, SetStatus(OK)
ImGui sets WantUpdates →  UpdateTexture: re-upload pixels to existing Texture2D, Apply, SetStatus(OK)
ImGui sets WantDestroy →  DestroyTexture ONLY if UnusedFrames > 0 (prevents current-frame use-after-free)
```

### Ownership split (current risk — tracked for hardening)

Backend-created textures (font atlas, emoji, etc.) and user-registered textures (`GetTextureId`) live in the same `_textures` / `_textureIds` dictionaries.
A future hardening pass must separate them so `DestroyTexture` can never accidentally destroy a user texture.

### Pixel format support

| `BytesPerPixel` | Upload path |
|-----------------|-------------|
| 4 (RGBA32) | Y-flip row-by-row with `NativeArray.Copy` |
| 1 (alpha) | Y-flip + expand to RGBA (`255,255,255,a`) |
| Other | Log error, destroy texture, return |

### `BuildFontAtlas` required flags

```csharp
io.Fonts.Flags |= ImFontAtlasFlags.NoBakedLines;       // required for 1.92 backend texture API
if (!io.MouseDrawCursor)
    io.Fonts.Flags |= ImFontAtlasFlags.NoMouseCursors;
```

---

## 6. Render Pipeline Contract

| Pipeline | Entry Point | Notes |
|----------|-------------|-------|
| Built-in | `Camera.AddCommandBuffer(AfterEverything)` | `DoUpdate(CommandBuffer)` — update + render in one step |
| URP < 17 | `ScriptableRenderPass.Execute` | Execute prepared `CommandBuffer` |
| URP ≥ 17 | `RecordRenderGraph` unsafe pass | `DoUpdate(null)` + deferred `RenderDrawData(nativeCmd)` |
| HDRP | `CustomPass.Execute` | `DoUpdate(context.cmd)` per UImGui instance |

### URP 17+ hardening rules

- Use `UniversalResourceData.activeColorTexture` — not private fields.
- Always wrap render func: `builder.AllowPassCulling(false)`.
- `CommandBufferHelpers.GetNativeCommandBuffer(ctx.cmd)` bridges render graph to legacy renderer code.
- Filter by camera: `if (Camera != renderingData.cameraData.camera) return;` in `AddRenderPasses`.

### Adding a new URP version breakpoint

1. Add to `UImGui.asmdef`:
   ```json
   { "name": "com.unity.render-pipelines.universal", "expression": "18.0.0", "define": "HAS_URP_18" }
   ```
2. Add `#if HAS_URP_18` path in `RenderImGui.cs`.
3. Keep previous path as fallback until deprecated.
4. Update `UImGui.Editor.asmdef` if editor-side URP types change.

---

## 7. Optional Plugin Maintenance

### Current optional plugins

| Define | Library | Context? | Status |
|--------|---------|----------|--------|
| `UIMGUI_ENABLE_IMPLOT` | ImPlot | Yes | Stable binary, intermittent rendering issues |
| `UIMGUI_ENABLE_IMNODES` | ImNodes | Yes | Stable |
| `UIMGUI_ENABLE_IMGUIZMO` | ImGuizmo | No | Disabled until crash-free with current cimgui |
| `UIMGUI_ENABLE_IMPLOT3D` | ImPlot3D | Yes | Enable after core is stable |
| `UIMGUI_ENABLE_IMNODES_R` | ImNodes-R | Yes | Enable after core is stable |
| `UIMGUI_ENABLE_IMGUIZMO_QUAT` | ImGuizmoQuat | No | Enable after core is stable |
| `UIMGUI_ENABLE_CIMCTE` | CimCTE | No | Enable after core is stable |

### Adding a new optional plugin — 8-step checklist

**1. Choose define symbol**: `UIMGUI_ENABLE_LIBFOO`

**2. Create plugin folder**:
```
Plugins/libfoo/
  LibFoo.NET.dll            ← managed wrapper
  LibFoo.NET.dll.meta       ← defineConstraints: [UIMGUI_ENABLE_LIBFOO]
  win-x64/cifoo.dll + .meta ← defineConstraints: [UIMGUI_ENABLE_LIBFOO]
  win-x86/  win-arm64/  linux-x64/  osx/  ← same pattern
```

**3. Context.cs** — only if library has `CreateContext`/`DestroyContext`:
```csharp
public IntPtr LibFooContext;
```

**4. UImGuiUtility.cs** — three blocks:
```csharp
// CreateContext:
#if UIMGUI_ENABLE_LIBFOO
    context.LibFooContext = LibFooNET.LibFoo.CreateContext();
#endif

// SetCurrentContext:
#if UIMGUI_ENABLE_LIBFOO
    LibFooNET.LibFoo.SetImGuiContext(context?.ImGuiContext ?? IntPtr.Zero);
#endif

// DestroyContext:
#if UIMGUI_ENABLE_LIBFOO
    LibFooNET.LibFoo.DestroyContext(context.LibFooContext);
#endif
```

**5. UImGuiEditor.cs** — `CheckRequirements()`:
```csharp
#if UIMGUI_ENABLE_LIBFOO
    EditorGUILayout.LabelField("LibFoo: enabled");
#else
    EditorGUILayout.LabelField("LibFoo: disabled");
#endif
```

**6. ShowDemoWindow.cs** — add a demo block under the existing ones.

**7. README.md** — add row to Features table and Directives table.

**8. PlanToUpdate.md** — add row to the Optional Plugins table above.

### Removing a plugin

Reverse of the above. Delete the `Plugins/<libname>/` folder and all four code blocks. Run compile tests with the define absent.

### Version update for an existing plugin

1. Close Unity.
2. Copy new native binaries to `Plugins/<libname>/<platform>/`.
3. Copy new managed DLL to `Plugins/<libname>/`.
4. Reopen Unity.
5. If managed API changed, update `UImGuiUtility.cs` and `ShowDemoWindow.cs`.
6. Update version note in `UImGuiEditor.cs`.
7. Update `PlanToUpdate.md` current baseline.

---

## 8. Update Procedures

### 8a. Dear ImGui / ImGui.NET

1. **Close Unity completely** (Windows locks native DLLs).
2. Pull `../ImGui.NET.4Unity` source.
3. Copy `ImGui.NET.dll` → `Plugins/imgui/ImGui.NET.dll`.
4. Copy native binaries to all platform folders.
5. Verify file sizes changed.
6. Reopen Unity. Check `ImGui.GetVersion()` in inspector.
7. Run compile tests and playmode smoke tests.
8. Update optional libraries one by one.

### 8b. Unity Editor version

1. Open project in new editor version.
2. Accept API upgrade prompts.
3. Fix compile errors before entering Play Mode.
4. Common break points: `NativeArray`, `CommandBuffer`, Input System, URP RenderGraph.
5. Run playmode smoke test.

### 8c. URP version

| File | What to check |
|------|---------------|
| `UImGui.asmdef` | Add `versionDefine` if new URP major breaks API |
| `Source/Renderer/RenderImGui.cs` | `RecordRenderGraph` / `UnsafeGraphContext` signatures |
| `Source/Renderer/RendererMesh.cs` | `CommandBuffer` API |
| `Source/Platform/PlatformBase.cs` | URP render scale path |
| `Resources/Shaders/PassesUniversal.hlsl` | SRP Batcher CBUFFER and include paths |
| `DearImGui-Mesh.shader` + `DearImGui-Procedural.shader` | `PackageRequirements` min URP version |

---

## 9. Testing Strategy

Goal: every future Unity or ImGui upgrade hits a failing test first, not a crash in production.

### Layer 1 — Compile gates

Each define combination must compile cleanly.

- No optional defines (core only).
- With `HAS_INPUTSYSTEM`.
- With each `UIMGUI_ENABLE_*` individually.
- With all certified defines simultaneously.

Run with:
```
Unity -batchmode -quit -projectPath <project> -runTests -testPlatform editmode
```

### Layer 2 — EditMode unit tests

Location: `Tests/Editor/`

**VectorExtensions round-trips**
```csharp
[Test] void Vector2_RoundTrip() {
    var unity = new Vector2(1.5f, 2.5f);
    var num = unity.AsNumerics();
    Assert.AreEqual(unity, num.AsUnity());
}
// Same for Vector3, Vector4, Color
```

**TextureManager — external registration stability**
```csharp
[Test] void RegisterSameTexture_ReturnsSameId() {
    var mgr = new TextureManager();
    var tex = new Texture2D(4, 4);
    var id1 = mgr.GetTextureId(tex);
    var id2 = mgr.GetTextureId(tex);
    Assert.AreEqual(id1, id2);
    Object.DestroyImmediate(tex);
}

[Test] void RegisterNullTexture_ReturnsZero() {
    var mgr = new TextureManager();
    Assert.AreEqual(IntPtr.Zero, mgr.GetTextureId(null));
}
```

**SpriteInfo caching**
```csharp
[Test] void GetSpriteInfo_NullSprite_ReturnsNull() {
    var mgr = new TextureManager();
    Assert.IsNull(mgr.GetSpriteInfo(null));
}
```

**FontConfig glyph range builder**
```csharp
[Test] void BuildRanges_BasicLatin_ContainsAscii() {
    var config = new FontConfig { ... };
    var ranges = config.BuildRanges();
    Assert.IsTrue(ranges.Count >= 2);
}
```

**Context — null safety**
```csharp
[Test] void SetCurrentContext_Null_DoesNotThrow() {
    Assert.DoesNotThrow(() => UImGuiUtility.SetCurrentContext(null));
}
```

### Layer 3 — PlayMode smoke tests

Location: `Tests/PlayMode/`

Minimum scene fixture:
1. Create `Camera` with `RenderImGui` feature.
2. Create `UImGui` component with default assets.
3. Register `UImGuiUtility.Layout += DrawTestUI`.
4. `DrawTestUI` calls `ImGui.Begin("Test")`, `ImGui.Text("hello")`, `ImGui.End()`.

Assertions after 3 frames:
```csharp
// No logged errors
LogAssert.NoUnexpectedReceived();

// Draw data contains work
var drawData = ImGui.GetDrawData();
Assert.IsTrue(drawData.CmdListsCount > 0);

// Font texture was created
// (Check via a flag on TextureManager exposed in tests)
Assert.IsTrue(uimgui.Context.TextureManager.HasCreatedBackendTextures);
```

### Layer 4 — Pipeline-specific tests

Run the same smoke test with project settings switched per pipeline:
- Built-in
- URP (pre-17 path if project supports)
- URP 17+ RenderGraph
- HDRP (if supported)

Each should pass with the same layout callback producing visible output.

### Layer 5 — Regression guards for known past failures

| Known failure | Test |
|--------------|------|
| `NoBakedLines` not set → atlas rebuild loop | Assert `io.Fonts.Flags` contains `NoBakedLines` after `BuildFontAtlas` |
| `UpdateTextures` skipped on URP 17+ | Assert font texture id is non-zero after first `DoUpdate(null)` call |
| Old cimgui DLL loaded after copy | Assert `ImGui.GetVersion()` contains "1.92.7" |
| `WantDestroy` before `UnusedFrames > 0` | Assert destroyed texture id is not referenced in draw data same frame |

### Test file structure

```
Tests/
  Editor/
    VectorExtensionsTests.cs
    TextureManagerTests.cs
    FontConfigTests.cs
    ContextTests.cs
    PluginFeatureTests.cs
  PlayMode/
    SmokeTest_BuiltIn.cs
    SmokeTest_URP.cs
    SmokeTest_HDRP.cs
    RegressionTests.cs
  Tests.asmdef           ← references UImGui + UImGui.Editor
  Tests.Editor.asmdef    ← EditorOnly=true
```

---

## 10. Performance Roadmap

### Short term (no API change)

- Pre-allocate `List<SubMeshDescriptor>` as a field in `RendererMesh` — currently `new List<>` each frame in `UpdateMesh`.
- Cache consecutive-same-texture lookups in `CreateDrawCommands` to skip repeated `TryGetTexture` dictionary hits.
- Remove per-frame `Debug.LogError` paths in texture upload from inner-loop — they cannot trigger in steady state but add branch pressure.

### Medium term (internal refactor only)

- Extract `UploadPixels(byte*, int, int, int, NativeArray<byte>)` as a static utility — enables unit testing and potential Burst jobs for the Y-flip copy.
- Separate owned backend textures into `_backendTextures` set vs user textures in `_textureIds` — unblocks safe bulk-destroy and prevents accidental user texture deletion.
- Add `TextureManager.Diagnostics` struct exposing `CreatedCount`, `DestroyedCount`, `UpdateCount` without allocating strings — drives test assertions and Profiler counters.

### Long term (interface change, target `feat/new-architecture`)

- Evaluate `RendererProcedural` as the primary path on SM4.5+ — fewer SetPass calls than Mesh on GPU-bound projects.
- Add backend format detection: if `GraphicsFormat.R8G8B8A8_UNorm` is unsupported (e.g. certain mobile GPUs), fall back gracefully.
- Consider `UnsafeUtility.MemCpy` for the 4bpp Y-flip instead of `NativeArray.Copy` loop.
- Async texture upload path for large atlases (custom fonts) via `AsyncGPUReadback` inverse.
- Add a profiler-friendly `Constants.TextureUploadMarker` around `UploadTexture` / `UpdateTexture`.

---

## 11. feat/new-architecture Branch Plan

All items in this section target branch `feat/new-architecture` and release 7.0.0.

Branch off `main` after the 1.92.7 texture fix is confirmed working.

```
git checkout -b feat/new-architecture
```

### Goal

Establish a layered architecture that:
- Survives future ImGui major version changes without touching user-facing APIs.
- Makes `TextureManager` fully unit-testable.
- Makes optional plugin hooks explicit and discoverable.
- Prevents the three most common crash types at the code level.

### Phase A — Test infrastructure (prerequisite, no behavior change)

**A1.** Create `Tests/Editor/` assembly with `VectorExtensionsTests`, `TextureManagerTests`, `ContextTests`.  
**A2.** Create `Tests/PlayMode/` assembly with `SmokeTest_URP.cs` asserting 3-frame render with no errors.  
**A3.** Add `HasCreatedBackendTextures` property to `TextureManager` gated behind `#if UNITY_TESTS || DEVELOPMENT_BUILD`.  
**A4.** Add `TextureManager.Diagnostics` struct with counts.

Commit: `test: add layer 1+2 tests and diagnostics struct`

### Phase B — TextureManager hardening

**B1.** Add `HashSet<IntPtr> _backendOwnedIds` — populated only by `UploadTexture`, never by `RegisterTexture`.  
**B2.** Guard `DestroyTexture` to only destroy if id is in `_backendOwnedIds`.  
**B3.** Rename `UploadTexture`'s pixel copy block to `CopyPixelsToTexture2D` static method for test isolation.  
**B4.** Add size-change handling to `UpdateTexture`: destroy + recreate if `width != tex2d.width || height != tex2d.height`.  
**B5.** Add `Constants.TextureUploadMarker` profiler around texture operations.

Commit: `refactor(texture): separate owned vs external textures, harden update path`

### Phase C — Crash resilience

**C1.** Wrap `DoUpdate` body in `try/catch(Exception e)` — on catch, log error + `enabled = false`. Prevents cascading per-frame crash spam.  
```csharp
internal void DoUpdate(CommandBuffer buffer)
{
    try { DoUpdateImpl(buffer); }
    catch (Exception e)
    {
        Debug.LogException(e, this);
        enabled = false;
    }
}
private void DoUpdateImpl(CommandBuffer buffer) { /* existing body */ }
```
**C2.** Add entry guard in `DoUpdate`: if `_renderer == null || _platform == null` log once and return.  
**C3.** In editor-only `OnEnable`, add DLL version assertion:
```csharp
#if UNITY_EDITOR
var version = ImGui.GetVersion();
if (!version.Contains("1.92"))
    Debug.LogWarning($"[UImGui] Expected ImGui 1.92.x, got {version}. Close Unity and recopy cimgui.dll.");
#endif
```
**C4.** Add `UImGuiUtility.SetCurrentContext(null)` null guard — already present but make explicit test for it.

Commit: `feat: crash resilience — self-disable on exception, DLL version warning`

### Phase D — Plugin system refactor

Replace the scattered `#if UIMGUI_ENABLE_*` blocks in `UImGuiUtility.cs` with a registration pattern:

```csharp
// Source/Utils/IOptionalPlugin.cs
internal interface IOptionalPlugin
{
    void CreateContext(Context context);
    void SetCurrentContext(Context context);
    void DestroyContext(Context context);
}
```

```csharp
// Source/Utils/PluginRegistry.cs
internal static class PluginRegistry
{
    static readonly List<IOptionalPlugin> _plugins = new List<IOptionalPlugin>(8);
    internal static void Register(IOptionalPlugin plugin) => _plugins.Add(plugin);
    internal static void CreateContextAll(Context ctx)      { foreach (var p in _plugins) p.CreateContext(ctx); }
    internal static void SetCurrentContextAll(Context ctx)  { foreach (var p in _plugins) p.SetCurrentContext(ctx); }
    internal static void DestroyContextAll(Context ctx)     { foreach (var p in _plugins) p.DestroyContext(ctx); }
}
```

Each optional plugin declares itself:
```csharp
// Source/Plugins/ImPlotPlugin.cs
#if UIMGUI_ENABLE_IMPLOT
[UnityEngine.RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
static void Register() => PluginRegistry.Register(new ImPlotPlugin());
#endif
```

Benefits:
- `UImGuiUtility.cs` becomes a thin coordinator with no per-plugin `#if` blocks.
- Adding a new library = one new file, one `Register` call.
- `PluginRegistry` is mockable in tests.

Commit: `refactor: plugin registration pattern replaces scattered #if blocks`

### Phase E — README + Samples

See §12 and §13. Update README.md and add sample scenes after architecture is stable.

### Phase F — Performance pass

Apply short/medium-term items from §10 after all tests are green.

### Branch merge criteria

- [ ] All EditMode tests pass.
- [ ] All PlayMode smoke tests pass on URP 17+ and Built-in.
- [ ] Crash resilience test: introduce null renderer manually, verify component disables instead of spamming errors.
- [ ] Plugin test: enable one plugin via define, verify it creates/destroys context without other plugins present.
- [ ] README and this document updated.

---

## 12. README Update Procedure

After any architecture or API change, update these README sections:

| Section | Trigger |
|---------|---------|
| Feature snapshot table | New optional library added or changed |
| Quick start — URP setup | `EnsureRenderFeatureRegistered` behavior changes |
| Optional integrations | Any `UIMGUI_ENABLE_*` define added, removed, or status change |
| Textures and images | `GetTextureId` / `GetSpriteInfo` API changes |
| Render pipeline setup | URP version breakpoints added |
| Troubleshooting | New error type or fix discovered |
| Credits / versions | ImGui version bump |

### Current README gaps to fill

- [ ] Add section "Frame lifecycle" explaining `DoUpdate` / `RenderDrawData` split for URP 17+.
- [ ] Document `VectorExtensions` usage and when to use `AsNumerics()` / `AsUnity()`.
- [ ] Keep `Font Atlas` marked as WIP until dedicated sample + validation is complete.
- [ ] Add troubleshooting entry: "Close Unity before copying native DLLs on Windows."
- [ ] Add note: `FontAtlasConfigAsset` is optional — default font renders without it.
- [ ] Update optional library versions in features table.
- [ ] Add `feat/new-architecture` migration notes once merged.
- [ ] Add cimgui ecosystem link: `https://github.com/orgs/cimgui/repositories`.
- [ ] Add credit note for `NewClear-mincho.ttf` with source link `https://booth.pm/en/items/713295`.

---

## 13. Samples Expansion Plan

All samples live in `Sample/`. Each must be self-contained and enable/disable cleanly.

### Existing

- `ShowDemoWindow.cs` — calls `ImGui.ShowDemoWindow()` and demonstrates each optional library.

### Add

| Sample | Class name | What it demonstrates |
|--------|------------|----------------------|
| Basic window | `SampleBasicWindow.cs` | `Begin`/`End`, text, button, checkbox, slider — zero optional deps |
| Texture display | `SampleTexture.cs` | `UImGuiUtility.GetTextureId`, `ImGui.Image`, `ImGui.ImageButton` |
| Custom font | `SampleCustomFont.cs` | `FontCustomInitializer` event, `AddFontFromFileTTF` pattern |
| Font atlas (WIP) | `SampleFontAtlasNewClearMincho.cs` | Font atlas flow using `NewClear-mincho.ttf` with graceful missing-file warning |
| Docking layout | `SampleDocking.cs` | `ImGui.DockSpaceOverViewport`, persistent layout via `IniSettingsAsset` |
| Sprite display | `SampleSprite.cs` | `UImGuiUtility.GetSpriteInfo`, UV-mapped sprite rendering |
| Multi-camera | `SampleMultiCamera.cs` | Two `UImGui` components on two cameras, `SetCamera` API |
| Runtime reload | `SampleReload.cs` | `uimgui.Reload()`, font hot-swap during Play Mode |
| Performance overlay | `SamplePerfOverlay.cs` | `ImGui.SetNextWindowBgAlpha`, frametime graph, no optional deps |

### Sample scene structure

Each sample should be a single `.cs` file with:
```csharp
void OnEnable()  => UImGuiUtility.Layout += OnLayout;
void OnDisable() => UImGuiUtility.Layout -= OnLayout;
void OnLayout(UImGui _) { /* ImGui calls here */ }
```

No scene setup logic in `Awake`/`Start` — samples wire themselves to the global layout event.

### ShowDemoWindow coverage rule

Every new request in `FixIssues.md` must end with a code snippet and have a matching block in `Sample/ShowDemoWindow.cs` so all requests are testable in one place.

---

## 14. Crash & Error Reference

### `EntryPointNotFoundException: igGetIO_Nil`

Loaded `cimgui.dll` is missing the expected symbol.

| Cause | Fix |
|-------|-----|
| Old DLL cached — copied while Unity was open | Close Unity, recopy, reopen |
| `defineConstraints` on meta blocks the DLL | Reset `defineConstraints: []` in meta |
| Version mismatch between managed and native | Verify file sizes; update matched pair |

### `Render Graph Execution error` (URP 17+)

Wrapper around an inner exception. Expand in Console or `Editor.log`.

- If inner = `EntryPointNotFoundException` → fix native DLL.
- If inner = texture-not-found Assert → verify `UpdateTextures` runs before `RenderDrawData`.
- If inner = `NullReferenceException` in render func → verify camera is assigned and not null.

### Text missing, widgets visible

1. Check `BackendFlags.RendererHasTextures` is set in renderer `Initialize()`.
2. Check `NoBakedLines` is set in `BuildFontAtlas`.
3. Verify `UpdateTextures` runs and font texture gets `WantCreate` on first frame.
4. Verify `TryGetTexture(TexID)` succeeds in `CreateDrawCommands`.
5. Check material property receives non-null texture.

### Crash inside `igNewFrame` / `igWindowRectRelToAbs`

DLL ABI mismatch — almost always wrong native binary in memory.

1. Disable all optional plugins.
2. Close Unity completely.
3. Check `Plugins/imgui/win-x64/cimgui.dll` file size matches the expected build.
4. Reopen Unity. Run without optional plugins first.

### Component keeps disabling itself on Play

`feat/new-architecture` adds auto-disable on exception. Check Console for the `[UImGui]` log line immediately before the disable. Fix the underlying error, then re-enable.

---

## 15. Release Checklist

Before tagging any version:

- [ ] `ImGui.GetVersion()` returns the expected string in inspector.
- [ ] Compile passes with no defines, with InputSystem, and with all certified optional defines.
- [ ] PlayMode smoke test passes: text renders, no Console errors, 3+ frames.
- [ ] URP 17+ RenderGraph path renders text and widgets.
- [ ] All EditMode unit tests pass.
- [ ] Optional libraries are disabled unless individually validated.
- [ ] `PlanToUpdate.md` Current Baseline table reflects the new version.
- [ ] `CHANGELOG.md` entry written.
- [ ] README troubleshooting section current.
- [ ] Native plugin file sizes or manifest updated if binaries changed.
- [ ] `feat/new-architecture` merge criteria met if this release includes that branch.
- [ ] Font atlas sample (`SampleFontAtlasNewClearMincho.cs`) compiles and renders expected text.
- [ ] Each new request has a matching `ShowDemoWindow` snippet.


