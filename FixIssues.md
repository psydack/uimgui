# FixIssues.md - GitHub Issues Fix Plan

> Source: https://github.com/psydack/uimgui/issues  
> Captured: 2026-04-21  
> Target release: 7.0.0  
> Execution branch: `feat/new-architecture`

---

## Status Legend

| Tag | Meaning |
|-----|---------|
| CRITICAL | Crash or feature not usable in production |
| HIGH | Broken behavior with workaround |
| MEDIUM | Quality/UX issue with lower risk |
| REQUEST | New capability |
| DOCS | Documentation/sample-only change |
| OUT OF SCOPE | Must be solved in `ImGui.NET.4Unity` or upstream bindings |

---

## Summary Table (ordered by priority)

| ID | Title | Status | Priority | Effort |
|----|-------|--------|----------|--------|
| NEW-C | Real HDRP support stabilization | DONE (WORKING) | P0 | L |
| #81 | HDRP setup broken in Unity 6 | DONE (WORKING) | P0 | M |
| #67/#54 | HDRP motion blur artifact (`DrawGizmos` interference) | DONE (WORKING) | P1 | S |
| NEW-A | `FontConfig.BuildRanges` ignores selected scripts | CRITICAL | P0 | S |
| NEW-B | "Support Everything" does not load expected ranges | CRITICAL | P0 | S |
| #45/#44 | Font atlas and custom font flow instability | HIGH (WIP) | P1 | M |
| #83 | Disabling `UImGui` can crash follow-up frames | CRITICAL | P0 | S |
| #80/#61 | Optional plugin native mismatch causes Play crash | CRITICAL | P0 | S |
| #63 | URP + FXAA render pass ordering issue | HIGH | P1 | M |
| #69 | Input System mouse wheel scale too low | HIGH | P1 | S |
| #77 | Docking helper API coverage gaps | REQUEST | P2 | S |
| #64 | Utility interfaces are internal | REQUEST | P2 | S |
| #71 | `p_open` usage confusion in docs/sample | DOCS | P2 | XS |
| #59 | `ActivateItemByID` missing | OUT OF SCOPE | - | - |
| #34 | `BeginTabItem` overload gap | OUT OF SCOPE | - | - |

---

## Global Delivery Rules

1. HDRP items are always implemented first.
2. Font atlas is currently **WIP** and must stay labeled as WIP until the sample and test flow pass.
3. Every request section must end with:
   - `ShowDemoWindow snippet`
   - `Expected behavior`
   - `Validation`
4. Every new request must be represented in `Sample/ShowDemoWindow.cs`.
5. Font Atlas must have a dedicated sample under `Sample/` and use `NewClear-mincho.ttf` as the canonical example font.

---

## Detailed Fix Plan

### NEW-C - Real HDRP support stabilization (P0 #1) - WORKING

**Scope**
- `Source/Renderer/RenderImGuiHDPass.cs`
- `Source/UImGui.cs`
- `Editor/Editors/UImGuiEditor.cs`
- README HDRP setup section

**Actions**
- Refresh UImGui registry safely after domain reload.
- Filter pass execution by camera ownership.
- Remove custom `DrawGizmos` call from HDRP pass path.
- Split update/render responsibilities for HDRP parity with URP 17+ model.
- Add inspector guidance for HDRP setup.

**ShowDemoWindow snippet**
```csharp
private bool _showHdrpStatus = true;

private void DrawHdrpStatus()
{
    if (!ImGui.Begin("HDRP Status", ref _showHdrpStatus))
    {
        ImGui.End();
        return;
    }

    ImGui.Text("HDRP custom pass path active.");
    ImGui.Text("This window should render only on the selected HDRP camera.");
    ImGui.End();
}
```

**Expected behavior**
- No duplicate UI across HDRP cameras.
- No motion blur ghost lines from ImGui pass ordering.

**Validation**
- Unity 6 HDRP scene with Custom Pass Volume, Play Mode, Game view visible, no render artifacts.
- Status: implemented and validated as working.

---

### #81 - HDRP setup broken in Unity 6 - WORKING

**Actions**
- Align setup guidance with actual required components.
- Ensure editor warning/help appears when HDRP is detected and pass is missing.

**ShowDemoWindow snippet**
```csharp
private bool _showHdrpSetupHelp = true;

private void DrawHdrpSetupHelp()
{
    if (!ImGui.Begin("HDRP Setup Help", ref _showHdrpSetupHelp))
    {
        ImGui.End();
        return;
    }

    ImGui.BulletText("1. Add Custom Pass Volume");
    ImGui.BulletText("2. Add DearImGuiPass");
    ImGui.BulletText("3. Assign camera on UImGui");
    ImGui.End();
}
```

**Expected behavior**
- Users can complete HDRP setup without trial-and-error.

**Validation**
- Fresh HDRP scene setup succeeds by following README + inspector hints only.
- Status: implemented and validated as working.

---

### #67/#54 - HDRP motion blur artifact - WORKING

**Actions**
- Remove manual gizmo draw call from HDRP custom pass path.

**ShowDemoWindow snippet**
```csharp
private bool _showMotionBlurCheck = true;

private void DrawMotionBlurCheck()
{
    if (!ImGui.Begin("HDRP Motion Blur Check", ref _showMotionBlurCheck))
    {
        ImGui.End();
        return;
    }

    ImGui.Text("Enable motion blur and verify there are no ghost lines.");
    ImGui.End();
}
```

**Expected behavior**
- No ImGui line ghosting when motion blur is enabled.

**Validation**
- HDRP camera with motion blur ON, stable UI in Game view.
- Status: implemented and validated as working.

---

### NEW-A - `FontConfig.BuildRanges` ignores selected scripts

**Actions**
- Replace incorrect range gating logic with explicit flag checks per script family.

**ShowDemoWindow snippet**
```csharp
private bool _showFontRangeCheck = true;

private void DrawFontRangeCheck()
{
    if (!ImGui.Begin("Font Range Check", ref _showFontRangeCheck))
    {
        ImGui.End();
        return;
    }

    ImGui.Text("Japanese: こんにちは");
    ImGui.Text("Cyrillic: Привет");
    ImGui.Text("Korean: 안녕하세요");
    ImGui.End();
}
```

**Expected behavior**
- Selected glyph ranges are honored at runtime.

**Validation**
- Font atlas with script flags renders each script sample correctly.

---

### NEW-B - "Support Everything" does not load expected ranges

**Actions**
- Add `None = 0` in enum for inspector clarity.
- Ensure "Everything" path includes all script ranges, not only default.

**ShowDemoWindow snippet**
```csharp
private bool _showSupportEverythingCheck = true;

private void DrawSupportEverythingCheck()
{
    if (!ImGui.Begin("Support Everything Check", ref _showSupportEverythingCheck))
    {
        ImGui.End();
        return;
    }

    ImGui.Text("Range test: English / 日本語 / 中文 / ไทย");
    ImGui.End();
}
```

**Expected behavior**
- "Everything" in inspector produces all selected ranges in final atlas.

**Validation**
- Toggle "Everything", enter Play Mode, verify multilingual rendering.

---

### #45/#44 - Font atlas and custom font flow (WIP)

**Actions**
- Keep issue open as WIP until sample + tests pass.
- Add dedicated sample file in `Sample/` for atlas initializer flow.
- Use `NewClear-mincho.ttf` as project example font.
- Add README credit link for the font source: `https://booth.pm/en/items/713295`.

**ShowDemoWindow snippet**
```csharp
private bool _showNewClearMinchoSample = true;

private void DrawNewClearMinchoSample()
{
    if (!ImGui.Begin("Font Atlas (WIP)", ref _showNewClearMinchoSample))
    {
        ImGui.End();
        return;
    }

    ImGui.Text("Example font: NewClear-mincho.ttf");
    ImGui.Text("If glyphs are missing, verify StreamingAssets path and atlas config.");
    ImGui.End();
}
```

**Expected behavior**
- Font sample renders with `NewClear-mincho.ttf` when configured.
- Missing-file path fails gracefully with warning, not crash.

**Validation**
- Play Mode with sample config on/off; no crash; clear warning when file is absent.

---

### Remaining items (short form)

| ID | Action | ShowDemoWindow requirement |
|----|--------|----------------------------|
| #83 | Add defensive null/exception flow in update path | Add a "component safety" debug panel |
| #80/#61 | Audit optional plugin meta `defineConstraints` | Add plugin availability panel |
| #63 | Move URP pass event default to post-processing-safe point | Add URP status panel |
| #69 | Normalize Input System wheel scaling | Add scroll calibration panel |
| #77 | Expand DockBuilder public utility surface | Add docking quick demo block |
| #64 | Expose required utility interfaces publicly | Add API visibility diagnostic text |
| #71 | Add clear `p_open` close-window sample | Add closable-window example |

---

## Implementation Order

### Sprint 1 - HDRP critical path (`feat/new-architecture`)
1. NEW-C
2. #81
3. #67/#54

### Sprint 2 - Font atlas WIP hardening (`feat/new-architecture`)
4. NEW-A
5. NEW-B
6. #45/#44 (keep WIP label until validated)

### Sprint 3 - Crash resilience and platform safety (`feat/new-architecture`)
7. #83
8. #80/#61
9. #63
10. #69

### Sprint 4 - API and docs completion (`feat/new-architecture`)
11. #77
12. #64
13. #71

---

## Regression Coverage

| Area | Validation |
|------|------------|
| HDRP core | Play Mode on Unity 6 HDRP, no duplicate windows, no blur artifacts |
| Font atlas | `NewClear-mincho.ttf` sample renders and degrades safely when missing |
| Optional plugins | No crash with all plugins disabled; per-plugin enable smoke test |
| URP path | FXAA + URP renders UI reliably |
| Sample policy | Every request above has a matching `ShowDemoWindow` snippet |
