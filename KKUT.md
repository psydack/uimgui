# UImGui package maintenance

- Keep `package.json`, `package-lock.json`, README, and CHANGELOG versions aligned.
- Treat `ImGui.NET.4Unity` as the only source for managed and native plugin artifacts.
- Run `sync-imgui.ps1`; do not copy individual DLLs ad hoc.
- Require one native-stack revision across Windows x86/x64/ARM64, Linux x64, and macOS universal.
- Preserve Unity `.meta` files and plugin importer settings.
- Compile the package in the supported Unity editor and run all EditMode tests before release.
- Scan the final `Plugins` tree and record SHA-256 hashes for Windows DLLs.
- Open release updates as a PR; do not tag until the PR and CI checks are green.
