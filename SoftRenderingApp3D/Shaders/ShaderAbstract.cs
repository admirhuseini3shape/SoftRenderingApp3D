using SoftRenderingApp3D.Buffer;
using SoftRenderingApp3D.DataStructures.Drawables;
using SoftRenderingApp3D.DataStructures.Materials;

namespace SoftRenderingApp3D.Shaders
{
    public abstract class ShaderAbstract<TMaterial, TVertexOutput, TFragmentOutput> : IShader<TMaterial, TVertexOutput, TFragmentOutput>
    where TMaterial : IMaterial
    {
        public virtual VertexBuffer VertexBuffer { get; }
        public virtual FrameBuffer FrameBuffer { get; }

        public abstract TVertexOutput[] VertexShader(IDrawable<TMaterial> drawable);
        public abstract TFragmentOutput FragmentShader(IDrawable<TMaterial> drawable, TVertexOutput[] vertexOutput);
        public abstract TFragmentOutput PostProcess(IDrawable<TMaterial> drawable,
            TVertexOutput[] vertexOutput, TFragmentOutput frameOutput);

        public TFragmentOutput RenderFrame(IDrawable<TMaterial> drawable)
        {
            var vertexOutput = VertexShader(drawable);
            var fragment = FragmentShader(drawable, vertexOutput);
            var result = PostProcess(drawable, vertexOutput, fragment);
            return result;
        }
    }
}
