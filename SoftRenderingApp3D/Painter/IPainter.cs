using SoftRenderingApp3D.Buffer;
using System.Collections.Generic;
using System.Numerics;

namespace SoftRenderingApp3D.Painter
{
    public interface IPainter
    {
        List<(int x, int y, float z, ColorRGB color)> DrawTriangle(VertexBuffer vertexBuffer, List<Vector3> pixels, int faId);
    }
}
