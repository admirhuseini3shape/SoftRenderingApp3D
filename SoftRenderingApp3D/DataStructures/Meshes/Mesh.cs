using SoftRenderingApp3D.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace SoftRenderingApp3D.DataStructures.Meshes
{
    public class Mesh : IMesh
    {
        private readonly Vector3[] _vertices;
        private readonly Vector3[] _vertexNormals;
        private readonly ColorRGB[] _vertexColors;
        private readonly Vector2[] _texCoordinates;
        private readonly Triangle[] _triangles;
        private ColorRGB[] _triangleColors;

        public Mesh(Vector3[] vertices, Triangle[] triangleIndices,
            Vector3[] vertexNormals = null, ColorRGB[] vertexColors = null,
            Vector2[] textureCoordinates = null, ColorRGB[] triangleColors = null)
        {
            _vertices = vertices.Clone() as Vector3[];
            _triangles = triangleIndices.Clone() as Triangle[];

            if(vertexNormals == null)
                _vertexNormals = this.CalculateVertexNormals();
            else
                _vertexNormals = vertexNormals.Clone() as Vector3[];

            _vertexColors = vertexColors;
            _texCoordinates = textureCoordinates;
            if(triangleColors == null)
                InitializeTrianglesColor(ColorRGB.Gray);
            else
                _triangleColors = triangleColors.Clone() as ColorRGB[];
        }

        public IReadOnlyList<Vector3> Vertices => _vertices;

        public IReadOnlyList<Vector3> VertexNormals => _vertexNormals;

        public IReadOnlyList<ColorRGB> VertexColors => _vertexColors;

        public IReadOnlyList<Vector2> TexCoordinates => _texCoordinates;
        public void SetVertexPosition(int i, Vector3 position)
        {
            _vertices[i] = position;
        }

        public void SetVertexNormal(int i, Vector3 normal)
        {
            _vertexNormals[i] = normal;
        }

        public void SetVertexColor(int i, ColorRGB color)
        {
            _vertexColors[i] = color;
        }

        public void SetVertexTextureCoordinate(int i, Vector2 uv)
        {
            _texCoordinates[i] = uv;
        }

        public IReadOnlyList<Triangle> Triangles => _triangles;

        public IReadOnlyList<ColorRGB> TriangleColors => _triangleColors;


        public void InitializeTrianglesColor(ColorRGB color)
        {
            var count = _triangleColors != null && _triangleColors.Length != 0 ?
                _triangleColors.Length : _triangles.Length;

            _triangleColors = Enumerable.Repeat(color, count).ToArray();
        }

        public void Transform(Matrix4x4 matrix)
        {
            if(_vertices != null && _vertices.Length > 0)
            {
                for(var i = 0; i < _vertices.Length; i++)
                    _vertices[i] = matrix.Transform(_vertices[i]);
            }

            if(_vertexNormals != null && _vertexNormals.Length > 0)
            {
                for(var i = 0; i < _vertexNormals.Length; i++)
                    _vertexNormals[i] = matrix.TransformWithoutTranslation(_vertexNormals[i]);
            }

            //The  Vector3.TransformNormal does not account for scaling.
            //If the normals are unit vectors then the correct way to do it would be like follows

            //var success = Matrix4x4.Decompose(matrix, out _, out var rotation, out _);
            //if(!success)
            //    throw new Exception($"Could not decompose into rotation, scale and translation the matrix {matrix}!");
            //for(var i = 0; i < _vertexNormals.Length; i++)
            //    _vertexNormals[i] = Vector3.Transform(_vertexNormals[i], rotation);
        }
    }
}
