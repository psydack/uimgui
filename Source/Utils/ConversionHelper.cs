using System.Runtime.InteropServices;
using UnityEngine;

namespace UImGui
{
	public static class ConversionHelper
	{
		public static Color ToColor(this System.Numerics.Vector4 v4)
		{
			return new Color(v4.X, v4.Y, v4.Z, v4.W);
		}

		public static System.Numerics.Vector4 ToSystem(this Color c)
		{
			return new System.Numerics.Vector4(c.r, c.g, c.b, c.a);
		}

		[StructLayout(LayoutKind.Explicit)]
		private struct ColorVectorUnion
		{
			[FieldOffset(0)]
			public UnityEngine.Vector4 UVector;

			[FieldOffset(0)]
			public UnityEngine.Vector4 Color;

			[FieldOffset(0)]
			public System.Numerics.Vector<float> SVector;
		}

		public static System.Numerics.Vector<float> ToSystemGeneric(this UnityEngine.Vector4 vector)
		{
			ColorVectorUnion vectorUnionF = default;
			vectorUnionF.UVector = vector;
			return vectorUnionF.SVector;
		}

		public static System.Numerics.Vector<float> ToSystemGeneric(this UnityEngine.Color vector)
		{
			ColorVectorUnion vectorUnionF = default;
			vectorUnionF.Color = vector;
			return vectorUnionF.SVector;
		}
	}
}