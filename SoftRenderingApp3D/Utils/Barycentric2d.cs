using SoftRenderingApp3D.DataStructures;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace SoftRenderingApp3D.Utils
{
    public class Barycentric2d
    {
        private readonly FacetBarycentricData[] barycentricData;
        public Barycentric2d(int facetCount)
        {
            barycentricData = new FacetBarycentricData[facetCount];
            if(facetCount == 0)
                return;

            var zero = FacetBarycentricData.Zero();
            barycentricData.Fill(zero);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasDataForFacet(int faId)
        {
            return !barycentricData[faId].IsZero();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            var zero = FacetBarycentricData.Zero();
            barycentricData.Fill(zero);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddFacet(int faId, Vector3 p0, Vector3 p1, Vector3 p2)
        {
            if(faId > barycentricData.Length)
                return;

            barycentricData[faId] = new FacetBarycentricData(p0, p1, p2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3 GetBarycentric(float x, float y, int faId)
        {
            if(faId > barycentricData.Length)
                throw new IndexOutOfRangeException($"Index {faId} is out of range for {nameof(barycentricData)}");
            
            if(barycentricData[faId].IsZero())
                throw new Exception("Barycentric data for given facet is not initialized!");
            
            return barycentricData[faId].GetBarycentric(x, y); ;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<Vector3> ConvertToBarycentricPoints(IReadOnlyList<Vector3> trianglePoints,
            Vector3 p0, Vector3 p1, Vector3 p2)
        {
            var barycentricData = new FacetBarycentricData(p0, p1, p2);

            var barycentricPoints = new List<Vector3>(trianglePoints.Count);
            for(var i = 0; i < trianglePoints.Count; i++)
            {
                var barycentric = barycentricData.GetBarycentric(trianglePoints[i].X, trianglePoints[i].Y);
                barycentricPoints.Add(barycentric);
            }

            return barycentricPoints;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector3 CalculateBarycentricCoordinates(Vector3 p, Vector3 v0, Vector3 v1, Vector3 v2)
        {
            var edge1 = v1 - v0;
            var edge2 = v2 - v0;
            var n = Vector3.Cross(edge1, edge2);

            // Ensure the area is not zero (or too small), otherwise the triangle is degenerate
            var area = n.Length();
            if(area < float.Epsilon)
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



    }
}
