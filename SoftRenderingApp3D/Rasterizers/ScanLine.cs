using SoftRenderingApp3D.Buffer;
using SoftRenderingApp3D.Utils;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace SoftRenderingApp3D.Rasterizers
{
    public static class ScanLine
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<Vector3> ScanLineTriangle(VertexBuffer vertexBuffer, FrameBuffer frameBuffer, int height, int width, int i0, int i1, int i2)
        {
            var result = new List<Vector3>();

            var p0 = vertexBuffer.ScreenPointVertices[i0];
            var p1 = vertexBuffer.ScreenPointVertices[i1];
            var p2 = vertexBuffer.ScreenPointVertices[i2];

            if(p0.Y > p1.Y || p0.Y > p2.Y || p1.Y > p2.Y)
                throw new ArgumentException("The point must be sorted according to the Y coordinate!");

            if(p0.IsNaN() || p1.IsNaN() || p2.IsNaN())
                return result;

            var yStart = (int)Math.Max(p0.Y, 0);
            var yEnd = (int)Math.Min(p2.Y, height - 1);

            // Out if clipped
            if(yStart > yEnd)
                return result;

            var yMiddle = MathUtils.Clamp((int)p1.Y, yStart, yEnd);

            if(PainterUtils.HaveClockwiseOrientation(p0, p1, p2))
            {
                // P0
                //   P1
                // P2
                var fh = ScanLineHalfTriangleBottomFlat(frameBuffer, width, yStart, (int)yMiddle - 1, p0, p1, p2);
                var sh = ScanLineHalfTriangleTopFlat(frameBuffer, width, (int)yMiddle, yEnd, p2, p1, p0);
                result.AddRange(fh);
                result.AddRange(sh);
            }
            else
            {
                //   P0
                // P1 
                //   P2

                var fh = ScanLineHalfTriangleBottomFlat(frameBuffer, width, yStart, (int)yMiddle - 1, p0, p2, p1);
                var sh = ScanLineHalfTriangleTopFlat(frameBuffer, width, (int)yMiddle, yEnd, p2, p0, p1);
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
        private static List<Vector3> ScanLineHalfTriangleBottomFlat(FrameBuffer frameBuffer, float frameWidth, int yStart, int yEnd,
            Vector3 anchor, Vector3 vRight, Vector3 vLeft)
        {
            var result = new List<Vector3>();

            var deltaY1 = Math.Abs(vLeft.Y - anchor.Y) < float.Epsilon ? 1f : 1 / (vLeft.Y - anchor.Y);
            var deltaY2 = Math.Abs(vRight.Y - anchor.Y) < float.Epsilon ? 1f : 1 / (vRight.Y - anchor.Y);

            for(var y = yStart; y <= yEnd; y++)
            {
                var gradient1 = ((y - anchor.Y) * deltaY1).Clamp();
                var gradient2 = ((vRight.Y - y) * deltaY2).Clamp();

                var start = Vector3.Lerp(anchor, vLeft, gradient1);
                var end = Vector3.Lerp(vRight, anchor, gradient2);

                if(start.X >= end.X)
                    continue;

                start.Y = y;
                end.Y = y;

                var line = ScanSingleLine(frameBuffer, frameWidth, start, end);
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
        private static List<Vector3> ScanLineHalfTriangleTopFlat(FrameBuffer frameBuffer, float frameWidth, int yStart, int yEnd,
            Vector3 anchor, Vector3 vRight, Vector3 vLeft)
        {
            var result = new List<Vector3>();

            var deltaY1 = Math.Abs(vLeft.Y - anchor.Y) < float.Epsilon ? 1f : 1 / (vLeft.Y - anchor.Y);
            var deltaY2 = Math.Abs(vRight.Y - anchor.Y) < float.Epsilon ? 1f : 1 / (vRight.Y - anchor.Y);

            for(var y = yStart; y <= yEnd; y++)
            {
                var gradient1 = ((vLeft.Y - y) * deltaY1).Clamp();
                var gradient2 = ((vRight.Y - y) * deltaY2).Clamp();

                var start = Vector3.Lerp(vLeft, anchor, gradient1);
                var end = Vector3.Lerp(vRight, anchor, gradient2);

                if(start.X >= end.X)
                    continue;

                start.Y = y;
                end.Y = y;

                var line = ScanSingleLine(frameBuffer, frameWidth, start, end);
                result.AddRange(line);
            }

            return result;
        }

        /// <summary>
        /// Scan line on the x direction
        /// </summary>
        /// <param name="frameBuffer">Frame buffer</param>
        /// <param name="frameWidth"></param>
        /// <param name="start">Scan line start</param>
        /// <param name="end">Scan line end</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static List<Vector3> ScanSingleLine(FrameBuffer frameBuffer, float frameWidth, Vector3 start, Vector3 end)
        {
            var result = new List<Vector3>();
            var minX = Math.Max(start.X, 0);
            var maxX = Math.Min(end.X, frameWidth);

            var deltaX = 1 / (end.X - start.X);

            for(var x = minX; x < maxX; x++)
            {
                var gradient = (x - start.X) * deltaX;
                var point = Vector3.Lerp(start, end, gradient);
                point.X = x;
                if(frameBuffer.TryUpdateZBuffer((int)point.X, (int)point.Y, point.Z))
                    result.Add(point);
            }

            return result;
        }
    }
}
