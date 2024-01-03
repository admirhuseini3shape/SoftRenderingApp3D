using SoftRenderingApp3D.Buffer;
using SoftRenderingApp3D.DataStructures.Drawables;
using SoftRenderingApp3D.DataStructures.Materials;

namespace SoftRenderingApp3D.Shaders
{
    public interface IVertexOutput{}
    public interface IFragmentOutput{}

    public interface IShader:IShader<IMaterial, IVertexOutput, IFragmentOutput>{}

    public interface IShader<TMaterial, TVertexOutput, TFragmentOutput>
    where TMaterial : IMaterial
    where TVertexOutput : IVertexOutput
    where TFragmentOutput : IFragmentOutput
    {
        VertexBuffer VertexBuffer { get; }
        FrameBuffer FrameBuffer { get; }

        TVertexOutput[] VertexShader(IDrawable<TMaterial> drawable);
        TFragmentOutput FragmentShader(IDrawable<TMaterial> drawable, TVertexOutput[] vertexOutput);
        TFragmentOutput PostProcess(IDrawable<TMaterial> drawable,
            TVertexOutput[] vertexOutput, TFragmentOutput frameOutput);

        TFragmentOutput RenderFrame(IDrawable<TMaterial> drawable);
    }
}
