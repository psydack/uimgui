using System;
using System.Reflection;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace UImGui.Tests
{
	internal class PlatformSettingsTests
	{
		[TestCase(null)]
		[TestCase("")]
		[TestCase("[Window][Settings regression]\nPos=17,29\nSize=240,180\nCollapsed=0\n")]
		public void Initialize_EmptyOrSavedIni_DoesNotCrashAndKeepsSavedWindows(string data)
		{
			var assembly = typeof(UImGui).Assembly;
			var settings = ScriptableObject.CreateInstance(assembly.GetType("UImGui.Assets.IniSettingsAsset"));
			settings.GetType().GetField("_data", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(settings, data);
			var context = UImGuiUtility.CreateContext();
			var platformType = assembly.GetType("UImGui.Platform.PlatformBase");
			var platform = Activator.CreateInstance(platformType, BindingFlags.Instance | BindingFlags.NonPublic, null, new object[] { null, settings }, null);
			try
			{
				UImGuiUtility.SetCurrentContext(context);
				var imgui = AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == "ImGui.NET").GetType("ImGuiNET.ImGui");
				var io = imgui.GetMethod("GetIO").Invoke(null, null);
				Assert.That(platformType.GetMethod("Initialize").Invoke(platform, new object[] { io, new UIOConfig(), "test" }), Is.True);
				if (!string.IsNullOrEmpty(data))
					Assert.That(imgui.GetMethod("SaveIniSettingsToMemory", Type.EmptyTypes).Invoke(null, null), Does.Contain("[Window][Settings regression]"));
				platformType.GetMethod("Shutdown").Invoke(platform, new object[] { io });
			}
			finally
			{
				UImGuiUtility.DestroyContext(context);
				UnityEngine.Object.DestroyImmediate(settings);
			}
		}
	}
}
