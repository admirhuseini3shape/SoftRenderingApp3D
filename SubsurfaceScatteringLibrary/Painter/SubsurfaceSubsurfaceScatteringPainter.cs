﻿using SoftRenderingApp3D.Buffer;
using SoftRenderingApp3D.Utils;
using SubsurfaceScatteringLibrary.Renderer;
using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace SubsurfaceScatteringLibrary.Painter
{
    internal class SubsurfaceSubsurfaceScatteringPainter : ISubsurfaceScatteringPainter
    {
        public SubsurfaceScatteringRenderContext RendererContext { get; set; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawTriangle(VertexBuffer vertexBuffer, int faId)
        {
            vertexBuffer.Drawable.Mesh.Facets[faId].TransformWorld(vertexBuffer);

            // Get z _subsurfaceScatteringWorld coordinate
            var zWorld = vertexBuffer.Drawable.Mesh.Facets[faId].CalculateCentroid(vertexBuffer.Drawable.Mesh.Vertices);

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

            var yMiddle = (int)p1.Y.Clamp(yStart, yEnd);

            // This has to move elsewhere
            var lightPos = new Vector3(0, 10, 10);

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
                PaintHalfTriangle(yStart, yMiddle, p0, p2, p0, p1, nl0, nl2, nl0, nl1, zWorld);
                PaintHalfTriangle(yMiddle + 1, yEnd, p0, p2, p1, p2, nl0, nl2, nl1, nl2, zWorld);
            }
            else
            {
                //   P0
                // P1 
                //   P2
                PaintHalfTriangle(yStart, yMiddle - 1, p0, p1, p0, p2, nl0, nl1, nl0, nl2, zWorld);
                PaintHalfTriangle(yMiddle, yEnd, p1, p2, p0, p2, nl1, nl2, nl0, nl2, zWorld);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void PaintHalfTriangle(int yStart, int yEnd, Vector3 pa, Vector3 pb, Vector3 pc, Vector3 pd, float nla,
            float nlb, float nlc, float nld, Vector3 zWorld)
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

                PaintScanline(y, sx, ex, sz, ez, sl, el, zWorld);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void PaintScanline(float y, float sx, float ex, float sz, float ez, float sl, float el,
            Vector3 zWorld)
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

                var finalColor = c * SubsurfaceScatteringRenderUtils.subsurfaceColor;

                surface.PutSubsurfacePixel((int)x, (int)y, z, finalColor, zWorld);
            }
        }
    }
}
