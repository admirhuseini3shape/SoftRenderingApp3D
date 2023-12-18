using SoftRenderingApp3D;
using SoftRenderingApp3D.Buffer;
using SoftRenderingApp3D.DataStructures;
using SubsurfaceScatteringLibrary.Buffer;

namespace SubsurfaceScatteringLibrary.Renderer {

    public class SubsurfaceScatteringRenderContext {
        public ICamera Camera { get; set; }
        public IWorld World { get; set; }
        public IProjection Projection { get; set; }
        public SubsurfaceScatteringRendererSettings RendererSettings { get; set; }
        public Stats Stats { get; set; }

        public SubsurfaceScatteringFrameBuffer Surface { get; set; }
        internal WorldBuffer WorldBuffer { get; set; }
    }
}