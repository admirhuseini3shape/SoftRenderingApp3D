using SoftRenderingApp3D.Buffer;
using SoftRenderingApp3D.DataStructures.Materials;
using SoftRenderingApp3D.Utils;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace SoftRenderingApp3D
{
    public readonly struct Facet
    {
        public Facet(int p0, int p1, int p2)
        {
            I0 = p0;
            I1 = p1;
            I2 = p2;
        }

        public int I0 { get; }
        public int I1 { get; }
        public int I2 { get; }

        public Vector3 CalculateCentroid(IReadOnlyList<Vector3> vertices)
        {
            return (vertices[I0] + vertices[I1] + vertices[I2]) / 3;
        }

        public float CalculateZAverages(IReadOnlyList<Vector4> vertices)
        {
            return (vertices[I0].Z + vertices[I1].Z + vertices[I2].Z) / 3;
        }

        public Vector3 CalculateNormal(IReadOnlyList<Vector3> vertices)
        {
            var edge1 = vertices[I1] - vertices[I0];
            var edge2 = vertices[I2] - vertices[I0];
            var normal = Vector3.Cross(edge1, edge2);
            return Vector3.Normalize(normal);
        }

        public bool Contains(Vector3 vertex, IReadOnlyList<Vector3> vertices)
        {
            return vertices[I0] == vertex || vertices[I1] == vertex || vertices[I2] == vertex;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsBehindFarPlane(VertexBuffer vertexBuffer)
        {
            var viewVertices = vertexBuffer.ViewVertices;
            return viewVertices[I0].Z > 0 && viewVertices[I1].Z > 0 && viewVertices[I2].Z > 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsFacingBack(VertexBuffer vertexBuffer)
        {
            var viewVertices = vertexBuffer.ViewVertices;
            var p0 = viewVertices[I0];
            var p1 = viewVertices[I1];
            var p2 = viewVertices[I2];

            var vCentroid = Vector3.Normalize((p0 + p1 + p2) / 3);
            var vNormal = Vector3.Normalize(Vector3.Cross(p1 - p0, p2 - p0));

            return Vector3.Dot(vCentroid, vNormal) >= 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsOutsideFrustum(VertexBuffer vertexBuffer)
        {
            var projectionVertices = vertexBuffer.ProjectionVertices;
            var p0 = projectionVertices[I0];
            var p1 = projectionVertices[I1];
            var p2 = projectionVertices[I2];

            // Will exit immediately if any w component is outside the frustum
            if (p0.W < 0 || p1.W < 0 || p2.W < 0) return true;

            // Check if all vertices are outside the same frustum plane to reduce redundant checks
            return (p0.X < -p0.W && p1.X < -p1.W && p2.X < -p2.W) ||
                   (p0.X > p0.W && p1.X > p1.W && p2.X > p2.W) ||
                   (p0.Y < -p0.W && p1.Y < -p1.W && p2.Y < -p2.W) ||
                   (p0.Y > p0.W && p1.Y > p1.W && p2.Y > p2.W) ||
                   (p0.Z > p0.W && p1.Z > p1.W && p2.Z > p2.W);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TransformProjection(VertexBuffer vertexBuffer, Matrix4x4 projectionMatrix)
        {
            var projectionVertices = vertexBuffer.ProjectionVertices;
            var viewVertices = vertexBuffer.ViewVertices;

            if(projectionVertices[I0] == Vector4.Zero)
            {
                projectionVertices[I0] = Vector4.Transform(viewVertices[I0], projectionMatrix);
            }

            if(projectionVertices[I1] == Vector4.Zero)
            {
                projectionVertices[I1] = Vector4.Transform(viewVertices[I1], projectionMatrix);
            }

            if(projectionVertices[I2] == Vector4.Zero)
            {
                projectionVertices[I2] = Vector4.Transform(viewVertices[I2], projectionMatrix);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TransformWorld(VertexBuffer vertexBuffer)
        {
            var worldMatrix = vertexBuffer.WorldMatrix;
            var mesh = vertexBuffer.Drawable.Mesh;
            var worldNormVertices = vertexBuffer.WorldVertexNormals;
            var normVertices = mesh.VertexNormals;
            var textureCoordinates = mesh.TextureCoordinates;

            if(worldNormVertices[I0] == Vector3.Zero)
            {
                worldNormVertices[I0] = Vector3.TransformNormal(normVertices[I0], worldMatrix);
            }

            if(worldNormVertices[I1] == Vector3.Zero)
            {
                worldNormVertices[I1] = Vector3.TransformNormal(normVertices[I1], worldMatrix);
            }

            if(worldNormVertices[I2] == Vector3.Zero)
            {
                worldNormVertices[I2] = Vector3.TransformNormal(normVertices[I2], worldMatrix);
            }

            /*var worldVertices = vertexBuffer.WorldVertices;
            if(worldVertices[I0] == Vector3.Zero)
                worldVertices[I0] = Vector3.Transform(worldVertices[I0], worldMatrix);

            if(worldVertices[I1] == Vector3.Zero)
                worldVertices[I1] = Vector3.Transform(worldVertices[I1], worldMatrix);

            if(worldVertices[I2] == Vector3.Zero)
                worldVertices[I2] = Vector3.Transform(worldVertices[I2], worldMatrix);
            */

            // Check if mesh has texture data
            if(!(vertexBuffer.Drawable.Material is ITextureMaterial) || mesh.TextureCoordinates == null)
                return;

            if(textureCoordinates[I0] == Vector2.Zero)
            {
                var temp = Vector3.Transform(new Vector3(textureCoordinates[I0].X, textureCoordinates[I0].Y, 1.0f),
                    worldMatrix);
                mesh.SetVertexTextureCoordinate(I0, new Vector2(temp.X, temp.Y));
            }

            if(textureCoordinates[I1] == Vector2.Zero)
            {
                var temp = Vector3.Transform(new Vector3(textureCoordinates[I1].X, textureCoordinates[I1].Y, 1.0f),
                    worldMatrix);
                mesh.SetVertexTextureCoordinate(I1, new Vector2(temp.X, temp.Y));
            }

            if(textureCoordinates[I2] == Vector2.Zero)
            {
                var temp = Vector3.Transform(new Vector3(textureCoordinates[I2].X, textureCoordinates[I2].Y, 1.0f),
                    worldMatrix);
                mesh.SetVertexTextureCoordinate(I2, new Vector2(temp.X, temp.Y));
            }
        }

        public Vector3 GetCenterCoordinates(VertexBuffer vertexBuffer)
        {
            var vertices = vertexBuffer.WorldVertices;
            var x = (vertices[I0].X + vertices[I1].X + vertices[I2].X) / 3.0f;
            var y = (vertices[I0].Y + vertices[I1].Y + vertices[I2].Y) / 3.0f;
            var z = (vertices[I0].Z + vertices[I1].Z + vertices[I2].Z) / 3.0f;

            return new Vector3(x, y, z);
        }
    }
}
