using System.Numerics;
using System.Runtime.CompilerServices;

namespace SoftRenderingApp3D {
    public struct Triangle {
        public Triangle(int p0, int p1, int p2) {
            I0 = p0;
            I1 = p1;
            I2 = p2;
        }

        public int I0 { get; }
        public int I1 { get; }
        public int I2 { get; }

        public Vector3 CalculateCentroid(ColoredVertex[] vertices) => (vertices[I0].position + vertices[I1].position + vertices[I2].position) / 3;

        public Vector3 CalculateNormal(ColoredVertex[] vertices) {
            // Avoid division with zero
            if(vertices[I0].position == Vector3.Zero && vertices[I1].position == Vector3.Zero && vertices[I2].position == Vector3.Zero)
                return Vector3.Zero;
            var result = Vector3.Normalize(Vector3.Cross(vertices[I1].position - vertices[I0].position, vertices[I2].position - vertices[I0].position));
            return (float.IsNaN(result.X) || float.IsNaN(result.Y) || float.IsNaN(result.Z)) ? Vector3.Zero : result;
        }

        public bool Contains(ColoredVertex vertex, ColoredVertex[] vertices) => vertices[I0] == vertex || vertices[I1] == vertex || vertices[I2] == vertex;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsBehindFarPlane(VertexBuffer vbx) {
            var viewVertices = vbx.ViewVertices;
            return viewVertices[I0].Z > 0 && viewVertices[I1].Z > 0 && viewVertices[I2].Z > 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsFacingBack(VertexBuffer vbx) {
            var viewVertices = vbx.ViewVertices;
            var p0 = viewVertices[I0]; var p1 = viewVertices[I1]; var p2 = viewVertices[I2];
 
            var vCentroid = Vector3.Normalize((p0 + p1 + p2) / 3);
            var vNormal = Vector3.Normalize(Vector3.Cross(p1 - p0, p2 - p0));
            
            return Vector3.Dot(vCentroid, vNormal) >= 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool isOutsideFrustum(VertexBuffer vbx) {
            var projectionVertices = vbx.ProjectionVertices;
            var p0 = projectionVertices[I0]; var p1 = projectionVertices[I1]; var p2 = projectionVertices[I2];

            if(p0.W < 0 || p1.W < 0 || p2.W < 0)
                return true;

            if(p0.X < -p0.W && p1.X < -p1.W && p2.X < -p2.W)
                return true;

            if(p0.X > p0.W && p1.X > p1.W && p2.X > p2.W)
                return true;

            if(p0.Y < -p0.W && p1.Y < -p1.W && p2.Y < -p2.W)
                return true;

            if(p0.Y > p0.W && p1.Y > p1.W && p2.Y > p2.W)
                return true;

            if(p0.Z > p0.W && p1.Z > p1.W && p2.Z > p2.W)
                return true;

            // This last one is normally not necessary when a IsTriangleBehind check is done
            if(p0.Z < 0 && p1.Z < 0 && p2.Z < 0)
                return true;

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TransformProjection(VertexBuffer vbx, Matrix4x4 projectionMatrix) {
            var projectionVertices = vbx.ProjectionVertices;
            var viewVertices = vbx.ViewVertices;

            if(projectionVertices[I0] == Vector4.Zero)
                projectionVertices[I0] = Vector4.Transform(viewVertices[I0], projectionMatrix);

            if(projectionVertices[I1] == Vector4.Zero)
                projectionVertices[I1] = Vector4.Transform(viewVertices[I1], projectionMatrix);

            if(projectionVertices[I2] == Vector4.Zero)
                projectionVertices[I2] = Vector4.Transform(viewVertices[I2], projectionMatrix);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TransformWorld(VertexBuffer vbx) {
            var worldMatrix = vbx.WorldMatrix;

            var worldNormVertices = vbx.WorldNormVertices;
            var normVertices = vbx.Volume.NormVertices;
            var textureCoordinates = vbx.Volume.TexCoordinates;

            if(worldNormVertices[I0] == Vector3.Zero)
                worldNormVertices[I0] = Vector3.TransformNormal(normVertices[I0], worldMatrix);

            if(worldNormVertices[I1] == Vector3.Zero)
                worldNormVertices[I1] = Vector3.TransformNormal(normVertices[I1], worldMatrix);

            if(worldNormVertices[I2] == Vector3.Zero)
                worldNormVertices[I2] = Vector3.TransformNormal(normVertices[I2], worldMatrix);

            /*var worldVertices = vbx.WorldVertices;
            if(worldVertices[I0] == Vector3.Zero)
                worldVertices[I0] = Vector3.Transform(worldVertices[I0], worldMatrix);

            if(worldVertices[I1] == Vector3.Zero)
                worldVertices[I1] = Vector3.Transform(worldVertices[I1], worldMatrix);

            if(worldVertices[I2] == Vector3.Zero)
                worldVertices[I2] = Vector3.Transform(worldVertices[I2], worldMatrix);
            */

            // Check if volume has texture data
            if(vbx.Volume.TexCoordinates != null) {
                if(textureCoordinates[I0] == Vector2.Zero) {
                    var temp = Vector3.Transform(new Vector3(textureCoordinates[I0].X, textureCoordinates[I0].Y, 1.0f), worldMatrix);
                    textureCoordinates[I0] = new Vector2(temp.X, temp.Y);
                }
                if(textureCoordinates[I1] == Vector2.Zero) {
                    var temp = Vector3.Transform(new Vector3(textureCoordinates[I1].X, textureCoordinates[I1].Y, 1.0f), worldMatrix);
                    textureCoordinates[I1] = new Vector2(temp.X, temp.Y);
                }
                if(textureCoordinates[I2] == Vector2.Zero) {
                    var temp = Vector3.Transform(new Vector3(textureCoordinates[I2].X, textureCoordinates[I2].Y, 1.0f), worldMatrix);
                    textureCoordinates[I2] = new Vector2(temp.X, temp.Y);
                }
            }


        }

        public Vector3 GetCenterCoordinates(VertexBuffer vbx) {
            var x = (vbx.WorldVertices[I0].X + vbx.WorldVertices[I1].X + vbx.WorldVertices[I2].X) / 3.0f;
            var y = (vbx.WorldVertices[I0].Y + vbx.WorldVertices[I1].Y + vbx.WorldVertices[I2].Y) / 3.0f;
            var z = (vbx.WorldVertices[I0].Z + vbx.WorldVertices[I1].Z + vbx.WorldVertices[I2].Z) / 3.0f;

            return new Vector3(x, y, z);
        }
    }
}