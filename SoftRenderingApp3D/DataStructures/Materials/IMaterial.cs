using SoftRenderingApp3D.DataStructures.Textures;

namespace SoftRenderingApp3D.DataStructures.Materials
{
    public interface IMaterial : IMaterialOptions
    {
    }

    public interface IVertexColorMaterial
    {
        ColorRGB[] VertexColors { get; }
    }

    public interface IFacetColorMaterial
    {
        ColorRGB[] VertexColors { get; }
    }

    public interface ITextureMaterial : ITextureMaterialOptions
    {
        Texture Texture { get; }
    }
}
