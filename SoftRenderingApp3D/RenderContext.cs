using SoftRenderingApp3D.Camera;
using SoftRenderingApp3D.Projection;
using SoftRenderingApp3D.Renderer;

namespace SoftRenderingApp3D
{
    public class RenderContext
    {
        public ICamera Camera { get; set; }
        public IProjection Projection { get; set; }
        public RendererSettings RendererSettings { get; set; }
    }
}
