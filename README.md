# UImGui
![GitHub tag (latest by date)](https://img.shields.io/github/v/tag/psydack/uimgui?style=flat-square)  
<sub>([ImGui library](https://github.com/ocornut/imgui) is available under a free and permissive license, but needs financial support to sustain its continued improvements. In addition to maintenance and stability there are many desirable features yet to be added. If your company is using Dear ImGui, please consider reaching out.)</sub>

UImGui (Unity ImGui) is an UPM package for the immediate mode GUI library using [ImGui.NET](https://github.com/mellinoe/ImGui.NET).
This project is based on [RG.ImGui](https://github.com/realgamessoftware/dear-imgui-unity) project. 
This project use [FreeType](https://github.com/ocornut/imgui/tree/master/misc/freetype) as default renderer.

**Using imgui 1.84 WIP**

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
| Custom Assert             | :heavy_check_mark: | :x: 		      |
| Unity Input Manager       | :heavy_check_mark: | :heavy_check_mark: |
| Unity Input System        | :heavy_check_mark: | :heavy_check_mark: |
| Docking                   | :x:                | :heavy_check_mark: |
| RenderPipeline Built in   | :heavy_check_mark: | :heavy_check_mark: |
| RenderPipeline URP        | :x:                | :heavy_check_mark: |
| RenderPipeline HDRP       | :x:                | :heavy_check_mark: |
| Renderer Mesh             | :heavy_check_mark: | :heavy_check_mark: |
| Renderer Procedural       |          ~         | :heavy_check_mark: |
| FreeType                  |          ~         | :heavy_check_mark: |
| Image / Texture           | :x: 		 | :heavy_check_mark: |

Usage
-------
- [Add package](https://docs.unity3d.com/Manual/upm-ui-giturl.html) from git URL: https://github.com/psydack/uimgui.git
- Add `UImGui` component to the scene and
- (Optional) Set `Platform Type` to `Input System` if you're using the new [input system](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/manual/index.html) the `SampleDemoWindow` object on the scene the following properties:
- If you're using **URP** check [Using URP](https://github.com/psydack/uimgui#using-urp) section, for **HDRP** [Using HDRP](https://github.com/psydack/uimgui#using-hdrp) section, for **built in** check [Using Built in](https://github.com/psydack/uimgui#using-hdrp) section.
- You're ready. Look [Samples section](https://github.com/psydack/uimgui#samples) for more usage samples.

Samples
-------
It has a demo scene called `UImGuiDemoScene` inside `UImGui/Sample` folder.

You can subscribe to global layout or for a specific `UImGui` context:
If choose to use global, don't to forget to set ``Do Global Events`` to ``true`` on ``UImGui`` instance.

```cs
using UImGui;
using UnityEngine;

public class StaticSample : MonoBehaviour
{
	private void Awake()
	{
		UImGuiUtility.Layout += OnLayout;
		UImGuiUtility.OnInitialize += OnInitialize;
		UImGuiUtility.OnDeinitialize += OnDeinitialize;
	}

	private void OnLayout(UImGui.UImGui obj)
	{
		// Unity Update method. 
		// Your code belongs here! Like ImGui.Begin... etc.
	}

	private void OnInitialize(UImGui.UImGui obj)
	{
		// runs after UImGui.OnEnable();
	}

	private void OnDeinitialize(UImGui.UImGui obj)
	{
		// runs after UImGui.OnDisable();
	}

	private void OnDisable()
	{
		UImGuiUtility.Layout -= OnLayout;
		UImGuiUtility.OnInitialize -= OnInitialize;
		UImGuiUtility.OnDeinitialize -= OnDeinitialize;
	}
}

```

To use instance instead a global UImGui, use like this.

```cs
using UnityEngine;

public class InstanceSample : MonoBehaviour
{
	[SerializeField]
	private UImGui.UImGui _uimGuiInstance;

	private void Awake()
	{
		if (_uimGuiInstance == null)
		{
			Debug.LogError("Must assign a UImGuiInstance or use UImGuiUtility with Do Global Events on UImGui component.");
		}

		_uimGuiInstance.Layout += OnLayout;
		_uimGuiInstance.OnInitialize += OnInitialize;
		_uimGuiInstance.OnDeinitialize += OnDeinitialize;
	}

	private void OnLayout(UImGui.UImGui obj)
	{
		// Unity Update method. 
		// Your code belongs here! Like ImGui.Begin... etc.
	}

	private void OnInitialize(UImGui.UImGui obj)
	{
		// runs after UImGui.OnEnable();
	}

	private void OnDeinitialize(UImGui.UImGui obj)
	{
		// runs after UImGui.OnDisable();
	}

	private void OnDisable()
	{
		_uimGuiInstance.Layout -= OnLayout;
		_uimGuiInstance.OnInitialize -= OnInitialize;
		_uimGuiInstance.OnDeinitialize -= OnDeinitialize;
	}
}
```

Sample code
```cs
[SerializeField]
private float _sliderFloatValue = 1;

[SerializeField]
private string _inputText;

// Add listeners, etc ...

private void OnLayout(UImGui.UImGui obj)
{
	ImGui.Text($"Hello, world {123}");
	if (ImGui.Button("Save"))
	{
		Debug.Log("Save");
	}

	ImGui.InputText("string", ref _inputText, 100);
	ImGui.SliderFloat("float", ref _sliderFloatValue, 0.0f, 1.0f);
}
```
![image](https://user-images.githubusercontent.com/961971/119239324-b54bf880-bb1e-11eb-87e3-0ecbfaafde27.png)

```cs
[SerializeField]
private Vector4 _myColor;
private bool _isOpen;

private void OnLayout(UImGui.UImGui obj)
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
	ImGui.TextColored(new Vector4(1, 1, 0, 1), "Important Stuff");
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
[SerializeField]
private Texture _sampleTexture;

private void OnLayout(UImGui.UImGui obj)
{
	if (ImGui.Begin("Image Sample"))
	{
		System.IntPtr id = UImGuiUtility.GetTextureId(_sampleTexture);
		Vector2 size = new Vector2(_sampleTexture.width, _sampleTexture.height)
		ImGui.Image(id, size);

		ImGui.End();
	}
}
```
![image](https://user-images.githubusercontent.com/961971/119574206-b9308280-bd8b-11eb-9df2-8bc07cf57140.png)  
  
Custom UserData

```cs
[Serializable]
private struct UserData
{
	public int SomeCoolValue;
}

[SerializeField]
private UserData _userData;
private string _input = "";

// Add Listeners... etc.

private unsafe void OnInitialize(UImGui.UImGui uimgui)
{
	fixed (UserData* ptr = &_userData)
	{
		uimgui.SetUserData((IntPtr)ptr);
	}
}

private unsafe void OnLayout(UImGui.UImGui obj)
{
	if (ImGui.Begin("Custom UserData"))
	{
		fixed (UserData* ptr = &_userData)
		{
			ImGuiInputTextCallback customCallback = CustomCallback;
			ImGui.InputText("label", ref _input, 100, ~(ImGuiInputTextFlags)0, customCallback, (IntPtr)ptr);
		}

		ImGui.End();
	}
}

private unsafe int CustomCallback(ImGuiInputTextCallbackData* data)
{
	IntPtr userDataPtr = (IntPtr)data->UserData;
	if (userDataPtr != IntPtr.Zero)
	{
		UserData userData = Marshal.PtrToStructure<UserData>(userDataPtr);
		Debug.Log(userData.SomeCoolValue);
	}

	// You must to overwrite how you handle with new inputs.
	// ...

	return 1;
}
```
![image](https://user-images.githubusercontent.com/961971/120383734-a1ad4880-c2fb-11eb-87e1-398d5e7aac97.png)

You can [see more samples here](https://pthom.github.io/imgui_manual_online/manual/imgui_manual.html).

Using URP
-------
- Add a `Render Im Gui Feature` render feature to the renderer asset. 
- Assign it to the `render feature` field of the DearImGui component.

Using HDRP
-------
- When using the ``High Definition Render Pipeline``, add a custom render pass and select "DearImGuiPass" injected after post processing.

Using Built in
-------
No special sets.

Directives
-------
- ``UIMGUI_REMOVE_IMPLOT``: don't load implot lib and sources.  
- ``UIMGUI_REMOVE_IMNODES``: don't load imnodes lib and sources.  
- ``UIMGUI_REMOVE_IMGUIZMO``: don't load imguizmo lib and sources.  

Known issues
-------

Issue: Already using ``System.Runtime.CompilerServices.Unsafe.dll`` will cause the following error: ``Multiple precompiled assemblies with the same name System.Runtime.CompilerServices.Unsafe.dll included or the current platform Only one assembly with the same name is allowed per platform.
Resolution: add ``UIMGUI_REMOVE_UNSAFE_DLL`` on Project Settings > Player > Other Settings >  Script define symbols > Apply > Restart Unity Editor.

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
