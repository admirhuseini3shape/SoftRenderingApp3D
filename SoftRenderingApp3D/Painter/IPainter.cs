namespace SoftRenderingApp3D {
    public interface IPainter {
        RenderContext RendererContext { get; set; }
        void DrawTriangle(VertexBuffer vbx, int triangleIndex);
    }
}
