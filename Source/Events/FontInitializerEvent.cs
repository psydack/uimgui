using ImGuiNET;
using UnityEngine.Events;


namespace UImGui.Events
{
	[System.Serializable]
	public class FontInitializerEvent : UnityEvent<ImGuiIOPtr> { }
}