using System.Collections.Generic;

namespace SoftRenderingApp3D.DataStructures.Meshes
{
    public interface IMesh : IPointCloud
    {
        int FacetCount { get; }
        IReadOnlyList<Facet> Facets { get; }
        (int[] vertexMapping, int[] facetMapping) Append(IMesh mesh);
        (IReadOnlyList<int[]> vertexMappings, IReadOnlyList<int[]> facetMappings) 
            Append(IReadOnlyList<IMesh> meshes);
    }
}
