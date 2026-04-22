using NUnit.Framework;
using UnityEngine;

namespace UImGui.Tests
{
	internal class VectorExtensionsTests
	{
		[Test]
		public void Vector2_RoundTrip()
		{
			var unity = new Vector2(1.5f, 2.5f);
			var num = unity.AsNumerics();
			Assert.AreEqual(unity, num.AsUnity());
		}

		[Test]
		public void Vector2_ToNumerics_Values()
		{
			var unity = new Vector2(3f, 4f);
			var num = unity.ToNumerics();
			Assert.AreEqual(unity.x, num.X);
			Assert.AreEqual(unity.y, num.Y);
		}

		[Test]
		public void Vector3_RoundTrip()
		{
			var unity = new Vector3(1f, 2f, 3f);
			var num = unity.AsNumerics();
			Assert.AreEqual(unity, num.AsUnity());
		}

		[Test]
		public void Vector4_RoundTrip()
		{
			var unity = new Vector4(1f, 2f, 3f, 4f);
			var num = unity.AsNumerics();
			Assert.AreEqual(unity, num.AsUnity());
		}

		[Test]
		public void Color_RoundTrip()
		{
			var unity = new Color(0.1f, 0.2f, 0.3f, 1f);
			var num = unity.AsNumerics();
			Assert.That(Mathf.Abs(unity.r - num.X), Is.LessThan(0.0001f));
			Assert.That(Mathf.Abs(unity.a - num.W), Is.LessThan(0.0001f));
		}
	}
}
