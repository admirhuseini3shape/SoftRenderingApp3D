﻿using SoftRenderingApp3D;
using SoftRenderingApp3D.Buffer;
using SoftRenderingApp3D.Painter;
using SoftRenderingApp3D.Utils;
using SubsurfaceScatteringLibrary.Renderer;
using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace SubsurfaceScatteringLibrary.Painter
{
    public class GouraudSubsurfaceScatteringPainter : ISubsurfaceScatteringPainter
    {
        public SubsurfaceScatteringRenderContext RendererContext { get; set; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawTriangle(VertexBuffer vertexBuffer, int faId)
        {
            vertexBuffer.Drawable.Mesh.Facets[faId].TransformWorld(vertexBuffer);

            var surface = RendererContext.Surface;
            SubsurfaceScatteringPainterUtils.SortTrianglePoints(vertexBuffer, surface, faId, out var v0, out var v1,
                out var v2, out _, out _, out _);

            var p0 = v0.ScreenPoint;
            var p1 = v1.ScreenPoint;
            var p2 = v2.ScreenPoint;

            var yStart = (int)Math.Max(p0.Y, 0);
            var yEnd = (int)Math.Min(p2.Y, surface.Height - 1);

            // Out if clipped
            if(yStart > yEnd)
            {
                return;
            }

            var yMiddle = MathUtils.Clamp((int)p1.Y, yStart, yEnd);

            // This has to move elsewhere
            // var lightPos = RendererContext.Camera.GetType() == typeof(ArcBallCam) ? -(RendererContext.Camera as ArcBallCam).Position : -(RendererContext.Camera as FlyCam).Position;
            var lightPos = new Vector3(0, 10, 50);

            // computing the cos of the angle between the light vector and the normal vector
            // it will return a value between 0 and 1 that will be used as the intensity of the color

            var nl0 = MathUtils.ComputeNDotL(v0.WorldPoint, v0.WorldNormal, lightPos);
            var nl1 = MathUtils.ComputeNDotL(v1.WorldPoint, v1.WorldNormal, lightPos);
            var nl2 = MathUtils.ComputeNDotL(v2.WorldPoint, v2.WorldNormal, lightPos);

            if(SubsurfaceScatteringPainterUtils.Cross2D(p0, p1, p2) > 0)
            {
                // P0
                //   P1
                // P2
                PaintHalfTriangle(yStart, (int)yMiddle - 1, p0, p2, p0, p1, nl0, nl2, nl0, nl1, v0, v1, v2);
                PaintHalfTriangle((int)yMiddle, yEnd, p0, p2, p1, p2, nl0, nl2, nl1, nl2, v0, v1, v2);
            }
            else
            {
                //   P0
                // P1 
                //   P2
                PaintHalfTriangle(yStart, (int)yMiddle - 1, p0, p1, p0, p2, nl0, nl1, nl0, nl2, v0, v1, v2);
                PaintHalfTriangle((int)yMiddle, yEnd, p1, p2, p0, p2, nl1, nl2, nl0, nl2, v0, v1, v2);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void PaintHalfTriangle(int yStart, int yEnd, Vector3 pa, Vector3 pb, Vector3 pc, Vector3 pd, float nla,
            float nlb, float nlc, float nld, PaintedVertex v0, PaintedVertex v1, PaintedVertex v2)
        {
            var mg1 = pa.Y == pb.Y ? 1f : 1 / (pb.Y - pa.Y);
            var mg2 = pd.Y == pc.Y ? 1f : 1 / (pd.Y - pc.Y);

            for(var y = yStart; y <= yEnd; y++)
            {
                var gradient1 = ((y - pa.Y) * mg1).Clamp();
                var gradient2 = ((y - pc.Y) * mg2).Clamp();

                var sx = MathUtils.Lerp(pa.X, pb.X, gradient1);
                var ex = MathUtils.Lerp(pc.X, pd.X, gradient2);

                if(sx >= ex)
                {
                    continue;
                }

                var sl = MathUtils.Lerp(nla, nlb, gradient1);
                var el = MathUtils.Lerp(nlc, nld, gradient2);

                var sz = MathUtils.Lerp(pa.Z, pb.Z, gradient1);
                var ez = MathUtils.Lerp(pc.Z, pd.Z, gradient2);

                PaintScanline(y, sx, ex, sz, ez, sl, el, v0, v1, v2);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void PaintScanline(float y, float sx, float ex, float sz, float ez, float sl, float el,
            PaintedVertex v0, PaintedVertex v1, PaintedVertex v2)
        {
            var surface = RendererContext.Surface;

            var minX = Math.Max(sx, 0);
            var maxX = Math.Min((double)ex, surface.Width);

            var mx = 1 / (ex - sx);

            for(var x = minX; x < maxX; x++)
            {
                var gradient = (x - sx) * mx;

                var z = MathUtils.Lerp(sz, ez, gradient);
                var c = MathUtils.Lerp(sl, el, gradient);

                var surfaceColor = SubsurfaceScatteringRenderUtils.surfaceColor;
                var scatteringColor = InterpolateTriangleVerticesColors(x, y, z, v0, v1, v2);

                var R = (int)(SubsurfaceScatteringRenderUtils.LightWeight * c * surfaceColor.R) +
                        (int)(SubsurfaceScatteringRenderUtils.SubsurfaceScatteringWeight * scatteringColor.R);
                var G = (int)(SubsurfaceScatteringRenderUtils.LightWeight * c * surfaceColor.G) +
                        (int)(SubsurfaceScatteringRenderUtils.SubsurfaceScatteringWeight * scatteringColor.G);
                var B = (int)(SubsurfaceScatteringRenderUtils.LightWeight * c * surfaceColor.B) +
                        (int)(SubsurfaceScatteringRenderUtils.SubsurfaceScatteringWeight * scatteringColor.B);

                var finalColor = new ColorRGB((byte)R, (byte)G, (byte)B, 255);
                //Console.WriteLine($"newColor {newColor}. alpha {newColor.Alpha}");
                surface.PutPixel((int)x, (int)y, (int)z, finalColor);
            }
        }

        private ColorRGB InterpolateTriangleVerticesColors(float x, float y, float z, PaintedVertex v0,
            PaintedVertex v1, PaintedVertex v2)
        {
            // point to be colored
            var pointInTriangle = new Vector3(x, y, z);
            // calculate barycentric weight for each vertex
            var barycentric =
                GetBarycentricCoordinates(pointInTriangle, v0.ScreenPoint, v1.ScreenPoint, v2.ScreenPoint);
            var maxR = Math.Max(v0.Color.R, Math.Max(v1.Color.R, v2.Color.R));
            var maxG = Math.Max(v0.Color.G, Math.Max(v1.Color.G, v2.Color.G));
            var maxB = Math.Max(v0.Color.B, Math.Max(v1.Color.B, v2.Color.B));
            // interpolate
            var R = (int)(barycentric.X * v0.Color.R + barycentric.Y * v1.Color.R +
                          barycentric.Z * v2.Color.R).Clamp(0, maxR);
            var G = (int)(barycentric.X * v0.Color.G + barycentric.Y * v1.Color.G +
                          barycentric.Z * v2.Color.G).Clamp(0, maxG);
            var B = (int)(barycentric.X * v0.Color.B + barycentric.Y * v1.Color.B +
                          barycentric.Z * v2.Color.B).Clamp(0, maxB);
            var finalColor = new ColorRGB((byte)R, (byte)G, (byte)B, 255);


            return finalColor;
        }

        private bool CheckIfBarycentricOutsideTriangle(Vector3 barycentric)
        {
            return barycentric.X < 0 || barycentric.X > 1
                                     || barycentric.Y < 0 || barycentric.Y > 1
                                     || barycentric.Z < 0 || barycentric.Z > 1
                                     || barycentric.X + barycentric.Y + barycentric.Z > 1;
        }

        // deals with edge cases in the scanline algorithm
        private Vector3 GetAdjustedBarycentric(Vector3 barycentric)
        {
            if(barycentric.X > 1)
            {
                return new Vector3(1, 0, 0);
            }

            if(barycentric.Y > 1)
            {
                return new Vector3(0, 1, 0);
            }

            if(barycentric.Z > 1)
            {
                return new Vector3(0, 0, 1);
            }

            if(barycentric.X < 0)
            {
                return new Vector3(0, barycentric.Y, barycentric.Z);
            }

            if(barycentric.Y < 0)
            {
                return new Vector3(barycentric.X, 0, barycentric.Z);
            }

            if(barycentric.Z < 0)
            {
                return new Vector3(barycentric.X, barycentric.Y, 0);
            }

            if(barycentric.X + barycentric.Y + barycentric.Z > 1)
            {
                var sumDenom = 1 / (barycentric.X + barycentric.Y + barycentric.Z);
                return new Vector3(barycentric.X * sumDenom, barycentric.Y * sumDenom, barycentric.Z * sumDenom);
            }

            throw new Exception("something not good if this happens");
        }


        private Vector3 GetBarycentricCoordinates(Vector3 p, Vector3 v0, Vector3 v1, Vector3 v2)
        {
            var barycentric = CalculateBarycentricCoordinates(p, v0, v1, v2);
            if(CheckIfBarycentricOutsideTriangle(barycentric))
            {
                return GetAdjustedBarycentric(barycentric);
            }

            return barycentric;
        }

        // Efficient calculation of barycentric coordinates
        // taken from https://gamedev.stackexchange.com/a/23745
        private Vector3 CalculateBarycentricCoordinates(Vector3 p, Vector3 v0, Vector3 v1, Vector3 v2)
        {
            var edge0 = v1 - v0;
            var edge1 = v2 - v0;
            var n = Vector3.Cross(edge0, edge1);

            var na = Vector3.Cross(v2 - v1, p - v1);
            var nb = Vector3.Cross(-edge1, p - v2); //v0 - v2 == -edge1 
            var nc = Vector3.Cross(edge0, p - v0); // Reuse edge0

            var normFactor = 1 / Vector3.Dot(n, n);

            var alpha = Vector3.Dot(n, na) * normFactor;
            var beta = Vector3.Dot(n, nb) * normFactor;
            var gamma = Vector3.Dot(n, nc) * normFactor;

            return new Vector3(alpha, beta, gamma);
        }
    }
}
