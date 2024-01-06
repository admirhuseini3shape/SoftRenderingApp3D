using SoftRenderingApp3D.Buffer;

namespace SoftRenderingApp3D.Painter
{
    public interface IPainter
    {
        void DrawTriangle(VertexBuffer vertexBuffer, FrameBuffer frameBuffer, float[,] zBuffer, int faId);
    }
}
