# UImGui
![GitHub tag (latest by date)](https://img.shields.io/github/v/tag/ocornut/imgui?style=flat-square)  
<sub>([ImGui library](https://github.com/ocornut/imgui) is available under a free and permissive license, but needs financial support to sustain its continued improvements. In addition to maintenance and stability there are many desirable features yet to be added. If your company is using Dear ImGui, please consider reaching out.)</sub>

UImGui (Unity ImGui) is an UPM package for the immediate mode GUI library using [ImGui.NET](https://github.com/mellinoe/ImGui.NET).
This project is based on [RG.ImGui](https://github.com/realgamessoftware/dear-imgui-unity) project. 

----

## What is Dear ImGui?

> Dear ImGui is a **bloat-free graphical user interface library for C++**. It outputs optimized vertex buffers that you can render anytime in your 3D-pipeline enabled application. It is fast, portable, renderer agnostic and self-contained (no external dependencies).
> 
> Dear ImGui is designed to **enable fast iterations** and to **empower programmers** to create **content creation tools and visualization / debug tools** (as opposed to UI for the average end-user). It favors simplicity and productivity toward this goal, and lacks certain features normally found in more high-level libraries.


## Motivation

To update (using ImGui.Net.dll) easier and often.

## Features

| Feature                   |         RG         |      UImGui        | 
| -----------------         | ------------------ | ------------------ |
| IL2CPP                    | :heavy_check_mark: | :heavy_check_mark: |
| Docking                   | :x:                | :heavy_check_mark: |
| Unity Input Manager       | :heavy_check_mark: | :heavy_check_mark: |
| Unity Input System        | :heavy_check_mark: | :heavy_check_mark: |
| Docking                   | :x:                | :heavy_check_mark: |
| RenderPipeline Built in   | :heavy_check_mark: | :heavy_check_mark: |
| RenderPipeline URP        | :x:                | :heavy_check_mark: |
| RenderPipeline HDRP       | :x:                | :x:                |
| Renderer Mesh             | :heavy_check_mark: | :heavy_check_mark: |
| Renderer Procedural       |          ~         | :heavy_check_mark: |
| FreeType                  | :heavy_check_mark: | :x:                |

Usage
-------
- [Add package](https://docs.unity3d.com/Manual/upm-ui-giturl.html) from git URL: https://github.com/psydack/uimguit;
- Add `UImGui` component to the scene and
- (Optional) Set `Platform Type` to `Input System` if you're using the new [input system](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/manual/index.html) the `SampleDemoWindow` object on the scene the following properties:
- If you're using **URP** check [Using URP](https://github.com/psydack/uimgui#using-urp) section, for **HDRP** [Using HDRP](https://github.com/psydack/uimgui#using-hdrp) section, for **built in** check [Using Built in](https://github.com/psydack/uimgui#using-hdrp) section.
- You're ready.

Sample
-------
It has a demo scene called `UImGuiDemoScene` inside `UImGui/Sample` folder.

Using URP
-------
- Add a `Render Im Gui Feature` render feature to the renderer asset. Assign it to the `render feature` field of the DearImGui component.

Using HDRP
-------
You can't yet. 

Using Built in
-------
No special sets.

Known issues
-------

### Crispy / weird look
![crispy](https://user-images.githubusercontent.com/961971/119237759-e6bfc680-bb14-11eb-8118-b91babee6242.png)
**Solution** You should change your anti-aliasing from `FXAA` to `None` or `SMAA` or disable post processing. To do this, check your camera.

Credits
-------
Original repo https://github.com/realgamessoftware/dear-imgui-unity  
Thanks to @lacrc and @airtonmotoki for encouraging me.
https://www.conventionalcommits.org/en/v1.0.0/
https://semver.org/  
https://github.com/yeyushengfan258/Lyra-Cursors  
https://github.com/lob/generate-changelog  

License
-------

Dear ImGui is licensed under the MIT License, see [LICENSE.txt](https://github.com/ocornut/imgui/blob/master/LICENSE.txt) for more information.
