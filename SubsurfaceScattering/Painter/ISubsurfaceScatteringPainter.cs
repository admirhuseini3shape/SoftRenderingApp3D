using SoftRenderingApp3D.Buffer;

namespace SubsurfaceScattering.Painter {
    public interface ISubsurfaceScatteringPainter {
        SubsurfaceScatteringRenderContext RendererContext { get; set; }
        void DrawTriangle(VertexBuffer vbx, int triangleIndex);
    }
}
