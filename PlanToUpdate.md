# UImGui — Plano Mestre de Correções, Issues e Migração Unity 6

## Context

Este plano consolida três fontes de trabalho pendente no repositório `psydack/uimgui`:
1. **35 issues abertas** no GitHub (de 84 total)
2. **24 TODOs/BUGs** inline no código-fonte
3. **Checklist de migração** Unity 2019.4 → 2022.3 → Unity 6 (6000.3)

O projeto já declara `"unity": "2022.3"` no `package.json` e usa `Object.FindObjectsByType` (API moderna). O render pipeline é **detectado em runtime** via `RenderUtility.cs` — sem hardcoding de pipeline. O maior trabalho é:
- Corrigir crashes reais (ImGuizmo, HDRP Unity 6)
- Implementar `RecordRenderGraph` para URP Unity 6
- Fechar os bugs de input (scroll, mod keys)
- Limpar TODOs de alto impacto

---

## Grupo 1 — CRASHES / BLOCKERS (prioridade máxima)

### 1.1 ImGuizmo crash — SIGSEGV em ImGuizmo_BeginFrame (Issues #80, #61)
**Problema**: `ImGuizmo.BeginFrame()` causa SIGSEGV em Unity 2022.3+ ao entrar em Play Mode.  
**Causa**: ImGuizmo nativo não está inicializado quando `BeginFrame` é chamado antes do contexto estar pronto.  
**Arquivo**: `Source/UImGui.cs` linha ~257  
**Fix**:
```csharp
#if !UIMGUI_REMOVE_IMGUIZMO
try
{
    ImGuizmoNET.ImGuizmo.BeginFrame();
}
catch (Exception e)
{
    Debug.LogError($"[UImGui] ImGuizmo.BeginFrame() falhou: {e.Message}. Adicione UIMGUI_REMOVE_IMGUIZMO se não usar ImGuizmo.");
}
#endif
```
Adicionar nota na documentação: "se não usar ImGuizmo, adicione `UIMGUI_REMOVE_IMGUIZMO` em Player Settings."

### 1.2 HDRP crash / sem renderização no Unity 6 (Issue #81)
**Problema**: Em Unity 6, `CustomPassContext` teve mudanças de API; layout event não emite sem CustomPassVolume.  
**Arquivo**: `Source/Renderer/RenderImGuiHDPass.cs`  
**Fix**: Adicionar guard para Unity 6 e garantir que `_uimguis` não seja null antes do loop:
```csharp
protected override void Execute(CustomPassContext context)
{
    if (!Application.isPlaying) return;
    if (_uimguis == null) return;
    ...
}
```

---

## Grupo 2 — BUGS FUNCIONAIS (alta prioridade)

### 2.1 URP: renderização quebrada quando Render Scale ≠ 1.0 (Issue #72)
**Problema**: `DisplaySize` usa `camera.pixelRect.size` (resolução lógica); com render scale o framebuffer tem tamanho diferente.  
**Arquivo**: `Source/Platform/PlatformBase.cs` linha ~64  
**Fix**: Multiplicar `DisplayFramebufferScale` pelo `renderScale` quando URP ativo:
```csharp
public virtual void PrepareFrame(ImGuiIOPtr io, Rect displayRect)
{
    float scale = 1f;
#if HAS_URP
    if (RenderUtility.IsUsingURP())
    {
        var urpAsset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
        if (urpAsset != null) scale = urpAsset.renderScale;
    }
#endif
    io.DisplaySize = displayRect.size;
    io.DisplayFramebufferScale = Vector2.one * scale;
    io.DeltaTime = Time.unscaledDeltaTime;
    ...
}
```

### 2.2 Mouse scroll wheel lento (Issue #69)
**Problema**: `mouse.scroll.ReadValue() / 120f` dá valores muito pequenos no Input System 1.4+.  
**Arquivo**: `Source/Platform/InputSystemPlatform.cs` linha ~51  
**Fix**: Remover a divisão por 120 (o Input System já normaliza o scroll):
```csharp
var mouseScroll = mouse.scroll.ReadValue();
io.MouseWheel = mouseScroll.y;
io.MouseWheelH = mouseScroll.x;
```

### 2.3 Mod keys lentas (BUG inline — ambos os platforms)
**Problema**: Iterar sobre todos os KeyCodes e chamar `io.AddKeyEvent` para cada um, incluindo mod keys que depois são chamadas novamente separadamente, causando double-dispatch.  
**Arquivos**: `Source/Platform/InputManagerPlatform.cs:46`, `Source/Platform/InputSystemPlatform.cs:123,163`  
**Fix**: Em `TryMapKeys`, retornar `ImGuiKey.None` para todas as mod keys (`LeftShift, RightShift, LeftControl, RightControl, LeftAlt, RightAlt, LeftWindows/LeftCommand, RightWindows/RightCommand`) para evitar que entrem no loop genérico.

### 2.4 Gizmos visíveis no game mode HDRP (Issues #67, #54)
**Problema**: `context.renderContext.DrawGizmos()` executa também no Game View.  
**Arquivo**: `Source/Renderer/RenderImGuiHDPass.cs:30`  
**Fix**:
```csharp
#if UNITY_EDITOR
if (!context.hdCamera.camera.cameraType.HasFlag(CameraType.Game))
{
    context.renderContext.DrawGizmos(context.hdCamera.camera, GizmoSubset.PostImageEffects);
}
#endif
```

### 2.5 URP FXAA quebra renderização (Issue #63)
**Problema**: `RenderPassEvent.AfterRenderingPostProcessing` coloca o pass depois do FXAA, tornando o overlay incompatível.  
**Arquivo**: `Source/Renderer/RenderImGui.cs`  
**Fix**: Mudar default para `RenderPassEvent.AfterRenderingTransparents` (executa antes do post-processing) e expor o campo no Inspector para configuração manual.

### 2.6 URP 17.x — tipo incompatível no Render Feature (Issue #65)
**Problema**: Unity 6 + URP 17+ exige `RecordRenderGraph`; sem ele o `AddRenderPasses` em Compatibility Mode não associa corretamente ao renderer.  
**Fix**: Implementar `RecordRenderGraph` (ver item 3.1 abaixo).

---

## Grupo 3 — MIGRAÇÃO UNITY 6 (URP Render Graph)

### 3.1 Implementar RecordRenderGraph para URP Unity 6 (Issue #74)
**Problema**: Unity 6 URP usa Render Graph API. `AddRenderPasses` funciona em Compatibility Mode mas gera warnings e será removida.  
**Arquivo**: `Source/Renderer/RenderImGui.cs`  
**Fix**:
```csharp
#if UNITY_6_0_OR_NEWER && HAS_URP
using UnityEngine.Rendering.RenderGraphModule;

public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
{
    if (CommandBuffer == null) return;
    var cameraData = frameData.Get<UniversalCameraData>();
    if (Camera != cameraData.camera) return;

    using (var builder = renderGraph.AddUnsafePass<PassData>(
        "UImGui CommandBuffer Pass", out var passData))
    {
        passData.CommandBuffer = CommandBuffer;
        builder.AllowPassCulling(false);
        builder.SetRenderFunc((PassData data, UnsafeGraphContext ctx) =>
        {
            ctx.cmd.ExecuteCommandBuffer(data.CommandBuffer);
        });
    }
}

private class PassData
{
    public CommandBuffer CommandBuffer;
}
#endif
```

### 3.2 Remover `#if !UNITY_2020_1_OR_NEWER` obsoleto
**Arquivo**: `Editor/Editors/UImGuiEditor.cs:121`  
Remover o bloco que exibe aviso de "Unity 2019 não suporta Mesh" — não é mais relevante.

### 3.3 Guards de versão para Unity 6
Padrão a seguir em todo código novo:
```csharp
#if UNITY_6_0_OR_NEWER
    // APIs Unity 6: RecordRenderGraph, UniversalCameraData, etc.
#elif UNITY_2022_3_OR_NEWER
    // AddRenderPasses ainda funciona sem warnings
#endif
```

---

## Grupo 4 — FEATURE REQUESTS (prioridade média)

### 4.1 Tornar classes utilitárias públicas (Issue #64)
**Arquivo**: `Source/Utils/UImGuiUtility.cs`  
**Fix**: Tornar `Context`, `CreateContext`, `DestroyContext`, `SetCurrentContext` públicos para suportar arquiteturas custom (DOTS, ECS):
```csharp
public static Context CreateContext() { ... }
public static void DestroyContext(Context context) { ... }
public static void SetCurrentContext(Context context) { ... }
```

### 4.2 DockBuilder functions (Issue #77)
**Dependência**: Verificar se `DockBuilderAddNode`, `DockBuilderSetNodeSize`, `DockBuilderSplitNode`, `DockBuilderDockWindow` estão expostas no `cimgui.dll`.  
**Arquivo novo**: `Source/Utils/ImGuiDockBuilder.cs`

### 4.3 BeginTabItem sem ref bool (Issue #34)
**Arquivo**: `Source/Utils/ImGuiExtension.cs`
```csharp
public static bool BeginTabItem(string label, ImGuiTabItemFlags flags = ImGuiTabItemFlags.None)
    => ImGui.BeginTabItem(label, flags);
```

### 4.4 ActivateItemByID (Issue #59)
Adicionar P/Invoke binding para `igActivateItemByID` em `Source/Utils/ImGuiExtension.cs`.

---

## Grupo 5 — TODOs de CÓDIGO (prioridade baixa-média)

| # | Arquivo | Linha | Ação |
|---|---------|-------|------|
| T1 | `RendererProcedural.cs:13` | Switch `ComputeBuffer` → `GraphicsBuffer` para vertex buffer (melhor suporte Vulkan/Metal em Unity 6) |
| T2 | `RendererMesh.cs:135` | Reduzir dependência de `NativeArrayUnsafeUtility`; usar `Span<T>` com unsafe block direto quando possível |
| T3 | `PlatformBase.cs:64` | DPI-awareness via `Screen.dpi` ou `EditorGUIUtility.pixelsPerPoint` |
| T4 | `TextureManager.cs:151` | Validar extensão `.ttf`/`.otf` antes de chamar `AddFontFromFileTTF` |
| T5 | `PlatformCallbacks.cs:10,66` | Resolver memory ownership de clipboard em UTF-8 usando `Marshal.StringToHGlobalAnsi` |
| T6 | `IniSettingsAsset.cs:5` | Adicionar opção de salvar em `PlayerPrefs` com chave customizável |
| T7 | `InputManagerPlatform.cs:7` | Remover comentário desatualizado "check and remove from here" |
| T8 | `Constants.cs:11` | Testar e ativar todos os ProfilerMarkers; adicionar o marcador faltando nos paths HDRP |

---

## Grupo 6 — PLATAFORMAS E DOCUMENTAÇÃO

### 6.1 WebGL (Issue #47)
- `RendererProcedural` usa `SV_VertexID` — não suportado em GLES 2.0  
- **Fix**: Forçar `RenderType.Mesh` em WebGL via `#if UNITY_WEBGL` em `RenderUtility.Create()`
- Documentar a limitação

### 6.2 iOS/Android (Issue #50)
- Verificar `.meta` files em `Plugins/imgui/` para incluir plataformas ARM64
- Garantir que `includePlatforms` inclua `iOS` e `Android`

### 6.3 Addressables (Issue #37)
- Garantir que `ShaderResourcesAsset` e shaders estejam em `Resources/` ou em um bundle Addressable separado
- Documentar o requisito

---

## Relatório de Migração Unity 2019.4 → 2022.3 → Unity 6

### Status Atual
O projeto **já está em 2022.3** (package.json). APIs críticas já migradas:
- ✅ `Object.FindObjectsByType` (não usa `FindObjectsOfType` legado)
- ✅ Sem `Texture2D.Resize`
- ✅ Sem `GraphicsFormat.DepthAuto/ShadowAuto/VideoAuto`
- ✅ Input System tem caminho novo e legado separados
- ✅ Sem código Enlighten/lightmapping

### Ações necessárias para Unity 6 (6000.3)

| Item | Arquivo | Ação |
|------|---------|------|
| **URP Render Graph** | `Source/Renderer/RenderImGui.cs` | Implementar `RecordRenderGraph` (item 3.1) |
| **HDRP CustomPassContext** | `Source/Renderer/RenderImGuiHDPass.cs` | Guard null + Unity 6 (item 1.2) |
| **Legacy Input aviso** | Documentação | Avisar que `InputManager` será deprecated em versão futura |
| **ComputeBuffer → GraphicsBuffer** | `Source/Renderer/RendererProcedural.cs` | `GraphicsBuffer` com `Target.Structured` (item T1) |
| **#if !UNITY_2020_1_OR_NEWER** | `Editor/Editors/UImGuiEditor.cs:121` | Remover (item 3.2) |

### Itens que NÃO precisam de ação
- Sem XR legacy (nenhum código XR encontrado)
- Sem Enlighten / baked GI
- Sem Android/WebGL custom Gradle
- Sem Netcode/Multiplay/Adaptive Performance

---

## Ordem de Execução Recomendada

1. **Crashes** (1.1, 1.2) — desbloqueia qualquer teste
2. **Scroll + mod keys** (2.2, 2.3) — bugs de input mais reportados
3. **URP render scale** (2.1) — afeta qualidade visual
4. **RecordRenderGraph** (3.1) — prepara Unity 6
5. **Gizmos HDRP** (2.4) — polimento HDRP
6. **API pública** (4.1) — extensibilidade
7. **TODOs** (Grupo 5) — limpeza de código
8. **WebGL/plataformas** (6.1, 6.2) — plataformas adicionais

---

## Verificação

Testar em:
- Unity 2022.3 LTS + Built-in RP
- Unity 2022.3 LTS + URP (render scale = 1.0 e 1.5)
- Unity 2022.3 LTS + HDRP
- Unity 6.0 (6000.x) + URP com Render Graph habilitado
- Unity 6.0 + HDRP

Testes mínimos por item:
- Crash 1.1: entrar em Play Mode sem `UIMGUI_REMOVE_IMGUIZMO` → sem SIGSEGV
- Issue #72: `renderScale = 1.5` → UI renderiza no tamanho correto
- Issue #69: scroll do mouse → velocidade normal
- Issue #74: Unity 6 + URP → sem warnings de RecordRenderGraph
- Issue #67: Motion Blur ativo no HDRP → sem gizmos no game view

---

## Arquivos Críticos a Modificar

| Arquivo | Issues / TODOs cobertos |
|---------|------------------------|
| `Source/Renderer/RenderImGui.cs` | #74, #65, #63 |
| `Source/Renderer/RenderImGuiHDPass.cs` | #81, #67, #54 |
| `Source/UImGui.cs` | #80, #61 |
| `Source/Platform/InputSystemPlatform.cs` | #69, BUG mod keys |
| `Source/Platform/InputManagerPlatform.cs` | BUG mod keys |
| `Source/Platform/PlatformBase.cs` | #72, TODO DPI |
| `Source/Renderer/RendererProcedural.cs` | TODO ComputeBuffer→GraphicsBuffer |
| `Source/Utils/UImGuiUtility.cs` | #64 |
| `Editor/Editors/UImGuiEditor.cs` | cleanup 3.2 |
