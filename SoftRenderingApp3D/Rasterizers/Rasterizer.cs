using SoftRenderingApp3D.Buffer;
using System.Runtime.CompilerServices;

namespace SoftRenderingApp3D.Rasterizers
{
    public class Rasterizer
    {
        private readonly VertexBuffer vertexBuffer;
        private readonly ScanLine scanLine;
        private readonly Stats stats;

        public Rasterizer(VertexBuffer vertexBuffer, FrameBuffer frameBuffer)
        {
            this.vertexBuffer = vertexBuffer;
            scanLine = new ScanLine(vertexBuffer, frameBuffer);
            this.stats = StatsSingleton.Instance;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RasterizeFacet(Facet facet, int faId, bool backFaceCulling)
        {
            // Discard if behind far plane
            if(facet.IsBehindFarPlane(vertexBuffer))
            {
                stats.BehindViewTriangleCount++;
                return;
            }

            // Discard if back facing 
            if(facet.IsFacingBack(vertexBuffer))
            {
                stats.FacingBackTriangleCount++;
                if(backFaceCulling)
                {
                    return;
                }
            }

            // Project in frustum
            //facet.TransformProjection(vertexBuffer, projectionMatrix);

            // Discard if outside view frustum
            if(facet.IsOutsideFrustum(vertexBuffer))
            {
                stats.OutOfViewTriangleCount++;
                return;
            }

            scanLine.ScanLineTriangle(facet, faId);
        }
    }
}
