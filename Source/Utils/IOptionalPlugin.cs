using ImGuiNET;

namespace UImGui
{
	internal interface IOptionalPlugin
	{
		void Create(Context context);
		void SetCurrent(Context context);
		void Destroy(Context context);
	}
}
