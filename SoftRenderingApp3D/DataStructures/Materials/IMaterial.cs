using SoftRenderingApp3D.DataStructures.Textures;
using System.Collections.Generic;

namespace SoftRenderingApp3D.DataStructures.Materials
{
    public interface IMaterial : IMaterialOptions
    {
        void Append(IMaterial material, int[] vertexMapping, int[] facetMapping);
        void Append(IReadOnlyList<IMaterial> materials, IReadOnlyList<int[]> vertexMappings, IReadOnlyList<int[]> facetMappings);
    }

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
