using System;
using System.Buffers;
using System.Numerics;

namespace SoftRenderingApp3D {

    public class WorldBuffer : IDisposable {
        
        private static ArrayPool<VertexBuffer> vertexBuffer3bag = ArrayPool<VertexBuffer>.Shared;

        public VertexBuffer[] VertexBuffer { get; }
        int Size { get; }

        public WorldBuffer(IWorld w) {
            var volumes = w.Volumes;
            Size = volumes.Count;

            VertexBuffer = vertexBuffer3bag.Rent(Size);

            for(var i = 0; i < Size; i++) {
                VertexBuffer[i] = new VertexBuffer(volumes[i].Vertices.Length);
            }
        }
        
        public void AllocateVertexBuffer(int index) {
            if (index >= 0 && index < Size) {
                VertexBuffer[index].EnsureAllocated();
            }
        }

        public void Dispose() {
            var nv = VertexBuffer.Length;
            for(var i = 0; i < nv; i++) {
                VertexBuffer[i]?.Dispose();
            }
            Array.Clear(VertexBuffer, 0, Size);
            vertexBuffer3bag.Return(VertexBuffer, false);
        }
    }

    public class VertexBuffer : IDisposable {
        private static ArrayPool<Vector3> vector3bag = ArrayPool<Vector3>.Shared;
        private static ArrayPool<Vector4> vector4bag = ArrayPool<Vector4>.Shared;

        public IVolume Volume { get; set; }             // Volumes
        public IVolume Offset { get; set; }
        public Vector3[] ViewVertices { get; private set; }          // Vertices in view
        public Vector3[] WorldVertices { get; private set; }         // Vertices in world
        public Vector3[] WorldNormVertices { get; private set; }     // Vertices normals in world
        public Vector4[] ProjectionVertices { get; private set; }    // Vertices in frustum

        
        private bool isAllocated = false;
        
        int size { get; }

        public Matrix4x4 WorldMatrix { get; set; }

        public VertexBuffer(int vertexCount) {
            this.size = vertexCount;
            ViewVertices = vector3bag.Rent(vertexCount);
            WorldVertices = vector3bag.Rent(vertexCount);
            WorldNormVertices = vector3bag.Rent(vertexCount);
            ProjectionVertices = vector4bag.Rent(vertexCount);
        }
        
        public void EnsureAllocated() {
            if (!isAllocated) {
                ViewVertices = vector3bag.Rent(size);
                WorldVertices = vector3bag.Rent(size);
                WorldNormVertices = vector3bag.Rent(size);
                ProjectionVertices = vector4bag.Rent(size);
                isAllocated = true;
            }
        }

        public void Dispose() {
            Array.Clear(ViewVertices, 0, size);
            Array.Clear(WorldVertices, 0, size);
            Array.Clear(WorldNormVertices, 0, size);
            Array.Clear(ProjectionVertices, 0, size);

            vector3bag.Return(ViewVertices, false);
            vector3bag.Return(WorldVertices, false);
            vector3bag.Return(WorldNormVertices, false);
            vector4bag.Return(ProjectionVertices, false);
        }
    }
}