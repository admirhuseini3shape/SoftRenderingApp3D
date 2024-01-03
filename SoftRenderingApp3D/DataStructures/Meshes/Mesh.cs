using System.Collections.Generic;
using System.Numerics;

namespace SoftRenderingApp3D.DataStructures.Meshes
{
    public class Mesh : PointCloud, IMesh
    {
        private readonly Facet[] _facets;

        public Mesh(Vector3[] vertices, Facet[] facets, Vector3[] vertexNormals = null, Vector2[] textureCoordinates = null) :
            base(vertices, vertexNormals, textureCoordinates)
        {
            _facets = facets.Clone() as Facet[];

            if(vertexNormals == null)
                _vertexNormals = this.CalculateVertexNormals();
        }

        public int FacetCount => _facets?.Length ?? 0;
        public IReadOnlyList<Facet> Facets => _facets;
    }
}
