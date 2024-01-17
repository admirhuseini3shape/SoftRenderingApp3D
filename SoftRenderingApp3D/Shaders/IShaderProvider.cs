using SoftRenderingApp3D.DataStructures.Drawables;
using SoftRenderingApp3D.Factories;

namespace SoftRenderingApp3D.Shaders
{
    public interface IShaderProvider : IFactory<IShader>
    {
        void RegisterShaders();

        IShader GetShader<TDrawable>(TDrawable drawable) where TDrawable : IDrawable;
    }
}
