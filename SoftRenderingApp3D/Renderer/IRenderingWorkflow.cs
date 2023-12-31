using SoftRenderingApp3D.DataStructures.Drawables;
using SoftRenderingApp3D.DataStructures.Materials;
using SoftRenderingApp3D.Shaders;
using System.Collections.Generic;

namespace SoftRenderingApp3D.Renderer
{
    public interface IRenderingWorkflow
    {
        IReadOnlyList<IDrawable<IMaterial>>  Drawables { get; }

        int[] DrawFrame(IDrawable<IMaterial> drawable, IShaderProvider shaderProvider);
        int[] DrawFrames(IReadOnlyList<IDrawable<IMaterial>> drawables, IShaderProvider shaderProvider);
    }
}
