using SoftRenderingApp3D;
using SoftRenderingApp3D.Buffer;
using SoftRenderingApp3D.Camera;
using SoftRenderingApp3D.DataStructures.Drawables;
using SoftRenderingApp3D.Projection;
using SubsurfaceScatteringLibrary.Buffer;
using System.Collections.Generic;

namespace SubsurfaceScatteringLibrary.Renderer
{
    public class SubsurfaceScatteringRenderContext
    {
        public ICamera Camera { get; set; }
        public IProjection Projection { get; set; }
        public SubsurfaceScatteringRendererSettings RendererSettings { get; set; }
        public Stats Stats { get; set; }
        public List<IDrawable> Drawables;
        public SubsurfaceScatteringFrameBuffer Surface { get; set; }
        internal AllVertexBuffers AllVertexBuffers { get; set; }
    }
}
