using SoftRenderingApp3D.Buffer;
using SoftRenderingApp3D.Utils;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace SoftRenderingApp3D.Painter
{
    public static class ScanLine
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<Vector4> ScanLineTriangle(VertexBuffer vertexBuffer, int height, int width, int i0, int i1, int i2)
        {
            var result = new List<Vector4>();

            var p0 = vertexBuffer.ScreenPointVertices[i0];
            var p1 = vertexBuffer.ScreenPointVertices[i1];
            var p2 = vertexBuffer.ScreenPointVertices[i2];

            //if(p0.IsNaN() || p1.IsNaN() || p2.IsNaN())
            //    return;

            var yStart = (int)Math.Max(p0.Y, 0);
            var yEnd = (int)Math.Min(p2.Y, height - 1);

            // Out if clipped
            if(yStart > yEnd)
                return result;

            var yMiddle = MathUtils.Clamp((int)p1.Y, yStart, yEnd);

            // This has to move elsewhere
            var lightPos = new Vector3(0, 10, 10);

            // computing the cos of the angle between the light vector and the normal vector
            // it will return a value between 0 and 1 that will be used as the intensity of the color

            var nl0 = MathUtils.ComputeNDotL(
                vertexBuffer.WorldVertices[i0],
                vertexBuffer.WorldVertexNormals[i0],
                lightPos);
            var nl1 = MathUtils.ComputeNDotL(
                vertexBuffer.WorldVertices[i1],
                vertexBuffer.WorldVertexNormals[i1],
                lightPos);
            var nl2 = MathUtils.ComputeNDotL(
                vertexBuffer.WorldVertices[i2],
                vertexBuffer.WorldVertexNormals[i2],
                lightPos);
            if(PainterUtils.HaveClockwiseOrientation(p0, p1, p2))
            {
                // P0
                //   P1
                // P2
                var firstHalf = ScanLineHalfTriangle(width,
                    yStart, (int)yMiddle - 1, p0, p2, p0, p1, nl0, nl2, nl0, nl1);
                var secondHalf = ScanLineHalfTriangle(width,
                    (int)yMiddle, yEnd, p0, p2, p1, p2, nl0, nl2, nl1, nl2);
                result.AddRange(firstHalf);
                result.AddRange(secondHalf);
            }
            else
            {
                //   P0
                // P1 
                //   P2
                var firstHalf = ScanLineHalfTriangle(width, yStart, (int)yMiddle - 1,
                    p0, p1, p0, p2, nl0, nl1, nl0, nl2);
                var secondHalf = ScanLineHalfTriangle(width, (int)yMiddle,
                    yEnd, p1, p2, p0, p2, nl1, nl2, nl0, nl2);
                result.AddRange(firstHalf);
                result.AddRange(secondHalf);
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        //private static List<Vector4> ScanLineHalfTriangle(
        //    float frameWidth, int yStart, int yEnd, Vector3 pa, Vector3 pb,
        //    Vector3 pc, Vector3 pd, float nla, float nlb, float nlc, float nld)
        //{
        //    var result = new List<Vector4>();
        //    var deltaY1 = Math.Abs(pa.Y - pb.Y) < float.Epsilon ? 1f : 1 / (pb.Y - pa.Y);
        //    var deltaY2 = Math.Abs(pd.Y - pc.Y) < float.Epsilon ? 1f : 1 / (pd.Y - pc.Y);

        //    var pla = new Vector4(pa, nla);
        //    var plb = new Vector4(pb, nlb);
        //    var plc = new Vector4(pc, nlc);
        //    var pld = new Vector4(pb, nlb);
        //    for(var y = yStart; y <= yEnd; y++)
        //    {
        //        var gradient1 = ((y - pla.Y) * deltaY1).Clamp();
        //        var gradient2 = ((y - plc.Y) * deltaY2).Clamp();

        //        var start = Vector4.Lerp(pla, plb, gradient1);
        //        start.Y = y;
        //        var end = Vector4.Lerp(plc, pld, gradient2);
        //        end.Y = y;

        //        if(start.X >= end.X)
        //            continue;

        //        var line = ScanSingleLine(frameWidth, start, end);
        //        result.AddRange(line);
        //    }
        //    return result;
        //}

        private static List<Vector4> ScanLineHalfTriangle(
            float frameWidth, int yStart, int yEnd, Vector3 pa, Vector3 pb,
            Vector3 pc, Vector3 pd, float nla, float nlb, float nlc, float nld)
        {
            var result = new List<Vector4>();
            var mg1 = Math.Abs(pa.Y - pb.Y) < float.Epsilon ? 1f : 1 / (pb.Y - pa.Y);
            var mg2 = Math.Abs(pd.Y - pc.Y) < float.Epsilon ? 1f : 1 / (pd.Y - pc.Y);


            for(var y = yStart; y <= yEnd; y++)
            {
                var gradient1 = ((y - pa.Y) * mg1).Clamp();
                var gradient2 = ((y - pc.Y) * mg2).Clamp();

                var sx = MathUtils.Lerp(pa.X, pb.X, gradient1);
                var ex = MathUtils.Lerp(pc.X, pd.X, gradient2);

                if(sx >= ex)
                    continue;

                var sl = MathUtils.Lerp(nla, nlb, gradient1);
                var el = MathUtils.Lerp(nlc, nld, gradient2);

                var sz = MathUtils.Lerp(pa.Z, pb.Z, gradient1);
                var ez = MathUtils.Lerp(pc.Z, pd.Z, gradient2);
                var start = new Vector4(sx, y, sz, sl);
                var end = new Vector4(ex, y, ez, el);
                var line = ScanSingleLine(frameWidth, start, end);
                result.AddRange(line);
            }
            return result;
        }

        /// <summary>
        /// Scan line on the x direction
        /// </summary>
        /// <param name="frameWidth"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static List<Vector4> ScanSingleLine(float frameWidth, Vector4 start, Vector4 end)
        {
            var result = new List<Vector4>();
            var minX = Math.Max(start.X, 0);
            var maxX = Math.Min(end.X, frameWidth);

            var deltaX = 1 / (end.X - start.X);

            for(var x = minX; x < maxX; x++)
            {
                var gradient = (x - start.X) * deltaX;
                var lerp = Vector4.Lerp(start, end, gradient);
                lerp.X = x;
                result.Add(lerp);
            }

            return result;
        }
    }
}
