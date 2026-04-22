using NUnit.Framework;

namespace UImGui.Tests
{
	internal class ContextTests
	{
		[Test]
		public void SetCurrentContext_Null_DoesNotThrow()
		{
			Assert.DoesNotThrow(() => UImGuiUtility.SetCurrentContext(null));
		}

		[Test]
		public void CreateDestroy_DoesNotThrow()
		{
			Context ctx = null;
			Assert.DoesNotThrow(() => ctx = UImGuiUtility.CreateContext());
			Assert.IsNotNull(ctx);
			Assert.DoesNotThrow(() => UImGuiUtility.DestroyContext(ctx));
		}

		[Test]
		public void DestroyContext_Null_DoesNotThrow()
		{
			Assert.DoesNotThrow(() => UImGuiUtility.DestroyContext(null));
		}
	}
}
