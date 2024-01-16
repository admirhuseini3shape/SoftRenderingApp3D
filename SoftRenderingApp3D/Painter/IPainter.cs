using SoftRenderingApp3D.Buffer;
using SoftRenderingApp3D.DataStructures;
using System.Collections.Generic;
using System.Numerics;

namespace SoftRenderingApp3D.Painter
{
    public interface IPainter
    {
        List<FacetPixelData> DrawTriangle(VertexBuffer vertexBuffer, List<Vector3> pixels, int faId);
    }
}
