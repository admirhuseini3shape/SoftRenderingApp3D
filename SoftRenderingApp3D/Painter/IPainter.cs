using SoftRenderingApp3D.Buffer;
using SoftRenderingApp3D.DataStructures.Materials;
using SoftRenderingApp3D.Renderer;
using SoftRenderingApp3D.Utils;

namespace SoftRenderingApp3D.Painter
{
    public interface IPainter
    {
        VertexBuffer VertexBuffer { get; }
        FrameBuffer FrameBuffer { get; }
        Barycentric2d BarycentricMapper { get; }

        int DrawPixel(int x, int y, RendererSettings rendererSettings);
    }

    public interface IPainter<T> : IPainter
    where T : IMaterial
    {
        T Material { get; }

        IPainter<T> Create(T material, VertexBuffer vertexBuffer, FrameBuffer frameBuffer);
    }

    public interface IPainterProvider
    {
        public IPainter GetPainter<T>(T material, VertexBuffer vertexBuffer,
            FrameBuffer frameBuffer, RendererSettings rendererSettings)
            where T : IMaterial;
    }
}
