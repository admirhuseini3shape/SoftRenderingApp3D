using SoftRenderingApp3D.Buffer;
using SoftRenderingApp3D.Painter;
using System.Numerics;

namespace SoftRenderingApp3D.Renderer
{
    public interface IRenderer
    {
        VertexBuffer VertexBuffer { get; }
        FrameBuffer FrameBuffer { get; }

        int[] Render(IPainterProvider painterProvider, Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix, RendererSettings rendererSettings);
    }
}
