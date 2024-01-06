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

            
            // Since multiplication is faster than division, we multiply by the inverse
            var invNumAlpha = 1 / numAlpha;
            var invNumBeta = 1 / numBeta;

            if(Math.Abs(numAlpha) < float.Epsilon || Math.Abs(numBeta) < float.Epsilon)
            {
                return null;
            }
            
            var barycentricPoints = new List<Vector4>(trianglePoints.Count);
            for(var i = 0; i < trianglePoints.Count; i++)
            {
                var x = trianglePoints[i].X;
                var y = trianglePoints[i].Y;

                // Barycentric coordinates are calculated
                var alpha = (-(x - p1.X) * delta21.Y + (y - p1.Y) * delta21.X) * invNumAlpha;
                var beta = (-(x - p2.X) * delta02.Y + (y - p2.Y) * delta02.X) * invNumBeta;
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
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector3 CalculateBarycentricCoordinates(Vector3 p, Vector3 v0, Vector3 v1, Vector3 v2)
        {
            var edge1 = v1 - v0;
            var edge2 = v2 - v0;
            var n = Vector3.Cross(edge1, edge2);

            // Ensure the area is not zero (or too small), otherwise the triangle is degenerate
            var area = n.Length();
            if (area < float.Epsilon)
            {
                return new Vector3(float.NaN, float.NaN, float.NaN);
            }

            var invArea = 1 / area;

            var n1 = Vector3.Cross(v2 - v1, p - v1) * invArea;
            var n2 = Vector3.Cross(-edge2, p - v2) * invArea;
            var n3 = Vector3.Cross(edge1, p - v0) * invArea;  

            var alpha = Vector3.Dot(n, n1);
            var beta = Vector3.Dot(n, n2);
            var gamma = Vector3.Dot(n, n3);

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
            // Check if the sum of coordinates exceeds 1 first, so we don't have to check for other conditions.
            if (barycentric.X + barycentric.Y + barycentric.Z > 1)
            {
                var sumDenom = 1 / (barycentric.X + barycentric.Y + barycentric.Z);
                return new Vector3(barycentric.X * sumDenom, barycentric.Y * sumDenom, barycentric.Z * sumDenom);
            }

            // Adjust negative coordinates to zero while keeping the sum to 1, ensure sum is still 1 and avoid accessing
            // the array, despite it being an extremely small performance gain 
            
            if (barycentric.X < 0) return new Vector3(0, 1 - barycentric.Z, barycentric.Z);
            if (barycentric.Y < 0) return new Vector3(1 - barycentric.Z, 0, barycentric.Z);
            if (barycentric.Z < 0) return new Vector3(1 - barycentric.Y, barycentric.Y, 0);

            
            if (barycentric.X > 1) return new Vector3(1, 0, 0);
            if (barycentric.Y > 1) return new Vector3(0, 1, 0);
            if (barycentric.Z > 1) return new Vector3(0, 0, 1);
            
            throw new Exception("something went wrong");
            
        }
    }
}
