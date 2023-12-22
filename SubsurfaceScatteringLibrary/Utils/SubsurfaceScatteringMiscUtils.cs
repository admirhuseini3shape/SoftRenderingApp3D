using g3;
using SoftRenderingApp3D;
using SoftRenderingApp3D.DataStructures;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace SubsurfaceScatteringLibrary.Utils {
    internal static class SubsurfaceScatteringMiscUtils {
        public static void Fill<T>(this T[] destinationArray, params T[] value) {
            Array.Copy(value, destinationArray, value.Length);

            int copyLength, nextCopyLength, destinationLength = destinationArray.Length;

            for(copyLength = value.Length;
                (nextCopyLength = copyLength << 1) < destinationLength;
                copyLength = nextCopyLength) {
                Array.Copy(destinationArray, 0, destinationArray, copyLength, copyLength);
            }

            Array.Copy(destinationArray, 0, destinationArray, copyLength, destinationLength - copyLength);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Swap<T>(ref T a, ref T b) {
            (b, a) = (a, b);
        }

        public static void Benchmark(this Action action, string caption, int l = 10000) {
            var sw = Stopwatch.StartNew();

            for(var i = 0; i < l; i++) {
                action();
            }

            sw.Stop();
            Console.WriteLine(caption + ":" + sw.ElapsedMilliseconds + " ms");
        }

        public static Vector3d Vector3ToVector3d(Vector3 vector) {
            return new Vector3d(vector.X, vector.Y, vector.Z);
        }

        public static IEnumerable<Vector3d> ToVector3dList(this Vector3[] array) {
            var resultList = new List<Vector3d>();
            foreach(var vector in array) {
                resultList.Add(Vector3ToVector3d(vector));
            }

            return resultList;
        }

        public static float[] ToFloatArray(this Vector3[] array) {
            var resultArray = new float[array.Length * 3];
            for(var i = 0; i < array.Length * 3; i += 3) {
                resultArray[i] = array[i / 3].X;
                resultArray[i + 1] = array[i / 3].Y;
                resultArray[i + 2] = array[i / 3].Z;
            }

            return resultArray;
        }

        public static float[] ToFloatArray(this ColoredVertex[] array) {
            var resultArray = new float[array.Length * 3];
            for(var i = 0; i < array.Length * 3; i += 3) {
                resultArray[i] = array[i / 3].position.X;
                resultArray[i + 1] = array[i / 3].position.Y;
                resultArray[i + 2] = array[i / 3].position.Z;
            }

            return resultArray;
        }

        public static int[] ToIntArray(this Triangle[] triangles) {
            var array = new int[triangles.Length * 3];
            for(var i = 0; i < triangles.Length * 3; i += 3) {
                array[i] = triangles[i / 3].I0;
                array[i + 1] = triangles[i / 3].I1;
                array[i + 2] = triangles[i / 3].I2;
            }

            return array;
        }
    }
}