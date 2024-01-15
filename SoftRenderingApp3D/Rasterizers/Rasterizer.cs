using SoftRenderingApp3D.Buffer;
using SoftRenderingApp3D.DataStructures.Drawables;
using SoftRenderingApp3D.Renderer;
using SoftRenderingApp3D.Utils;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace SoftRenderingApp3D.Rasterizers
{
    public static class Rasterizer
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<Vector3> RasterizeFacet(VertexBuffer vertexBuffer, FrameBuffer frameBuffer, IDrawable drawable,
            RendererSettings rendererSettings, int faId, Stats stats)
        {
            var facet = drawable.Mesh.Facets[faId];

            // Discard if behind far plane
            if(facet.IsBehindFarPlane(vertexBuffer))
            {
                stats.BehindViewTriangleCount++;
                return null;
            }

            // Discard if back facing 
            if(facet.IsFacingBack(vertexBuffer))
            {
                stats.FacingBackTriangleCount++;
                if(rendererSettings.BackFaceCulling)
                {
                    return null;
                }
            }

            // Project in frustum
            //facet.TransformProjection(vertexBuffer, projectionMatrix);

            // Discard if outside view frustum
            if(facet.IsOutsideFrustum(vertexBuffer))
            {
                stats.OutOfViewTriangleCount++;
                return null;
            }

            return GetPixels(vertexBuffer, frameBuffer, facet);
        }

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
