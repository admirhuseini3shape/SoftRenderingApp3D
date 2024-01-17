using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace SoftRenderingApp3D.DataStructures
{
    public struct FacetBarycentricData
    {
        public Vector3 p0;
        public Vector3 p1;
        public Vector3 p2;

        public Vector3 Delta21;
        public Vector3 Delta10;
        public Vector3 Delta02;

        public float InverseAlpha;
        public float InverseBeta;

        public static FacetBarycentricData Zero()
        {
            return new FacetBarycentricData
            {
                p0 = Vector3.Zero,
                p1 = Vector3.Zero,
                p2 = Vector3.Zero,
                Delta21 = Vector3.Zero,
                Delta10 = Vector3.Zero,
                Delta02 = Vector3.Zero,
                InverseAlpha = 0,
                InverseBeta = 0
            };
        }

        public FacetBarycentricData(Vector3 p0, Vector3 p1, Vector3 p2)
        {
            this.p0 = p0;
            this.p1 = p1;
            this.p2 = p2;

            Delta21 = p2 - p1;
            Delta10 = p1 - p0;
            Delta02 = p0 - p2;
            var numAlpha = Delta10.X * Delta21.Y - Delta10.Y * Delta21.X;
            var numBeta = Delta21.X * Delta02.Y - Delta21.Y * Delta02.X;

            // Since multiplication is faster than division, we multiply by the inverse
            InverseAlpha = 1 / numAlpha;
            InverseBeta = 1 / numBeta;


            if(Math.Abs(InverseAlpha) < float.Epsilon || Math.Abs(InverseBeta) < float.Epsilon)
            {
                throw new OverflowException("Overflow in calculating barycentric mapping parameters!");
            }
        }

        public bool IsZero() => InverseAlpha == 0 && InverseBeta == 0;

        public Vector3 GetBarycentric(float x, float y)
        {
            // Barycentric coordinates are calculated
            var alpha = (-(x - p1.X) * Delta21.Y + (y - p1.Y) * Delta21.X) * InverseAlpha;
            var beta = (-(x - p2.X) * Delta02.Y + (y - p2.Y) * Delta02.X) * InverseBeta;
            var gamma = 1 - alpha - beta;
            var barycentric = new Vector3(alpha, beta, gamma);
            if(CheckIfBarycentricOutsideTriangle(barycentric))
                barycentric = GetAdjustedBarycentric(barycentric);
            return barycentric;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool CheckIfBarycentricOutsideTriangle(Vector3 barycentric)
        {
            return barycentric.X < 0 || barycentric.X > 1
                                     || barycentric.Y < 0 || barycentric.Y > 1
                                     || barycentric.Z < 0 || barycentric.Z > 1
                                     || barycentric.X + barycentric.Y + barycentric.Z > 1;
        }

        // deals with edge cases in the scan line algorithm
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector3 GetAdjustedBarycentric(Vector3 barycentric)
        {
            // Check if the sum of coordinates exceeds 1 first, so we don't have to check for other conditions.
            if(barycentric.X + barycentric.Y + barycentric.Z > 1)
            {
                var sumDenominator = 1 / (barycentric.X + barycentric.Y + barycentric.Z);
                return new Vector3(barycentric.X * sumDenominator,
                    barycentric.Y * sumDenominator,
                    barycentric.Z * sumDenominator);
            }

            // Adjust negative coordinates to zero while keeping the sum to 1, ensure sum is still 1 and avoid accessing
            // the array, despite it being an extremely small performance gain.

            if(barycentric.X < 0)
                barycentric = new Vector3(0, 1 - barycentric.Z, barycentric.Z);
            if(barycentric.Y < 0)
                barycentric = new Vector3(1 - barycentric.Z, 0, barycentric.Z);
            if(barycentric.Z < 0)
                barycentric = new Vector3(1 - barycentric.Y, barycentric.Y, 0);
            return barycentric;
        }
    }
}
