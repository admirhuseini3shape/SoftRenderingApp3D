using System.Numerics;
using System.Runtime.CompilerServices;

namespace SoftRenderingApp3D.Utils
{
    internal static class PainterUtils
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (int i0, int i1, int i2) SortIndices(Vector3[] screenPoints, int i0, int i1, int i2)
        {
            var c0 = screenPoints[i0].Y;
            var c1 = screenPoints[i1].Y;
            var c2 = screenPoints[i2].Y;

            if(c0 < c1)
            {
                if(c2 < c0)
                    return (i2, i0, i1);
                if(c1 < c2)
                    return (i0, i1, i2);
                return (i0, i2, i1);
            }

            if(c2 < c1)
                return (i2, i1, i0);
            if(c0 < c2)
                return (i1, i0, i2);
            return (i1, i2, i0);

        }

        // https://www.geeksforgeeks.org/orientation-3-ordered-points/

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Cross2D(Vector3 p0, Vector3 p1, Vector3 p2)
        {
            return (p1.X - p0.X) * (p2.Y - p1.Y) - (p1.Y - p0.Y) * (p2.X - p1.X);
        }
    }
}
