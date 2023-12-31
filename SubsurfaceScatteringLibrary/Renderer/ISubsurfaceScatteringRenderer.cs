using SubsurfaceScatteringLibrary.Painter;

namespace SubsurfaceScatteringLibrary.Renderer
{
    public interface ISubsurfaceScatteringRenderer
    {
        SubsurfaceScatteringRenderContext SubsurfaceScatteringRenderContext { get; set; }
        ISubsurfaceScatteringPainter SubsurfaceScatteringPainter { get; set; }
        int[] Render();
    }
}
