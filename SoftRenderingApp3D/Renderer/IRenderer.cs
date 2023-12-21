using SoftRenderingApp3D.Painter;

namespace SoftRenderingApp3D.Renderer {
    public interface IRenderer {
        RenderContext RenderContext { get; set; }
        IPainter Painter { get; set; }
        int[] Render();
    }
}