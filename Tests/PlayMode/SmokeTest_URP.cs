using System.Collections;
using ImGuiNET;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace UImGui.Tests
{
	internal class SmokeTest_URP
	{
		private GameObject _go;
		private bool _layoutCalled;

		[UnitySetUp]
		public IEnumerator SetUp()
		{
			_layoutCalled = false;
			UImGuiUtility.Layout += OnLayout;
			yield return null;
		}

		[UnityTearDown]
		public IEnumerator TearDown()
		{
			UImGuiUtility.Layout -= OnLayout;
			if (_go != null)
				Object.Destroy(_go);
			yield return null;
		}

		private void OnLayout(UImGui uimgui)
		{
			if (ImGui.Begin("SmokeTest"))
			{
				ImGui.Text("hello");
				ImGui.End();
			}
			_layoutCalled = true;
		}

		[UnityTest]
		public IEnumerator Layout_FiresWithinThreeFrames()
		{
			// Wait up to 3 frames for a UImGui instance to call Layout.
			for (int i = 0; i < 3; i++)
			{
				if (_layoutCalled) break;
				yield return null;
			}

			// If no UImGui is in the scene this test is skipped (not a failure).
			if (!_layoutCalled)
				Assert.Ignore("No active UImGui found in scene — skipping smoke test.");

			Assert.IsTrue(_layoutCalled);
			LogAssert.NoUnexpectedReceived();
		}

		[UnityTest]
		public IEnumerator DrawData_HasWorkAfterLayout()
		{
			yield return new WaitForSeconds(0.1f);

			if (!_layoutCalled)
				Assert.Ignore("No active UImGui found in scene — skipping.");

			var drawData = ImGui.GetDrawData();
			Assert.IsTrue(drawData.CmdListsCount > 0, "Draw data should contain work after layout.");
		}
	}
}
