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

            var pl0 = new Vector4(p0, nl0);
            var pl1 = new Vector4(p1, nl1);
            var pl2 = new Vector4(p2, nl2);
            if(PainterUtils.HaveClockwiseOrientation(p0, p1, p2))
            {
                // P0
                //   P1
                // P2
                var fh = ScanLineHalfTriangleBottomFlat(width, yStart, (int)yMiddle - 1, pl0, pl1, pl2);
                var sh = ScanLineHalfTriangleTopFlat(width, (int)yMiddle, yEnd, pl2, pl1, pl0);
                result.AddRange(fh);
                result.AddRange(sh);
            }
            else
            {
                //   P0
                // P1 
                //   P2

                var fh = ScanLineHalfTriangleBottomFlat(width, yStart, (int)yMiddle - 1, pl0, pl2, pl1);
                var sh = ScanLineHalfTriangleTopFlat(width, (int)yMiddle, yEnd, pl2, pl0, pl1);
                result.AddRange(fh);
                result.AddRange(sh);
            }

            return result;
        }

        //            P0
        //          .....
        //       ..........
        //   .................P1
        // P2
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static List<Vector4> ScanLineHalfTriangleBottomFlat(float frameWidth, int yStart, int yEnd,
            Vector4 anchor, Vector4 vRight, Vector4 vLeft)
        {
            var result = new List<Vector4>();

            var deltaY1 = Math.Abs(vLeft.Y - anchor.Y) < float.Epsilon ? 1f : 1 / (vLeft.Y - anchor.Y);
            var deltaY2 = Math.Abs(vRight.Y - anchor.Y) < float.Epsilon ? 1f : 1 / (vRight.Y - anchor.Y);

            for(var y = yStart; y <= yEnd; y++)
            {
                var gradient1 = ((y - anchor.Y) * deltaY1).Clamp();
                var gradient2 = ((vRight.Y - y) * deltaY2).Clamp();

                var start = Vector4.Lerp(anchor, vLeft, gradient1);
                var end = Vector4.Lerp(vRight, anchor, gradient2);

                if(start.X >= end.X)
                    continue;

                start.Y = y;
                end.Y = y;

                var line = ScanSingleLine(frameWidth, start, end);
                result.AddRange(line);
            }

            return result;
        }

        // P2
        //   .................P1
        //       ..........
        //          .....
        //            P0
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static List<Vector4> ScanLineHalfTriangleTopFlat(float frameWidth, int yStart, int yEnd,
            Vector4 anchor, Vector4 vRight, Vector4 vLeft)
        {
            var result = new List<Vector4>();

            var deltaY1 = Math.Abs(vLeft.Y - anchor.Y) < float.Epsilon ? 1f : 1 / (vLeft.Y - anchor.Y);
            var deltaY2 = Math.Abs(vRight.Y - anchor.Y) < float.Epsilon ? 1f : 1 / (vRight.Y - anchor.Y);

            for(var y = yStart; y <= yEnd; y++)
            {
                var gradient1 = ((vLeft.Y - y) * deltaY1).Clamp();
                var gradient2 = ((vRight.Y - y) * deltaY2).Clamp();

                var start = Vector4.Lerp(vLeft, anchor, gradient1);
                var end = Vector4.Lerp(vRight, anchor, gradient2);

                if(start.X >= end.X)
                    continue;

                start.Y = y;
                end.Y = y;

                var line = ScanSingleLine(frameWidth, start, end);
                result.AddRange(line);
            }

            return result;
        }

        /// <summary>
        /// Scan line on the x direction
        /// </summary>
        /// <param name="frameWidth"></param>
        /// <param name="start">Scan line start</param>
        /// <param name="end">Scan line end</param>
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
                var point = Vector4.Lerp(start, end, gradient);
                point.X = x;
                result.Add(point);
            }

            return result;
        }
    }
}
