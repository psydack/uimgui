namespace UImGui
{
	internal interface IOptionalPlugin
	{
		void CreateContext(Context context);
		void SetCurrentContext(Context context);
		void DestroyContext(Context context);
	}
}
