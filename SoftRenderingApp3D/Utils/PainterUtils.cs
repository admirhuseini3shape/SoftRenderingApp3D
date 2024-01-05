using SoftRenderingApp3D.Buffer;
using SoftRenderingApp3D.DataStructures.Materials;
using SoftRenderingApp3D.Painter;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace SoftRenderingApp3D.Utils
{
    internal static class PainterUtils
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (PaintedVertex v0, PaintedVertex v1, PaintedVertex v2) GetPaintedVertices(
            VertexBuffer vertexBuffer, FrameBuffer frameBuffer,int faId, (int i0, int i1,int i2) indices)
        {

            var worldNormVertices = vertexBuffer.WorldVertexNormals;
            var projectionVertices = vertexBuffer.ProjectionVertices;
            var worldVertices = vertexBuffer.WorldVertices;
            var triangleColor = Constants.StandardColor;

            if(vertexBuffer.Drawable.Material is IFacetColorMaterial facetColorMaterial)
                triangleColor = facetColorMaterial.FacetColors[faId];

            var color0 = triangleColor;
            var color1 = triangleColor;
            var color2 = triangleColor;

            if(vertexBuffer.Drawable.Material is IVertexColorMaterial vertexColorMaterial)
            {
                color0 = vertexColorMaterial.VertexColors[indices.i0];
                color1 = vertexColorMaterial.VertexColors[indices.i1];
                color2 = vertexColorMaterial.VertexColors[indices.i2];
            }

            var v0 = new PaintedVertex(worldNormVertices[indices.i0],
                frameBuffer.ToScreen3(projectionVertices[indices.i0]),
                worldVertices[indices.i0], color0);
            var v1 = new PaintedVertex(worldNormVertices[indices.i1],
                frameBuffer.ToScreen3(projectionVertices[indices.i1]),
                worldVertices[indices.i1], color1);
            var v2 = new PaintedVertex(worldNormVertices[indices.i2],
                frameBuffer.ToScreen3(projectionVertices[indices.i2]),
                worldVertices[indices.i2], color2);
            return (v0, v1, v2);
        }

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
