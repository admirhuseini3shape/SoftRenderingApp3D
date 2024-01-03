using SoftRenderingApp3D.DataStructures.Textures;
using System.Collections.Generic;

namespace SoftRenderingApp3D.DataStructures.Materials
{
    public interface IMaterial : IMaterialOptions { }

    public interface IVertexColorMaterial : IMaterial
    {
        IReadOnlyList<ColorRGB> VertexColors { get; }
    }

    public interface IFacetColorMaterial : IMaterial
    {
        IReadOnlyList<ColorRGB> FacetColors { get; }
    }

    public interface ITextureMaterial : IMaterial, ITextureMaterialOptions
    {
        Texture Texture { get; }
    }
}
