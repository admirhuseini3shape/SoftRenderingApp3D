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
        public static void SortTrianglePoints(VertexBuffer vbx, FrameBuffer frameBuffer, int faId,
            out PaintedVertex v0, out PaintedVertex v1, out PaintedVertex v2, out int index0, out int index1,
            out int index2)
        {
            var t = vbx.Drawable.Mesh.Facets[faId];

            var worldNormVertices = vbx.WorldVertexNormals;
            var projectionVertices = vbx.ProjectionVertices;
            var worldVertices = vbx.WorldVertices;
            var triangleColor = Constants.StandardColor;
            
            if(vbx.Drawable.Material is IFacetColorMaterial facetColorMaterial)
                triangleColor = facetColorMaterial.FacetColors[faId];
            
            var color0 = triangleColor;
            var color1 = triangleColor;
            var color2 = triangleColor;
            
            if(vbx.Drawable.Material is IVertexColorMaterial vertexColorMaterial)
            {
                color0 = vertexColorMaterial.VertexColors[t.I0];
                color1 = vertexColorMaterial.VertexColors[t.I1];
                color2 = vertexColorMaterial.VertexColors[t.I2];
            }


            v0 = new PaintedVertex(worldNormVertices[t.I0],
                frameBuffer.ToScreen3(projectionVertices[t.I0]),
                worldVertices[t.I0], color0);
            v1 = new PaintedVertex(worldNormVertices[t.I1],
                frameBuffer.ToScreen3(projectionVertices[t.I1]),
                worldVertices[t.I1], color1);
            v2 = new PaintedVertex(worldNormVertices[t.I2],
                frameBuffer.ToScreen3(projectionVertices[t.I2]),
                worldVertices[t.I2], color2);

            index0 = t.I0;
            index1 = t.I1;
            index2 = t.I2;

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
