using NUnit.Framework;
using System;
using UImGui.Texture;
using UnityEngine;

namespace UImGui.Tests
{
	internal class TextureManagerTests
	{
		[Test]
		public void RegisterSameTexture_ReturnsSameId()
		{
			var mgr = new TextureManager();
			var tex = new Texture2D(4, 4);
			IntPtr id1 = mgr.GetTextureId(tex);
			IntPtr id2 = mgr.GetTextureId(tex);
			Assert.AreEqual(id1, id2);
			UnityEngine.Object.DestroyImmediate(tex);
		}

		[Test]
		public void RegisterNullTexture_ReturnsZero()
		{
			var mgr = new TextureManager();
			Assert.AreEqual(IntPtr.Zero, mgr.GetTextureId(null));
		}

		[Test]
		public void GetSpriteInfo_NullSprite_ReturnsNull()
		{
			var mgr = new TextureManager();
			Assert.IsNull(mgr.GetSpriteInfo(null));
		}

		[Test]
		public void TryGetTexture_UnknownId_ReturnsFalse()
		{
			var mgr = new TextureManager();
			bool found = mgr.TryGetTexture(new IntPtr(0xDEAD), out _);
			Assert.IsFalse(found);
		}

		[Test]
		public void TryGetTexture_AfterRegister_ReturnsTrue()
		{
			var mgr = new TextureManager();
			var tex = new Texture2D(4, 4);
			IntPtr id = mgr.GetTextureId(tex);
			bool found = mgr.TryGetTexture(id, out var result);
			Assert.IsTrue(found);
			Assert.AreEqual(tex, result);
			UnityEngine.Object.DestroyImmediate(tex);
		}

		[Test]
		public void Diagnostics_UserRegistered_Increments()
		{
			var mgr = new TextureManager();
			var tex = new Texture2D(4, 4);
			mgr.GetTextureId(tex);
			Assert.AreEqual(1, mgr.GetDiagnostics().UserRegisteredCount);
			UnityEngine.Object.DestroyImmediate(tex);
		}
	}
}
