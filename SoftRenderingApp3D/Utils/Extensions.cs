using g3;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace SoftRenderingApp3D.Utils
{
    internal static class Extensions
    {
        public static void Fill<T>(this T[] destinationArray, params T[] value)
        {
            Array.Copy(value, destinationArray, value.Length);

            int copyLength, nextCopyLength, destinationLength = destinationArray.Length;

            for(copyLength = value.Length;
                (nextCopyLength = copyLength << 1) < destinationLength;
                copyLength = nextCopyLength)
            {
                Array.Copy(destinationArray, 0, destinationArray, copyLength, copyLength);
            }

            Array.Copy(destinationArray, 0, destinationArray, copyLength, destinationLength - copyLength);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Swap<T>(ref T a, ref T b)
        {
            (b, a) = (a, b);
        }

        public static void Benchmark(this Action action, string caption, int l = 10000)
        {
            var sw = Stopwatch.StartNew();

            for(var i = 0; i < l; i++)
                action();

            sw.Stop();
            Console.WriteLine(caption + ":" + sw.ElapsedMilliseconds + " ms");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3d ToVector3d(Vector3 vector)
        {
            return new Vector3d(vector.X, vector.Y, vector.Z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNaN(this Vector3 v)
        {
            return double.IsNaN(v.X) || double.IsNaN(v.Y) || double.IsNaN(v.Z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<Vector3d> ToVector3d(this IEnumerable<Vector3> array)
        {
            return array.Select(ToVector3d).ToList();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float[] ToFloatArray(this Vector3[] array)
        {
            var resultArray = new float[array.Length * 3];
            for(var i = 0; i < array.Length * 3; i += 3)
            {
                resultArray[i] = array[i / 3].X;
                resultArray[i + 1] = array[i / 3].Y;
                resultArray[i + 2] = array[i / 3].Z;
            }

            return resultArray;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int[] ToIntArray(this Facet[] triangles)
        {
            var array = new int[triangles.Length * 3];
            for(var i = 0; i < triangles.Length * 3; i += 3)
            {
                array[i] = triangles[i / 3].I0;
                array[i + 1] = triangles[i / 3].I1;
                array[i + 2] = triangles[i / 3].I2;
            }

            return array;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Transform(this Matrix4x4 matrix, Vector3 vector)
        {
            return Vector3.Transform(vector, matrix);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 TransformWithoutTranslation(this Matrix4x4 matrix, Vector3 vector)
        {
            return Vector3.TransformNormal(vector, matrix);
        }
    }
}
