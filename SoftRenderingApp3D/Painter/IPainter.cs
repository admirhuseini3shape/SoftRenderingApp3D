using SoftRenderingApp3D.Buffer;
using SoftRenderingApp3D.DataStructures;
using SoftRenderingApp3D.DataStructures.Materials;
using SoftRenderingApp3D.Renderer;
using System.Collections.Generic;
using System.Numerics;

namespace SoftRenderingApp3D.Painter
{
    public interface IPainter
    {
        List<FacetPixelData> DrawTriangle(IMaterial material, VertexBuffer vertexBuffer,
            RendererSettings rendererSettings, IReadOnlyList<Vector3> pixels, int faId);
    }
}
