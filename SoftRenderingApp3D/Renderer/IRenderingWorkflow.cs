using SoftRenderingApp3D.DataStructures.Drawables;
using SoftRenderingApp3D.Shaders;
using System.Collections.Generic;

namespace SoftRenderingApp3D.Renderer
{
    public interface IRenderingWorkflow
    {
        IReadOnlyList<IDrawable>  Drawables { get; }

        int[] DrawFrame(IDrawable drawable, IShaderProvider shaderProvider);
        int[] DrawFrames(IReadOnlyList<IDrawable> drawables, IShaderProvider shaderProvider);
    }
}
