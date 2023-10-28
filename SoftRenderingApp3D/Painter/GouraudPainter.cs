using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace SoftRenderingApp3D {

    public class GouraudPainter : IPainter {
        public RenderContext RendererContext { get; set; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawTriangle(VertexBuffer vbx, int triangleIndice) {
            vbx.Volume.Triangles[triangleIndice].TransformWorld(vbx);

            var surface = RendererContext.Surface;
            PainterUtils.SortTrianglePoints(vbx, surface, triangleIndice, out var v0, out var v1, out var v2, out var index0, out var index1, out var index2);

            var p0 = v0.ScreenPoint; var p1 = v1.ScreenPoint; var p2 = v2.ScreenPoint;

            var yStart = (int)Math.Max(p0.Y, 0);
            var yEnd = (int)Math.Min(p2.Y, surface.Height - 1);

            // Out if clipped
            if(yStart > yEnd) return;

            var yMiddle = MathUtils.Clamp((int)p1.Y, yStart, yEnd);

            // This has to move elsewhere
            // var lightPos = RendererContext.Camera.GetType() == typeof(ArcBallCam) ? -(RendererContext.Camera as ArcBallCam).Position : -(RendererContext.Camera as FlyCam).Position;
            var lightPos = new Vector3(0, 10, 50);

            // computing the cos of the angle between the light vector and the normal vector
            // it will return a value between 0 and 1 that will be used as the intensity of the color

            var nl0 = MathUtils.ComputeNDotL(v0.WorldPoint.position, v0.WorldNormal, lightPos);
            var nl1 = MathUtils.ComputeNDotL(v1.WorldPoint.position, v1.WorldNormal, lightPos);
            var nl2 = MathUtils.ComputeNDotL(v2.WorldPoint.position, v2.WorldNormal, lightPos);

            if(PainterUtils.Cross2D(p0, p1, p2) > 0) {
                // P0
                //   P1
                // P2
                paintHalfTriangle(yStart, (int)yMiddle - 1, p0, p2, p0, p1, nl0, nl2, nl0, nl1, v0, v1, v2);
                paintHalfTriangle((int)yMiddle, yEnd, p0, p2, p1, p2, nl0, nl2, nl1, nl2, v0, v1, v2);
            }
            else {
                //   P0
                // P1 
                //   P2
                paintHalfTriangle(yStart, (int)yMiddle - 1, p0, p1, p0, p2, nl0, nl1, nl0, nl2, v0, v1, v2);
                paintHalfTriangle((int)yMiddle, yEnd, p1, p2, p0, p2, nl1, nl2, nl0, nl2, v0, v1, v2);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void paintHalfTriangle(int yStart, int yEnd, Vector3 pa, Vector3 pb, Vector3 pc, Vector3 pd, float nla, float nlb, float nlc, float nld, PaintedVertex v0, PaintedVertex v1, PaintedVertex v2) {
            var mg1 = pa.Y == pb.Y ? 1f : 1 / (pb.Y - pa.Y);
            var mg2 = pd.Y == pc.Y ? 1f : 1 / (pd.Y - pc.Y);

            for(var y = yStart; y <= yEnd; y++) {
                var gradient1 = ((y - pa.Y) * mg1).Clamp();
                var gradient2 = ((y - pc.Y) * mg2).Clamp();

                var sx = MathUtils.Lerp(pa.X, pb.X, gradient1);
                var ex = MathUtils.Lerp(pc.X, pd.X, gradient2);

                if(sx >= ex) continue;

                var sl = MathUtils.Lerp(nla, nlb, gradient1);
                var el = MathUtils.Lerp(nlc, nld, gradient2);

                var sz = MathUtils.Lerp(pa.Z, pb.Z, gradient1);
                var ez = MathUtils.Lerp(pc.Z, pd.Z, gradient2);

                paintScanline(y, sx, ex, sz, ez, sl, el, v0, v1, v2);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void paintScanline(float y, float sx, float ex, float sz, float ez, float sl, float el, PaintedVertex v0, PaintedVertex v1, PaintedVertex v2) {
            var surface = RendererContext.Surface;

            var minX = Math.Max(sx, 0);
            var maxX = Math.Min(ex, surface.Width);

            var mx = 1 / (ex - sx);

            for(var x = minX; x < maxX; x++) {
                var gradient = (x - sx) * mx;

                var z = MathUtils.Lerp(sz, ez, gradient);
                var c = MathUtils.Lerp(sl, el, gradient);

                var surfaceColor = RenderUtils.surfaceColor;

                var scatteringColor = InterpolateTriangleVerticesColors((int)x, (int)y, (int)z, v0, v1, v2);

                var finalColor = RenderUtils.lightWeight * c * surfaceColor + RenderUtils.subsurfaceScatteringWeight * scatteringColor;
                ColorRGB finalColorWithAlpha = new ColorRGB(finalColor.R, finalColor.G, finalColor.B, (byte)(RenderUtils.surfaceOpacity * 255));
                //Console.WriteLine($"newColor {newColor}. alpha {newColor.Alpha}");
                surface.PutPixel((int)x, (int)y, (int)z, finalColorWithAlpha);
            }
        }

        ColorRGB InterpolateTriangleVerticesColors(int x, int y, int z, PaintedVertex v0, PaintedVertex v1, PaintedVertex v2) {
            // point to be colored
            var pointInTriangle = new Vector3(x, y, z);
            // calculate barycentric weight for each vertex
            var barycentric = GetBarycentricCoordinates(pointInTriangle, v0.ScreenPoint, v1.ScreenPoint, v2.ScreenPoint);
            // interpolate
            ColorRGB finalColor = barycentric.X * v0.WorldPoint.color + barycentric.Y * v1.WorldPoint.color + barycentric.Z * v2.WorldPoint.color;

            return finalColor;
        }

        // Efficient calculation of barycentric coordinates
        // taken from https://gamedev.stackexchange.com/a/23745
        Vector3 GetBarycentricCoordinates(Vector3 p, Vector3 v0, Vector3 v1, Vector3 v2) {
            var temp0 = v1 - v0;
            var temp1 = v2 - v0;
            var temp2 = p - v0;
            float d00 = Vector3.Dot(temp0, temp0);
            float d01 = Vector3.Dot(temp0, temp1);
            float d11 = Vector3.Dot(temp1, temp1);
            float d20 = Vector3.Dot(temp2, temp0);
            float d21 = Vector3.Dot(temp2, temp1);
            float denom = d00 * d11 - d01 * d01;
            var v = (d11 * d20 - d01 * d21) / denom;
            var w = (d00 * d21 - d01 * d20) / denom;
            var u = 1.0f - v - w;

            return new Vector3(u, v, w);
        }

    }
}
