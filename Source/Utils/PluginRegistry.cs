using System.Collections.Generic;

namespace UImGui
{
	internal static class PluginRegistry
	{
		private static readonly List<IOptionalPlugin> Plugins = new List<IOptionalPlugin>();

		public static void Register(IOptionalPlugin plugin)
		{
			if (plugin != null && !Plugins.Contains(plugin))
			{
				Plugins.Add(plugin);
			}
		}

		public static void Create(Context context)
		{
			foreach (IOptionalPlugin plugin in Plugins)
			{
				plugin.Create(context);
			}
		}

		public static void SetCurrent(Context context)
		{
			foreach (IOptionalPlugin plugin in Plugins)
			{
				plugin.SetCurrent(context);
			}
		}

		public static void Destroy(Context context)
		{
			for (int i = Plugins.Count - 1; i >= 0; i--)
			{
				Plugins[i].Destroy(context);
			}
		}
	}
}
