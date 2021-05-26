#if NET_STANDARD_2_0
using System.Runtime.InteropServices;
using SVectorF = System.Numerics.Vector<float>;
using SVectorI = System.Numerics.Vector<int>;
using UVector2Int = UnityEngine.Vector2Int;
using UVector3Int = UnityEngine.Vector3Int;
#endif

using SMatrix4x4 = System.Numerics.Matrix4x4;
using SPlane = System.Numerics.Plane;
using SQuaternion = System.Numerics.Quaternion;
using SVector2 = System.Numerics.Vector2;
using SVector3 = System.Numerics.Vector3;
using SVector4 = System.Numerics.Vector4;

using UMatrix4x4 = UnityEngine.Matrix4x4;
using UPlane = UnityEngine.Plane;
using UQuaternion = UnityEngine.Quaternion;
using UVector2 = UnityEngine.Vector2;
using UVector3 = UnityEngine.Vector3;
using UVector4 = UnityEngine.Vector4;

namespace NumericsConverter
{
	/// <summary>
	/// Converts equivalent types between System.Numerics and UnityEngine vector types
	/// </summary>
	public static class ConverterExtensions
	{
		/// <summary>
		/// Convert <see cref="UnityEngine.Vector2"/> to <see cref="System.Numerics.Vector2"/>
		/// </summary>
		/// <param name="vector">Convertee</param>
		/// <returns>Converted</returns>
		public static SVector2 ToSystem(this UVector2 vector) => new SVector2(vector.x, vector.y);
#if NET_STANDARD_2_0
		[StructLayout(LayoutKind.Explicit)]
		private struct VectorUnionF2
		{
			[FieldOffset(0)] public UVector2 UVector;
			[FieldOffset(0)] public SVectorF SVector;
		}
		/// <summary>
		/// Convert <see cref="UnityEngine.Vector2"/> to <see cref="System.Numerics.Vector"/>
		/// </summary>
		/// <param name="vector">Convertee</param>
		/// <returns>Converted</returns>
		public static SVectorF ToSystemGeneric(this UVector2 vector) => new VectorUnionF2 { UVector = vector }.SVector;
#endif

		/// <summary>
		/// Convert <see cref="System.Numerics.Vector2"/> to <see cref="UnityEngine.Vector2"/>
		/// </summary>
		/// <param name="vector">Convertee</param>
		/// <returns>Converted</returns>
		public static UVector2 ToUnity(this SVector2 vector) => new UVector2(vector.X, vector.Y);

		/// <summary>
		/// Convert <see cref="UnityEngine.Vector3"/> to <see cref="System.Numerics.Vector3"/>
		/// </summary>
		/// <param name="vector">Convertee</param>
		/// <returns>Converted</returns>
		public static SVector3 ToSystem(this UVector3 vector) => new SVector3(vector.x, vector.y, vector.z);
#if NET_STANDARD_2_0
		[StructLayout(LayoutKind.Explicit)]
		private struct VectorUnionF3
		{
			[FieldOffset(0)] public UVector3 UVector;
			[FieldOffset(0)] public SVectorF SVector;
		}
		/// <summary>
		/// Convert <see cref="UnityEngine.Vector3"/> to <see cref="System.Numerics.Vector"/>
		/// </summary>
		/// <param name="vector">Convertee</param>
		/// <returns>Converted</returns>
		public static SVectorF ToSystemGeneric(this UVector3 vector) => new VectorUnionF3 { UVector = vector }.SVector;
#endif

		/// <summary>
		/// Convert <see cref="System.Numerics.Vector3"/> to <see cref="UnityEngine.Vector3"/>
		/// </summary>
		/// <param name="vector">Convertee</param>
		/// <returns>Converted</returns>
		public static UVector3 ToUnity(this SVector3 vector) => new UVector3(vector.X, vector.Y, vector.Z);

		/// <summary>
		/// Convert <see cref="UnityEngine.Vector4"/> to <see cref="System.Numerics.Vector4"/>
		/// </summary>
		/// <param name="vector">Convertee</param>
		/// <returns>Converted</returns>
		public static SVector4 ToSystem(this UVector4 vector) => new SVector4(vector.x, vector.y, vector.z, vector.w);
#if NET_STANDARD_2_0
		[StructLayout(LayoutKind.Explicit)]
		private struct VectorUnionF4
		{
			[FieldOffset(0)] public UVector4 UVector;
			[FieldOffset(0)] public SVectorF SVector;
		}

		/// <summary>
		/// Convert <see cref="UnityEngine.Vector4"/> to <see cref="System.Numerics.Vector"/>
		/// </summary>
		/// <param name="vector">Convertee</param>
		/// <returns>Converted</returns>
		public static SVectorF ToSystemGeneric(this UVector4 vector) => new VectorUnionF4 { UVector = vector }.SVector;
#endif

		/// <summary>
		/// Convert <see cref="System.Numerics.Vector4"/> to <see cref="UnityEngine.Vector4"/>
		/// </summary>
		/// <param name="vector">Convertee</param>
		/// <returns>Converted</returns>
		public static UVector4 ToUnity(this SVector4 vector) => new UVector4(vector.X, vector.Y, vector.Z, vector.W);
#if NET_STANDARD_2_0
		[StructLayout(LayoutKind.Explicit)]
		private struct VectorUnionI2
		{
			[FieldOffset(0)] public UVector2Int UVector;
			[FieldOffset(0)] public SVectorI SVector;
		}

		/// <summary>
		/// Convert <see cref="UnityEngine.Vector2Int"/> to <see cref="System.Numerics.Vector"/>
		/// </summary>
		/// <param name="vector">Convertee</param>
		/// <returns>Converted</returns>
		public static SVectorI ToSystem(this UVector2Int vector) => new VectorUnionI2 { UVector = vector }.SVector;

		/// <summary>
		/// Convert <see cref="System.Numerics.Vector"/> to <see cref="UnityEngine.Vector2Int"/>
		/// </summary>
		/// <param name="vector">Convertee</param>
		/// <returns>Converted</returns>
		public static UVector2Int ToUnity2(this SVectorI vector) => new UVector2Int(vector[0], vector[1]);

		[StructLayout(LayoutKind.Explicit)]
		private struct VectorUnionI3
		{
			[FieldOffset(0)] public UVector3Int UVector;
			[FieldOffset(0)] public SVectorI SVector;
		}

		/// <summary>
		/// Convert <see cref="UnityEngine.Vector3Int"/> to <see cref="System.Numerics.Vector"/>
		/// </summary>
		/// <param name="vector">Convertee</param>
		/// <returns>Converted</returns>
		public static SVectorI ToSystem(this UVector3Int vector) => new VectorUnionI3 { UVector = vector }.SVector;

		/// <summary>
		/// Convert <see cref="System.Numerics.Vector"/> to <see cref="UnityEngine.Vector3Int"/>
		/// </summary>
		/// <param name="vector">Convertee</param>
		/// <returns>Converted</returns>
		public static UVector3Int ToUnity3(this SVectorI vector) => new UVector3Int(vector[0], vector[1], vector[2]);
#endif

		/// <summary>
		/// Convert <see cref="UnityEngine.Quaternion"/> to <see cref="System.Numerics.Quaternion"/>
		/// </summary>
		/// <param name="quaternion">Convertee</param>
		/// <returns>Converted</returns>
		public static SQuaternion ToSystem(this UQuaternion quaternion) => new SQuaternion(quaternion.x, quaternion.y, quaternion.z, quaternion.w);

		/// <summary>
		/// Convert <see cref="System.Numerics.Quaternion"/> to <see cref="UnityEngine.Quaternion"/>
		/// </summary>
		/// <param name="quaternion">Convertee</param>
		/// <returns>Converted</returns>
		public static UQuaternion ToUnity(this SQuaternion quaternion) => new UQuaternion(quaternion.X, quaternion.Y, quaternion.Z, quaternion.W);

		/// <summary>
		/// Convert <see cref="UnityEngine.Plane"/> to <see cref="System.Numerics.Plane"/>
		/// </summary>
		/// <param name="plane">Convertee</param>
		/// <returns>Converted</returns>
		public static SPlane ToSystem(this UPlane plane) => new SPlane(plane.normal.ToSystem(), plane.distance);

		/// <summary>
		/// Convert <see cref="System.Numerics.Plane"/> to <see cref="UnityEngine.Plane"/>
		/// </summary>
		/// <param name="plane">Convertee</param>
		/// <returns>Converted</returns>
		public static UPlane ToUnity(this SPlane plane) => new UPlane(plane.Normal.ToUnity(), plane.D);

		/// <summary>
		/// Convert <see cref="UnityEngine.Matrix4x4"/> to <see cref="System.Numerics.Matrix4x4"/>
		/// </summary>
		/// <param name="matrix">Convertee</param>
		/// <returns>Converted</returns>
		public static SMatrix4x4 ToSystem(this UMatrix4x4 matrix) => new SMatrix4x4(
			matrix.m00, matrix.m01, matrix.m02, matrix.m03,
			matrix.m10, matrix.m11, matrix.m12, matrix.m13,
			matrix.m20, matrix.m21, matrix.m22, matrix.m23,
			matrix.m30, matrix.m31, matrix.m32, matrix.m33);

		/// <summary>
		/// Convert <see cref="System.Numerics.Matrix4x4"/> to <see cref="UnityEngine.Matrix4x4"/>
		/// </summary>
		/// <param name="matrix">Convertee</param>
		/// <returns>Converted</returns>
		public static UMatrix4x4 ToUnity(this SMatrix4x4 matrix) => new UMatrix4x4
		{
			m00 = matrix.M11,
			m01 = matrix.M12,
			m02 = matrix.M13,
			m03 = matrix.M14,
			m10 = matrix.M21,
			m11 = matrix.M22,
			m12 = matrix.M23,
			m13 = matrix.M24,
			m20 = matrix.M31,
			m21 = matrix.M32,
			m22 = matrix.M33,
			m23 = matrix.M34,
			m30 = matrix.M41,
			m31 = matrix.M42,
			m32 = matrix.M43,
			m33 = matrix.M44,
		};
	}
}
