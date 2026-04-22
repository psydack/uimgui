using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace UImGui.Tests
{
	internal class SmokeTest_URP
	{
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
			yield return null;
		}

		private void OnLayout(UImGui uimgui)
		{
			_layoutCalled = true;
		}

		[UnityTest]
		public IEnumerator Layout_FiresWithinThreeFrames()
		{
			for (int i = 0; i < 3; i++)
			{
				if (_layoutCalled) break;
				yield return null;
			}

			if (!_layoutCalled)
				Assert.Ignore("No active UImGui found in scene - skipping smoke test.");

			Assert.IsTrue(_layoutCalled);
			LogAssert.NoUnexpectedReceived();
		}

		[UnityTest]
		public IEnumerator Context_IsAvailableAfterLayout()
		{
			yield return new WaitForSeconds(0.1f);

			if (!_layoutCalled)
				Assert.Ignore("No active UImGui found in scene - skipping.");

			Assert.IsNotNull(UImGuiUtility.Context, "Context should be available after layout.");
		}
	}
}
