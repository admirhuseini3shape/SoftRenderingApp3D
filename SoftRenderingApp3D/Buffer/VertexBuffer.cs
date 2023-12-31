using SoftRenderingApp3D.DataStructures.Meshes;
using SoftRenderingApp3D.DataStructures.World;
using SoftRenderingApp3D.Utils;
using System;
using System.Buffers;
using System.Numerics;

namespace SoftRenderingApp3D.Buffer
{
    public class WorldBuffer : IDisposable
    {
        private static readonly ArrayPool<VertexBuffer> VertexBuffer3Bag = ArrayPool<VertexBuffer>.Shared;

        public WorldBuffer(IWorld w)
        {
            var meshes = w.Meshes;
            Size = meshes.Count;

            VertexBuffer = VertexBuffer3Bag.Rent(Size);

            for(var i = 0; i < Size; i++)
            {
                VertexBuffer[i] = new VertexBuffer(meshes[i].Vertices.Count);
            }
        }

        public VertexBuffer[] VertexBuffer { get; }
        private int Size { get; }

        public void Dispose()
        {
            var nv = VertexBuffer.Length;
            for(var i = 0; i < nv; i++)
            {
                VertexBuffer[i]?.Dispose();
            }

            Array.Clear(VertexBuffer, 0, Size);
            VertexBuffer3Bag.Return(VertexBuffer);
        }
    }

    public class VertexBuffer : IDisposable
    {
        private static readonly ArrayPool<Vector3> Vector3Bag = ArrayPool<Vector3>.Shared;
        private static readonly ArrayPool<Vector4> Vector4Bag = ArrayPool<Vector4>.Shared;
        private static readonly ArrayPool<ColorRGB> ColorRGBBag = ArrayPool<ColorRGB>.Shared;

        public VertexBuffer(int vertexCount)
        {
            Size = vertexCount;
            ViewVertices = Vector3Bag.Rent(vertexCount);
            WorldVertices = Vector3Bag.Rent(vertexCount);
            WorldVertexNormals = Vector3Bag.Rent(vertexCount);
            ProjectionVertices = Vector4Bag.Rent(vertexCount);
            VertexColors = ColorRGBBag.Rent(vertexCount);
        }

        public IMesh Mesh { get; set; } // Meshes
        public Vector3[] ViewVertices { get; } // Vertices in view
        public Vector3[] WorldVertices { get; } // Vertices in _subsurfaceScatteringWorld
        public Vector3[] WorldVertexNormals { get; } // Vertices normals in _subsurfaceScatteringWorld
        public Vector4[] ProjectionVertices { get; } // Vertices in frustum
        public ColorRGB[] VertexColors { get; } // Vertex colors
        private int Size { get; }

        public Matrix4x4 WorldMatrix { get; set; }
        public Matrix4x4 WorldViewMatrix { get; set; }

        public void Dispose()
        {
            Array.Clear(ViewVertices, 0, Size);
            Array.Clear(WorldVertices, 0, Size);
            Array.Clear(WorldVertexNormals, 0, Size);
            Array.Clear(ProjectionVertices, 0, Size);
            Array.Clear(VertexColors, 0, Size);

            Vector3Bag.Return(ViewVertices);
            Vector3Bag.Return(WorldVertices);
            Vector3Bag.Return(WorldVertexNormals);
            Vector4Bag.Return(ProjectionVertices);
            ColorRGBBag.Return(VertexColors);
        }

        public void TransformVertices(Matrix4x4 matrix)
        {
            for(var i = 0; i < Mesh.Vertices.Count; i++)
            {
                WorldVertices[i] = matrix.Transform(Mesh.Vertices[i]);
                WorldVertexNormals[i] = matrix.TransformWithoutTranslation(Mesh.VertexNormals[i]);
            }
        }

        public void TransformWorld()
        {
            for(var i = 0; i < Mesh.Vertices.Count; i++)
            {
                WorldVertices[i] = Vector3.Transform(Mesh.Vertices[i], WorldMatrix);
            }
        }

        public void TransformWorldView()
        {
            for(var i = 0; i < Mesh.Vertices.Count; i++)
            {
                ViewVertices[i] = Vector3.Transform(Mesh.Vertices[i], WorldViewMatrix);
            }
        }
    }
}
