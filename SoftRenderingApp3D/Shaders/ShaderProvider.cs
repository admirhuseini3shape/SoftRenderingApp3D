using SoftRenderingApp3D.DataStructures.Drawables;
using SoftRenderingApp3D.Factories;

namespace SoftRenderingApp3D.Shaders
{
    public class ShaderProvider : IShaderProvider
    {
        private Factory<IShader> _factory = new Factory<IShader>();
        public IShader Create<TInput>(TInput input)
        {
            throw new System.NotImplementedException();
        }

        public void RegisterShaders()
        {
            //_factory.Add();
        }

        public IShader GetShader<TDrawable>(TDrawable drawable)
            where TDrawable : IDrawable
        {
            throw new System.NotImplementedException();
        }
    }
}
