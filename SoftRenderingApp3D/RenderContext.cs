using SoftRenderingApp3D.Buffer;
using SoftRenderingApp3D.Camera;
using SoftRenderingApp3D.DataStructures.World;
using SoftRenderingApp3D.Projection;
using SoftRenderingApp3D.Renderer;

namespace SoftRenderingApp3D
{
    public class RenderContext
    {
        private IWorld world;
        public ICamera Camera { get; set; }

        public IWorld World
        {
            get { return world; }
            set
            {
                if(value.Equals(world))
                    return;
                world = value;
                WorldBuffer = new WorldBuffer(world);
            }
        }

        public IProjection Projection { get; set; }
        public RendererSettings RendererSettings { get; set; }
        public Stats Stats { get; set; }

        public FrameBuffer Surface { get; set; }
        internal WorldBuffer WorldBuffer { get; set; }
    }
}
