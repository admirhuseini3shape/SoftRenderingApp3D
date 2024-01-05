using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System;

namespace SoftRenderingApp3D.Painter
{
    public static class Barycentric2d
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<Vector4> ConvertToBarycentricPoints(IReadOnlyList<Vector4> trianglePoints,
            Vector3 p0, Vector3 p1, Vector3 p2)
        {
            var delta21 = p2 - p1;
            var delta10 = p1 - p0;
            var delta02 = p0 - p2;
            var numAlpha = delta10.X * delta21.Y - delta10.Y * delta21.X;
            var numBeta = delta21.X * delta02.Y - delta21.Y * delta02.X;

            var barycentricPoints = new List<Vector4>(trianglePoints.Count);
            for(var i = 0; i < trianglePoints.Count; i++)
            {
                var x = trianglePoints[i].X;
                var y = trianglePoints[i].Y;

                // Barycentric coordinates are calculated
                var alpha = (-(x - p1.X) * delta21.Y + (y - p1.Y) * delta21.X) / numAlpha;
                var beta = (-(x - p2.X) * delta02.Y + (y - p2.Y) * delta02.X) / numBeta;
                var gamma = 1 - alpha - beta;

                barycentricPoints.Add(new Vector4(alpha, beta, gamma, trianglePoints[i].W));
            }

            return barycentricPoints;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 GetBarycentricCoordinates(Vector3 p, Vector3 pt0, Vector3 pt1, Vector3 pt2)
        {
            var barycentric = CalculateBarycentricCoordinates(p, pt0, pt1, pt2);
            return CheckIfBarycentricOutsideTriangle(barycentric) ?
                GetAdjustedBarycentric(barycentric)
                : barycentric;
        }

        // Efficient calculation of barycentric coordinates
        // taken from https://gamedev.stackexchange.com/a/23745
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector3 CalculateBarycentricCoordinates(Vector3 p, Vector3 v0, Vector3 v1, Vector3 v2)
        {
            var n = Vector3.Cross(v1 - v0, v2 - v0);
            var na = Vector3.Cross(v2 - v1, p - v1);
            var nb = Vector3.Cross(v0 - v2, p - v2);
            var nc = Vector3.Cross(v1 - v0, p - v0);

            var normFactor = 1 / Vector3.Dot(n, n);

            var alpha = Vector3.Dot(n, na) * normFactor;
            var beta = Vector3.Dot(n, nb) * normFactor;
            var gamma = Vector3.Dot(n, nc) * normFactor;

            return new Vector3(alpha, beta, gamma);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool CheckIfBarycentricOutsideTriangle(Vector3 barycentric)
        {
            return barycentric.X < 0 || barycentric.X > 1
                                     || barycentric.Y < 0 || barycentric.Y > 1
                                     || barycentric.Z < 0 || barycentric.Z > 1
                                     || barycentric.X + barycentric.Y + barycentric.Z > 1;
        }

        // deals with edge cases in the scanline algorithm
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector3 GetAdjustedBarycentric(Vector3 barycentric)
        {
            if(barycentric.X > 1)
            {
                return new Vector3(1, 0, 0);
            }

            if(barycentric.Y > 1)
            {
                return new Vector3(0, 1, 0);
            }

            if(barycentric.Z > 1)
            {
                return new Vector3(0, 0, 1);
            }

            if(barycentric.X < 0)
            {
                return new Vector3(0, barycentric.Y, barycentric.Z);
            }

            if(barycentric.Y < 0)
            {
                return new Vector3(barycentric.X, 0, barycentric.Z);
            }

            if(barycentric.Z < 0)
            {
                return new Vector3(barycentric.X, barycentric.Y, 0);
            }

            if(barycentric.X + barycentric.Y + barycentric.Z > 1)
            {
                var sumDenom = 1 / (barycentric.X + barycentric.Y + barycentric.Z);
                return new Vector3(barycentric.X * sumDenom, barycentric.Y * sumDenom, barycentric.Z * sumDenom);
            }

            throw new Exception("something not good if this happens");
        }
    }
}
