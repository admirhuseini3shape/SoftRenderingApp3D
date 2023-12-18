using SoftRenderingApp3D;
using SubsurfaceScattering.Painter;

namespace SubsurfaceScattering.Renderer {
    public interface ISubsurfaceScatteringRenderer {
        SubsurfaceScatteringRenderContext SubsurfaceScatteringRenderContext { get; set; }
        ISubsurfaceScatteringPainter SubsurfaceScatteringPainter { get; set; }
        int[] Render();
    }
}