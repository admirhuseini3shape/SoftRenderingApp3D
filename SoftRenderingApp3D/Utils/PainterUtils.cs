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
        public static void SortTrianglePoints(VertexBuffer vertexBuffer, FrameBuffer frameBuffer, int faId,
            out PaintedVertex v0, out PaintedVertex v1, out PaintedVertex v2, out int index0, out int index1,
            out int index2)
        {
            var facet = vertexBuffer.Drawable.Mesh.Facets[faId];

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
                color0 = vertexColorMaterial.VertexColors[facet.I0];
                color1 = vertexColorMaterial.VertexColors[facet.I1];
                color2 = vertexColorMaterial.VertexColors[facet.I2];
            }


            v0 = new PaintedVertex(worldNormVertices[facet.I0],
                frameBuffer.ToScreen3(projectionVertices[facet.I0]),
                worldVertices[facet.I0], color0);
            v1 = new PaintedVertex(worldNormVertices[facet.I1],
                frameBuffer.ToScreen3(projectionVertices[facet.I1]),
                worldVertices[facet.I1], color1);
            v2 = new PaintedVertex(worldNormVertices[facet.I2],
                frameBuffer.ToScreen3(projectionVertices[facet.I2]),
                worldVertices[facet.I2], color2);

            index0 = facet.I0;
            index1 = facet.I1;
            index2 = facet.I2;

            if(v0.ScreenPoint.Y > v1.ScreenPoint.Y)
            {
                Extensions.Swap(ref v0, ref v1);
                Extensions.Swap(ref index0, ref index1);
            }

            if(v1.ScreenPoint.Y > v2.ScreenPoint.Y)
            {
                Extensions.Swap(ref v1, ref v2);
                Extensions.Swap(ref index1, ref index2);
            }

            if(v0.ScreenPoint.Y > v1.ScreenPoint.Y)
            {
                Extensions.Swap(ref v0, ref v1);
                Extensions.Swap(ref index0, ref index1);
            }
        }

        // https://www.geeksforgeeks.org/orientation-3-ordered-points/

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Cross2D(Vector3 p0, Vector3 p1, Vector3 p2)
        {
            return (p1.X - p0.X) * (p2.Y - p1.Y) - (p1.Y - p0.Y) * (p2.X - p1.X);
        }
    }
}
