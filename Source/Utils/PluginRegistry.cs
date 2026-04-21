using System.Collections.Generic;

namespace UImGui
{
	internal static class PluginRegistry
	{
		static readonly List<IOptionalPlugin> _plugins = new List<IOptionalPlugin>(8);

		internal static void Register(IOptionalPlugin plugin) => _plugins.Add(plugin);

		internal static void CreateContextAll(Context ctx)
		{
			for (int i = 0; i < _plugins.Count; i++)
				_plugins[i].CreateContext(ctx);
		}

		internal static void SetCurrentContextAll(Context ctx)
		{
			for (int i = 0; i < _plugins.Count; i++)
				_plugins[i].SetCurrentContext(ctx);
		}

		internal static void DestroyContextAll(Context ctx)
		{
			for (int i = 0; i < _plugins.Count; i++)
				_plugins[i].DestroyContext(ctx);
		}
	}
}
