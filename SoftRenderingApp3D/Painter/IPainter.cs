using SoftRenderingApp3D.Buffer;

namespace SoftRenderingApp3D.Painter {
    public interface IPainter {
        RenderContext RendererContext { get; set; }
        void DrawTriangle(VertexBuffer vbx, int triangleIndex);
    }
}