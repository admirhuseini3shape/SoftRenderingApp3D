using SoftRenderingApp3D.Buffer;
using SoftRenderingApp3D.Painter;

namespace SoftRenderingApp3D.Renderer
{
    public interface IRenderer
    {
        FrameBuffer FrameBuffer { get; }
        int[] Render(RenderContext renderContext, IPainter painter);
    }
}
