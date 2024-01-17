using SoftRenderingApp3D.Utils;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace SoftRenderingApp3D.DataStructures.Meshes
{
    public class PointCloud : IPointCloud
    {
        protected Vector3[] _vertices;
        protected Vector3[] _vertexNormals;
        protected Vector2[] _textureCoordinates;

        public int VertexCount => _vertices?.Length ?? 0;

        public IReadOnlyList<Vector3> Vertices => _vertices;

        public IReadOnlyList<Vector3> VertexNormals => _vertexNormals;
        public IReadOnlyList<Vector2> TextureCoordinates => _textureCoordinates;

        public PointCloud(int vertexCapacity = -1)
        {
            _vertices =  Array.Empty<Vector3>();
            _vertexNormals = Array.Empty<Vector3>();
            _textureCoordinates = Array.Empty<Vector2>();
        }

        public PointCloud(Vector3[] vertices, Vector3[] vertexNormals = null, Vector2[] textureCoordinates = null)
        {
            _vertices = vertices.Clone() as Vector3[];
            _vertexNormals = vertexNormals?.Clone() as Vector3[];
            _textureCoordinates = textureCoordinates?.Clone() as Vector2[];
        }

        public void SetVertexPosition(int i, Vector3 position)
        {
            _vertices[i] = position;
        }

        public void SetVertexNormal(int i, Vector3 normal)
        {
            _vertexNormals[i] = normal;
        }

        public void SetVertexTextureCoordinate(int i, Vector2 uv)
        {
            _textureCoordinates[i] = uv;
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

        public object Clone()
        {
            return new PointCloud(_vertices, _vertexNormals, _textureCoordinates);
        }
    }
}
