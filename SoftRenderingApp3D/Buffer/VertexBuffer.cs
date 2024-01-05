using SoftRenderingApp3D.DataStructures.Drawables;
using SoftRenderingApp3D.Utils;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Numerics;

namespace SoftRenderingApp3D.Buffer
{
    public class AllVertexBuffers : IDisposable
    {
        private static readonly ArrayPool<VertexBuffer> VertexBuffer3Bag = ArrayPool<VertexBuffer>.Shared;

        public AllVertexBuffers(IReadOnlyList<IDrawable> drawables)
        {
            Size = drawables.Count;

            VertexBuffer = VertexBuffer3Bag.Rent(Size);

            for(var i = 0; i < Size; i++)
                VertexBuffer[i] = new VertexBuffer(drawables[i].Mesh.Vertices.Count);
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
        private static readonly ArrayPool<ColorRGB> ColorRgbBag = ArrayPool<ColorRGB>.Shared;

        public VertexBuffer(int vertexCount)
        {
            Size = vertexCount;
            ViewVertices = Vector3Bag.Rent(vertexCount);
            WorldVertices = Vector3Bag.Rent(vertexCount);
            WorldVertexNormals = Vector3Bag.Rent(vertexCount);
            ProjectionVertices = Vector4Bag.Rent(vertexCount);
            VertexColors = ColorRgbBag.Rent(vertexCount);
        }

        public IDrawable Drawable { get; set; } // Drawables
        public Vector3[] ViewVertices { get; } // Vertices in view
        public Vector3[] WorldVertices { get; } // Vertices in _subsurfaceScatteringWorld
        public Vector3[] WorldVertexNormals { get; } // Vertices normals in _subsurfaceScatteringWorld
        public Vector4[] ProjectionVertices { get; } // Vertices in frustum
        public ColorRGB[] VertexColors { get; } // Vertex colors
        private int Size { get; }

        public Matrix4x4 WorldMatrix { get; set; }
        public Matrix4x4 WorldViewMatrix { get; set; }

        public void Clear()
        {
            Array.Clear(ViewVertices, 0, Size);
            Array.Clear(WorldVertices, 0, Size);
            Array.Clear(WorldVertexNormals, 0, Size);
            Array.Clear(ProjectionVertices, 0, Size);
            Array.Clear(VertexColors, 0, Size);
        }

        public void Dispose()
        {
            Clear();

            Vector3Bag.Return(ViewVertices);
            Vector3Bag.Return(WorldVertices);
            Vector3Bag.Return(WorldVertexNormals);
            Vector4Bag.Return(ProjectionVertices);
            ColorRgbBag.Return(VertexColors);
        }

        public void TransformVertices(Matrix4x4 matrix)
        {
            var mesh = Drawable.Mesh;
            for(var i = 0; i < mesh.Vertices.Count; i++)
            {
                WorldVertices[i] = matrix.Transform(mesh.Vertices[i]);
                WorldVertexNormals[i] = matrix.TransformWithoutTranslation(mesh.VertexNormals[i]);
            }
        }

        public void TransformWorld()
        {
            for(var i = 0; i < Drawable.Mesh.Vertices.Count; i++)
            {
                WorldVertices[i] = Vector3.Transform(Drawable.Mesh.Vertices[i], WorldMatrix);
            }
        }

        public void TransformWorldView()
        {
            for(var i = 0; i < Drawable.Mesh.Vertices.Count; i++)
            {
                ViewVertices[i] = Vector3.Transform(Drawable.Mesh.Vertices[i], WorldViewMatrix);
            }
        }
    }
}
