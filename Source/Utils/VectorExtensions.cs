using System.Runtime.CompilerServices;
using UnityEngine;
using Num = System.Numerics;

namespace UImGui
{
    /// <summary>
    /// Zero-cost ref conversions between UnityEngine and System.Numerics vector types.
    /// Both types have identical memory layout (sequential floats), so Unsafe.As is safe
    /// on all platforms (Mono, IL2CPP, WebGL, Android, iOS).
    ///
    /// Usage in your plugin (add this file to your own asmdef):
    ///     global using Vector2 = System.Numerics.Vector2;
    ///     global using Vector3 = System.Numerics.Vector3;
    ///     global using Vector4 = System.Numerics.Vector4;
    ///
    /// Then at the Unity boundary:
    ///     UnityEngine.Vector2 unityPos = transform.position;
    ///     ImGui.SetNextWindowPos(ref unityPos.AsNumerics());
    /// </summary>
    public static class VectorExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref Num.Vector2 AsNumerics(this ref Vector2 v)
            => ref Unsafe.As<Vector2, Num.Vector2>(ref v);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref Vector2 AsUnity(this ref Num.Vector2 v)
            => ref Unsafe.As<Num.Vector2, Vector2>(ref v);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref Num.Vector3 AsNumerics(this ref Vector3 v)
            => ref Unsafe.As<Vector3, Num.Vector3>(ref v);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref Vector3 AsUnity(this ref Num.Vector3 v)
            => ref Unsafe.As<Num.Vector3, Vector3>(ref v);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref Num.Vector4 AsNumerics(this ref Vector4 v)
            => ref Unsafe.As<Vector4, Num.Vector4>(ref v);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref Vector4 AsUnity(this ref Num.Vector4 v)
            => ref Unsafe.As<Num.Vector4, Vector4>(ref v);

        /// <summary>Color (r,g,b,a) and Vector4 share the same 4-float layout.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref Num.Vector4 AsNumerics(this ref Color c)
            => ref Unsafe.As<Color, Num.Vector4>(ref c);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref Color AsColor(this ref Num.Vector4 v)
            => ref Unsafe.As<Num.Vector4, Color>(ref v);
    }
}
