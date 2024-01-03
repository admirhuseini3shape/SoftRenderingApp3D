using System.Collections.Generic;

namespace SoftRenderingApp3D.DataStructures.Meshes
{
    public interface IMesh : IPointCloud
    {
        int FacetCount { get; }
        IReadOnlyList<Facet> Facets { get; }
    }
}
