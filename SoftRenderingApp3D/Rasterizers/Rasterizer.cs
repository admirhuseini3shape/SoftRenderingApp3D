using SoftRenderingApp3D.Buffer;
using SoftRenderingApp3D.Utils;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace SoftRenderingApp3D.Rasterizers
{
    public static class Rasterizer
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<Vector3> GetPixels(VertexBuffer vertexBuffer, FrameBuffer frameBuffer, Facet facet)
        {
            var result = new List<Vector3>();
            //vertexBuffer.ScreenPointVertices[facet.I0] = frameBuffer.ToScreen3(vertexBuffer.ProjectionVertices[facet.I0]);
            //vertexBuffer.ScreenPointVertices[facet.I1] = frameBuffer.ToScreen3(vertexBuffer.ProjectionVertices[facet.I1]);
            //vertexBuffer.ScreenPointVertices[facet.I2] = frameBuffer.ToScreen3(vertexBuffer.ProjectionVertices[facet.I2]);

            var (i0, i1, i2) = PainterUtils.SortIndices(vertexBuffer.ScreenPointVertices, facet.I0, facet.I1, facet.I2);
            if(i0 == i1 || i1 == i2 || i2 == i0)
                return result;

            return ScanLine.ScanLineTriangle(vertexBuffer, frameBuffer, frameBuffer.Height, frameBuffer.Width, i0, i1, i2);
        }
    }
}
