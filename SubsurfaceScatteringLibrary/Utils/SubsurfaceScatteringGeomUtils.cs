﻿using SoftRenderingApp3D.Utils;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace SubsurfaceScatteringLibrary.Utils
{
    internal static class SubsurfaceScatteringGeomUtils
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 ToVector2(this Vector3 p)
        {
            return new Vector2(p.X, p.Y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 ToVector2(this Point p)
        {
            return new Vector2(p.X, p.Y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 ToVector3(this Vector4 p)
        {
            return new Vector3(p.X, p.Y, p.Z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Distribute(this Vector3 p, out int x, out int y, out int z)
        {
            x = (int)p.X;
            y = (int)p.Y;
            z = (int)p.Z;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 ToNdc(this Vector4 p)
        {
            return new Vector3(p.X, p.Y, p.Z) / (p.W == 0 ? MathUtils.Epsilon : p.W);
        }
    }
}
