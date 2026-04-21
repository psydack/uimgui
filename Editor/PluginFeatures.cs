using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;

namespace UImGui.Editor
{
	internal static class PluginFeatures
	{
		internal readonly struct Feature
		{
			public Feature(string name, string define)
			{
				Name = name;
				Define = define;
			}

			public string Name { get; }
			public string Define { get; }
		}

		private static readonly Feature[] OptionalFeatures =
		{
			new Feature("ImPlot", "UIMGUI_ENABLE_IMPLOT"),
			new Feature("ImNodes", "UIMGUI_ENABLE_IMNODES"),
			new Feature("ImGuizmo", "UIMGUI_ENABLE_IMGUIZMO"),
			new Feature("ImPlot3D", "UIMGUI_ENABLE_IMPLOT3D"),
			new Feature("ImNodes-R", "UIMGUI_ENABLE_IMNODES_R"),
			new Feature("ImGuizmoQuat", "UIMGUI_ENABLE_IMGUIZMO_QUAT"),
			new Feature("CimCTE", "UIMGUI_ENABLE_CIMCTE"),
		};

		public static IReadOnlyList<Feature> Features => OptionalFeatures;

		public static bool IsEnabled(Feature feature)
		{
			return GetDefines().Contains(feature.Define);
		}

		private static HashSet<string> GetDefines()
		{
			var buildTarget = NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
			var symbols = PlayerSettings.GetScriptingDefineSymbols(buildTarget);
			return symbols.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToHashSet();
		}
	}
}
