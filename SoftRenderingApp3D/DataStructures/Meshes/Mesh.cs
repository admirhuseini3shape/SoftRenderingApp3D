using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace SoftRenderingApp3D.DataStructures.Meshes
{
    public class Mesh : PointCloud, IMesh
    {
        private Facet[] _facets;

        public Mesh()
        {
            _facets = Array.Empty<Facet>();
        }

        public Mesh(Vector3[] vertices, Facet[] facets, Vector3[] vertexNormals = null, Vector2[] textureCoordinates = null) :
            base(vertices, vertexNormals, textureCoordinates)
        {
            _facets = facets.Clone() as Facet[];

            if(vertexNormals == null)
                _vertexNormals = this.CalculateVertexNormals();
        }

        public int FacetCount => _facets?.Length ?? 0;
        public IReadOnlyList<Facet> Facets => _facets;
        public (int[] vertexMapping, int[] facetMapping) Append(IMesh mesh)
        {
            var veCount = mesh.VertexCount;
            var faCount = mesh.FacetCount;

            if(veCount == 0 || faCount == 0 || mesh.Vertices == null || mesh.Facets == null)
                return (Array.Empty<int>(), Array.Empty<int>());

            var vertexMapping = new int[veCount];
            var facetMapping = new int[faCount];

            var verticesList = new List<Vector3>(_vertices);
            var normalsList = mesh.VertexNormals != null ? new List<Vector3>(_vertexNormals) : null;
            var texCoordsList = mesh.TextureCoordinates != null ? new List<Vector2>(_textureCoordinates) : null;
            var facetsList = new List<Facet>(_facets);

            for (var i = 0; i < veCount; i++)
            {
                vertexMapping[i] = verticesList.Count;
                verticesList.Add(mesh.Vertices[i]);

                if (normalsList != null && mesh.VertexNormals != null)
                    normalsList.Add(mesh.VertexNormals[i]);

                if (texCoordsList != null && mesh.TextureCoordinates != null)
                    texCoordsList.Add(mesh.TextureCoordinates[i]);
            }

            for (var i = 0; i < faCount; i++)
            {
                facetMapping[i] = facetsList.Count;
                var facet = mesh.Facets[i];
                facetsList.Add(new Facet(
                    vertexMapping[facet.I0],
                    vertexMapping[facet.I1],
                    vertexMapping[facet.I2]));
            }

            _vertices = verticesList.ToArray();
            _vertexNormals = normalsList?.ToArray();
            _textureCoordinates = texCoordsList?.ToArray();
            _facets = facetsList.ToArray();

            return (vertexMapping, facetMapping);
        }

        public (IReadOnlyList<int[]> vertexMappings, IReadOnlyList<int[]> facetMappings) Append(IReadOnlyList<IMesh> meshes)
        {
            var finalVerticesCount = meshes.Sum(x => x.VertexCount);
            var finalFacetsCount = meshes.Sum(x => x.FacetCount);

            Array.Resize(ref _vertices, finalVerticesCount);
            Array.Resize(ref _vertexNormals, finalVerticesCount);
            Array.Resize(ref _textureCoordinates, finalVerticesCount);
            Array.Resize(ref _facets, finalFacetsCount);

            var vertexMappings = new List<int[]>(meshes.Count);
            var facetMappings = new List<int[]>(meshes.Count);

            var currentVertexCount = 0;
            var currentFacetCount = 0;
            for(var k = 0; k < meshes.Count; k++)
            {
                var mesh = meshes[k];
                var veCount = mesh.VertexCount;
                var faCount = mesh.FacetCount;

                if(veCount == 0 || faCount == 0 || mesh.Vertices == null || mesh.Facets == null)
                {
                    vertexMappings.Add(Array.Empty<int>());
                    facetMappings.Add(Array.Empty<int>());
                    continue;
                }

                var vertexMapping = new int[veCount];
                for(var i = 0; i < veCount; i++)
                {
                    var newVeId = currentVertexCount + i;
                    vertexMapping[i] = newVeId;
                    _vertices[newVeId] = mesh.Vertices[i];
                }
                vertexMappings.Add(vertexMapping);

                if(mesh.VertexNormals != null)
                {
                    for(var i = 0; i < veCount; i++)
                        _vertexNormals[vertexMapping[i]] = mesh.VertexNormals[i];
                }

                if(mesh.TextureCoordinates != null)
                {
                    for(var i = 0; i < veCount; i++)
                        _textureCoordinates[vertexMapping[i]] = mesh.TextureCoordinates[i];
                }


                var facetMapping = new int[faCount];
                for(var i = 0; i < faCount; i++)
                {
                    var newFaId = currentFacetCount + i;
                    facetMapping[i] = newFaId;
                    var facet = mesh.Facets[i];
                    var newFacet = new Facet(
                        vertexMapping[facet.I0],
                        vertexMapping[facet.I1],
                        vertexMapping[facet.I2]);
                    _facets[newFaId] = newFacet;
                }
                facetMappings.Add(facetMapping);

                currentVertexCount += mesh.VertexCount;
                currentFacetCount += mesh.FacetCount;
            }

            return (vertexMappings, facetMappings);
        }

        public new object Clone()
        {
            return new Mesh(_vertices, _facets, _vertexNormals, _textureCoordinates);
        }
    }
}
