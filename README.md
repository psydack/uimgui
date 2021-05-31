# UImGui
![GitHub tag (latest by date)](https://img.shields.io/github/v/tag/psydack/uimgui?style=flat-square)  
<sub>([ImGui library](https://github.com/ocornut/imgui) is available under a free and permissive license, but needs financial support to sustain its continued improvements. In addition to maintenance and stability there are many desirable features yet to be added. If your company is using Dear ImGui, please consider reaching out.)</sub>

UImGui (Unity ImGui) is an UPM package for the immediate mode GUI library using [ImGui.NET](https://github.com/mellinoe/ImGui.NET).
This project is based on [RG.ImGui](https://github.com/realgamessoftware/dear-imgui-unity) project. 
This project use [FreeType](https://github.com/ocornut/imgui/tree/master/misc/freetype) as default renderer.

**Using ImGui.NET 1.84 WIP**

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
| IL2CPP                    | :x:                | :heavy_check_mark: |
| Windows                   | :heavy_check_mark: | :heavy_check_mark: |
| Linux                     | :heavy_check_mark: | :x: 		      |
| MacOS                     | :heavy_check_mark: | :x: 		      |
| Docking                   | :x:                | :heavy_check_mark: |
| Unity Input Manager       | :heavy_check_mark: | :heavy_check_mark: |
| Unity Input System        | :heavy_check_mark: | :heavy_check_mark: |
| Docking                   | :x:                | :heavy_check_mark: |
| RenderPipeline Built in   | :heavy_check_mark: | :heavy_check_mark: |
| RenderPipeline URP        | :x:                | :heavy_check_mark: |
| RenderPipeline HDRP       | :x:                | :heavy_check_mark: |
| Renderer Mesh             | :heavy_check_mark: | :heavy_check_mark: |
| Renderer Procedural       |          ~         | :heavy_check_mark: |
| FreeType                  | :heavy_check_mark: | :heavy_check_mark: |
| Image / Texture           | :x: 		 | :heavy_check_mark: |

Usage
-------
- [Add package](https://docs.unity3d.com/Manual/upm-ui-giturl.html) from git URL: https://github.com/psydack/uimguit
- Add `UImGui` component to the scene and
- (Optional) Set `Platform Type` to `Input System` if you're using the new [input system](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/manual/index.html) the `SampleDemoWindow` object on the scene the following properties:
- If you're using **URP** check [Using URP](https://github.com/psydack/uimgui#using-urp) section, for **HDRP** [Using HDRP](https://github.com/psydack/uimgui#using-hdrp) section, for **built in** check [Using Built in](https://github.com/psydack/uimgui#using-hdrp) section.
- You're ready. Look [Samples section](https://github.com/psydack/uimgui#samples) for more usage samples.

Samples
-------
It has a demo scene called `UImGuiDemoScene` inside `UImGui/Sample` folder.

You can subscribe to global layout or for a specific `UImGui` context:
If choose to use global, don't to forget to set ``Do Global Layout`` to ``true`` on ``UImGui`` instance.

```cs
using ImGuiNET;
using UImGui;
using UnityEngine;

public class UsingCurrentUImGui : MonoBehaviour
{
	[SerializeField]
	private UImGui.UImGui _uImGui;
  
	private void OnEnable()
	{
		_uImGui.Layout += OnLayout;
		// UImGuiUtility.Layout += OnLayout; // Use this for global layout.
	}

	private void OnDisable()
	{
		_uImGui.Layout -= OnLayout;
		// UImGuiUtility.Layout -= OnLayout; // Use this for global layout.
	}

	private void OnLayout()
	{
            // Your code goes here.
	}
}
```

The following codes goes on your `Layout` event, like this:  

```cs
using ImGuiNET;
using UnityEngine;

public class UsingCurrentUImGui : MonoBehaviour
{
	// Or you can use global layout. Look commented lines.
	[SerializeField]
	private UImGui.UImGui _uImGui;

	[SerializeField]
	private float _sliderFloatValue = 1;

	private byte[] _inputText = new byte[100];

	private void OnEnable()
	{
		_uImGui.Layout += OnLayout;
		// UImGuiUtility.Layout += OnLayout; // Use this for global layout.
	}

	private void OnDisable()
	{
		_uImGui.Layout -= OnLayout;
		// UImGuiUtility.Layout -= OnLayout; // Use this for global layout.
	}

	private void OnLayout()
	{
		ImGui.Text($"Hello, world {123}");
		if (ImGui.Button("Save"))
		{
			Debug.Log("Save");
		}

		ImGui.InputText("string", _inputText, (uint)(sizeof(byte) * _inputText.Length));
		ImGui.SliderFloat("float", ref _sliderFloatValue, 0.0f, 1.0f);
	}
}
```
![image](https://user-images.githubusercontent.com/961971/119239324-b54bf880-bb1e-11eb-87e3-0ecbfaafde27.png)

```cs
[SerializeField]
private System.Numerics.Vector4 _myColor;
private bool _isOpen;

private void OnLayout()
{
	// Create a window called "My First Tool", with a menu bar.
	ImGui.Begin("My First Tool", ref _isOpen, ImGuiWindowFlags.MenuBar);
	if (ImGui.BeginMenuBar())
	{
		if (ImGui.BeginMenu("File"))
		{
			if (ImGui.MenuItem("Open..", "Ctrl+O")) { /* Do stuff */ }
			if (ImGui.MenuItem("Save", "Ctrl+S")) { /* Do stuff */ }
			if (ImGui.MenuItem("Close", "Ctrl+W")) { _isOpen = false; }
			ImGui.EndMenu();
		}
		ImGui.EndMenuBar();
	}

	// Edit a color (stored as ~4 floats)
	ImGui.ColorEdit4("Color", ref _myColor);

	// Plot some values
	float[] my_values = new float[] { 0.2f, 0.1f, 1.0f, 0.5f, 0.9f, 2.2f };
	ImGui.PlotLines("Frame Times", ref my_values[0], my_values.Length);


	// Display contents in a scrolling region
	ImGui.TextColored(new System.Numerics.Vector4(1, 1, 0, 1), "Important Stuff");
	ImGui.BeginChild("Scrolling");
	for (int n = 0; n < 50; n++)
		ImGui.Text($"{n}: Some text");
	ImGui.EndChild();
	ImGui.End();
}
```
![image](https://user-images.githubusercontent.com/961971/119239823-f42f7d80-bb21-11eb-9f65-9fe03d8b2887.png)


Image Sample

```cs
using ImGuiNET;
using UImGui;
using UnityEngine;

public class UsingCurrentUImGui : MonoBehaviour
{
	[SerializeField]
	private Texture _sampleTexture;

	private void OnEnable()
	{
		UImGuiUtility.Layout += OnLayout;
	}

	private void OnDisable()
	{
		UImGuiUtility.Layout -= OnLayout;
	}

	private void OnLayout()
	{
		if (ImGui.Begin("Image Sample"))
		{
			System.IntPtr id = UImGuiUtility.GetTextureId(_sampleTexture);
			System.Numerics.Vector2 size = new System.Numerics.Vector2(_sampleTexture.width, _sampleTexture.height)
			ImGui.Image(id, size);

			ImGui.End();
		}
	}
}
```
![image](https://user-images.githubusercontent.com/961971/119574206-b9308280-bd8b-11eb-9df2-8bc07cf57140.png)  
  
  
[See more](https://pthom.github.io/imgui_manual_online/manual/imgui_manual.html).  

Using URP
-------
- Add a `Render Im Gui Feature` render feature to the renderer asset. 
- Assign it to the `render feature` field of the DearImGui component.

Using HDRP
-------
- When using the ``High Definition Render Pipeline``, add a custom render pass and select "DearImGuiPass" injected after post processing.
- Assign it to the `render feature` field of the DearImGui component.
- See **Known issues HDRP** for more info.

Using Built in
-------
No special sets.

Known issues
-------


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
