using SoftRenderingApp3D.DataStructures;
using SoftRenderingApp3D.DataStructures.Volume;
using SoftRenderingApp3D.DataStructures.World;
using System;
using System.Buffers;
using System.Numerics;

namespace SoftRenderingApp3D.Buffer {

    public class WorldBuffer : IDisposable {
        private static readonly ArrayPool<VertexBuffer> VertexBuffer3Bag = ArrayPool<VertexBuffer>.Shared;

        public VertexBuffer[] VertexBuffer { get; }
        int Size { get; }

        public WorldBuffer(IWorld w) {
            var volumes = w.Volumes;
            Size = volumes.Count;

            VertexBuffer = VertexBuffer3Bag.Rent(Size);

            for(var i = 0; i < Size; i++) {
                VertexBuffer[i] = new VertexBuffer(volumes[i].Vertices.Length);
            }
        }

        public void Dispose() {
            var nv = VertexBuffer.Length;
            for(var i = 0; i < nv; i++) {
                VertexBuffer[i]?.Dispose();
            }
            Array.Clear(VertexBuffer, 0, Size);
            VertexBuffer3Bag.Return(VertexBuffer, false);
        }
    }

    public class VertexBuffer : IDisposable {
        private static readonly ArrayPool<Vector3> Vector3Bag = ArrayPool<Vector3>.Shared;
        private static readonly ArrayPool<Vector4> Vector4Bag = ArrayPool<Vector4>.Shared;
        public IVolume Volume { get; set; }             // Volumes
        public Vector3[] ViewVertices { get; }          // Vertices in view
        public Vector3[] WorldVertices { get; }         // Vertices in _subsurfaceScatteringWorld
        public Vector3[] WorldNormVertices { get; }     // Vertices normals in _subsurfaceScatteringWorld
        public Vector4[] ProjectionVertices { get; }    // Vertices in frustum

        int Size { get; }

        public Matrix4x4 WorldMatrix { get; set; }
        public Matrix4x4 WorldViewMatrix { get; set; }

        public void TransformWorld() {
            for(int i = 0; i < Volume.Vertices.Length; i++)
                WorldVertices[i] = Vector3.Transform(Volume.Vertices[i].position, WorldMatrix);
        }

        public void TransformWorldView() {
            for(int i = 0; i < Volume.Vertices.Length; i++)
                ViewVertices[i] = Vector3.Transform(Volume.Vertices[i].position, WorldViewMatrix);
        }

        public VertexBuffer(int vertexCount) {
            this.Size = vertexCount;
            ViewVertices = Vector3Bag.Rent(vertexCount);
            WorldVertices = Vector3Bag.Rent(vertexCount);
            WorldNormVertices = Vector3Bag.Rent(vertexCount);
            ProjectionVertices = Vector4Bag.Rent(vertexCount);
        }

        public void Dispose() {
            Array.Clear(ViewVertices, 0, Size);
            Array.Clear(WorldVertices, 0, Size);
            Array.Clear(WorldNormVertices, 0, Size);
            Array.Clear(ProjectionVertices, 0, Size);

            Vector3Bag.Return(ViewVertices);
            Vector3Bag.Return(WorldVertices);
            Vector3Bag.Return(WorldNormVertices);
            Vector4Bag.Return(ProjectionVertices);
        }
    }
}