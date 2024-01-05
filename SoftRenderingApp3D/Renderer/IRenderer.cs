using SoftRenderingApp3D.Buffer;
using SoftRenderingApp3D.DataStructures.Drawables;
using SoftRenderingApp3D.Painter;
using System.Collections.Generic;
using System.Numerics;

namespace SoftRenderingApp3D.Renderer
{
    public interface IRenderer
    {
        int[] Render(AllVertexBuffers allVertexBuffers, FrameBuffer frameBuffer, IPainter painter, 
            IList<IDrawable> drawables, Stats stats, Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix, 
            RendererSettings rendererSettings);
    }
}
