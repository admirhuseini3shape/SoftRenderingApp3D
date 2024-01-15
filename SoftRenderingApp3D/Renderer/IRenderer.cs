using SoftRenderingApp3D.Buffer;
using SoftRenderingApp3D.DataStructures.Drawables;
using SoftRenderingApp3D.Painter;
using System.Numerics;

namespace SoftRenderingApp3D.Renderer
{
    public interface IRenderer
    {
        int[] Render(VertexBuffer allVertexBuffers, FrameBuffer frameBuffer, IPainter painter,
            IDrawable drawables, Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix, RendererSettings rendererSettings);
    }
}
