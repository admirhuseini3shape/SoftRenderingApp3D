using SoftRenderingApp3D.Buffer;
using SubsurfaceScatteringLibrary.Renderer;

namespace SubsurfaceScatteringLibrary.Painter
{
    public interface ISubsurfaceScatteringPainter
    {
        SubsurfaceScatteringRenderContext RendererContext { get; set; }
        void DrawTriangle(VertexBuffer vertexBuffer, int faId);
    }
}
