using SoftRenderingApp3D.Buffer;
using SoftRenderingApp3D.Painter;
using SubsurfaceScatteringLibrary.Buffer;
using SubsurfaceScatteringLibrary.Utils;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace SubsurfaceScatteringLibrary.Painter {
    internal class SubsurfaceScatteringPainterUtils {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SortTrianglePoints(VertexBuffer vbx, SubsurfaceScatteringFrameBuffer frameBuffer,
            int triangleIndices, out PaintedVertex v0, out PaintedVertex v1, out PaintedVertex v2, out int index0,
            out int index1, out int index2) {
            var t = vbx.Mesh.Triangles[triangleIndices];

            var worldNormVertices = vbx.WorldVertexNormals;
            var projectionVertices = vbx.ProjectionVertices;
            var worldVertices = vbx.WorldVertices;

            v0 = new PaintedVertex(worldNormVertices[t.I0], 
                frameBuffer.ToScreen3(projectionVertices[t.I0]),
                worldVertices[t.I0], 
                vbx.VertexColors[t.I0]);
            v1 = new PaintedVertex(worldNormVertices[t.I1], 
                frameBuffer.ToScreen3(projectionVertices[t.I1]),
                worldVertices[t.I1],
                vbx.VertexColors[t.I1]);
            v2 = new PaintedVertex(worldNormVertices[t.I2], 
                frameBuffer.ToScreen3(projectionVertices[t.I2]),
                worldVertices[t.I2],
                vbx.VertexColors[t.I2]);

            index0 = t.I0;
            index1 = t.I1;
            index2 = t.I2;

            if(v0.ScreenPoint.Y > v1.ScreenPoint.Y) {
                SubsurfaceScatteringMiscUtils.Swap(ref v0, ref v1);
                SubsurfaceScatteringMiscUtils.Swap(ref index0, ref index1);
            }

            if(v1.ScreenPoint.Y > v2.ScreenPoint.Y) {
                SubsurfaceScatteringMiscUtils.Swap(ref v1, ref v2);
                SubsurfaceScatteringMiscUtils.Swap(ref index1, ref index2);
            }

            if(v0.ScreenPoint.Y > v1.ScreenPoint.Y) {
                SubsurfaceScatteringMiscUtils.Swap(ref v0, ref v1);
                SubsurfaceScatteringMiscUtils.Swap(ref index0, ref index1);
            }
        }

        // https://www.geeksforgeeks.org/orientation-3-ordered-points/

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Cross2D(Vector3 p0, Vector3 p1, Vector3 p2) {
            return (p1.X - p0.X) * (p2.Y - p1.Y) - (p1.Y - p0.Y) * (p2.X - p1.X);
        }
    }
}