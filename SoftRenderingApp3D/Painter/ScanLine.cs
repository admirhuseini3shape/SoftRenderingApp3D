using SoftRenderingApp3D.Buffer;
using SoftRenderingApp3D.Utils;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

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

            var pl0 = new Vector4(p0, nl0);
            var pl1 = new Vector4(p1, nl1);
            var pl2 = new Vector4(p2, nl2);
            if(PainterUtils.HaveClockwiseOrientation(p0, p1, p2))
            {
                // P0
                //   P1
                // P2

                // Anchor p0
                var pla = pl0;
                var plb = pl2;
                var plc = pl0;
                var pld = pl1;

                var firstHalf = ScanLineHalfTriangle(width, yStart, (int)yMiddle - 1, pla, plb, plc, pld);
                var fh = ScanLineCorner(width, yStart, (int)yMiddle - 1, pl0, pl2, pl1);
                // Anchor p2
                pla = pl0;
                plb = pl2;
                plc = pl1;
                pld = pl2;
                var secondHalf = ScanLineHalfTriangle(width, (int)yMiddle, yEnd, pla, plb, plc, pld);
                var sh = ScanLineCorner(width, yStart, (int)yMiddle - 1, pl2, pl1, pl0);
                result.AddRange(firstHalf);
                result.AddRange(secondHalf);
            }
            else
            {
                //   P0
                // P1 
                //   P2

                // Anchor p0
                var pla = pl0;
                var plb = pl1;
                var plc = pl0;
                var pld = pl2;
                var firstHalf = ScanLineHalfTriangle(width, yStart, (int)yMiddle - 1, pla, plb, plc, pld);
                var fh = ScanLineCorner(width, yStart, (int)yMiddle - 1, pl0, pl1, pl2);
                
                // Anchor p2
                pla = pl1;
                plb = pl2;
                plc = pl0;
                pld = pl2;
                var secondHalf = ScanLineHalfTriangle(width, (int)yMiddle, yEnd, pla, plb, plc, pld);
                var sh = ScanLineCorner(width, yStart, (int)yMiddle - 1, pl2, pl0, pl1);
                result.AddRange(firstHalf);
                result.AddRange(secondHalf);
            }

            return result;
        }

        private static List<Vector4> ScanLineCorner(float frameWidth, int yStart, int yEnd,
            Vector4 anchor, Vector4 end1, Vector4 end2)
        {
            var result = new List<Vector4>();

            var deltaY1 = Math.Abs(anchor.Y - end1.Y) < float.Epsilon ? 1f : 1 / (anchor.Y - end1.Y);
            var deltaY2 = Math.Abs(anchor.Y - end2.Y) < float.Epsilon ? 1f : 1 / (anchor.Y - end2.Y);

            for(var y = yStart; y <= yEnd; y++)
            {
                var gradient1 = ((y - end1.Y) * deltaY1).Clamp();
                var gradient2 = ((y - end2.Y) * deltaY2).Clamp();

                var start = Vector4.Lerp(anchor, end1, gradient1);
                start.Y = y;
                var end = Vector4.Lerp(anchor, end2, gradient2);
                end.Y = y;

                if(start.X >= end.X)
                    continue;

                var line = ScanSingleLine(frameWidth, start, end);
                result.AddRange(line);
            }

            return result;
        }

        private static List<Vector4> ScanLineHalfTriangle(float frameWidth, int yStart, int yEnd,
            Vector4 pla, Vector4 plb, Vector4 pld, Vector4 plc)
        {
            var result = new List<Vector4>();

            var deltaY1 = Math.Abs(pla.Y - plb.Y) < float.Epsilon ? 1f : 1 / (plb.Y - pla.Y);
            var deltaY2 = Math.Abs(pld.Y - plc.Y) < float.Epsilon ? 1f : 1 / (pld.Y - plc.Y);

            for(var y = yStart; y <= yEnd; y++)
            {
                var gradient1 = ((y - pla.Y) * deltaY1).Clamp();
                var gradient2 = ((y - plc.Y) * deltaY2).Clamp();

                var start = Vector4.Lerp(pla, plb, gradient1);
                start.Y = y;
                var end = Vector4.Lerp(plc, pld, gradient2);
                end.Y = y;

                if(start.X >= end.X)
                    continue;

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
