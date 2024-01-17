using SoftRenderingApp3D.Buffer;
using SoftRenderingApp3D.Utils;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace SoftRenderingApp3D.Rasterizers
{
    public class ScanLine
    {
        private readonly VertexBuffer vertexBuffer;
        private readonly FrameBuffer frameBuffer;
        private readonly int width;
        private readonly int height;

        public ScanLine(VertexBuffer vertexBuffer, FrameBuffer frameBuffer)
        {
            this.vertexBuffer = vertexBuffer;
            this.frameBuffer = frameBuffer;
            width = this.frameBuffer.Width;
            height = this.frameBuffer.Height;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<Vector3> ScanLineTriangle(Facet facet)
        {
            var result = new List<Vector3>();

            var (i0, i1, i2) = PainterUtils.SortIndices(vertexBuffer.ScreenPointVertices, facet.I0, facet.I1, facet.I2);
            if(i0 == i1 || i1 == i2 || i2 == i0)
                return result;

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
                var fh = ScanLineHalfTriangleBottomFlat(yStart, (int)yMiddle - 1, p0, p1, p2);
                var sh = ScanLineHalfTriangleTopFlat((int)yMiddle, yEnd, p2, p1, p0);
                result.AddRange(fh);
                result.AddRange(sh);
            }
            else
            {
                //   P0
                // P1 
                //   P2

                var fh = ScanLineHalfTriangleBottomFlat(yStart, (int)yMiddle - 1, p0, p2, p1);
                var sh = ScanLineHalfTriangleTopFlat((int)yMiddle, yEnd, p2, p0, p1);
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
        private List<Vector3> ScanLineHalfTriangleBottomFlat(int yStart, int yEnd, 
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

                var line = ScanSingleLine(start, end);
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
        private List<Vector3> ScanLineHalfTriangleTopFlat(int yStart, int yEnd,
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

                var line = ScanSingleLine(start, end);
                result.AddRange(line);
            }

            return result;
        }

        /// <summary>
        /// Scan line on the x direction
        /// </summary>
        /// <param name="start">Scan line start</param>
        /// <param name="end">Scan line end</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private List<Vector3> ScanSingleLine(Vector3 start, Vector3 end)
        {
            var result = new List<Vector3>();
            var minX = Math.Max(start.X, 0);
            var maxX = Math.Min(end.X, width);

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
