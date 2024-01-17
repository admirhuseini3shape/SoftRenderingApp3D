using SoftRenderingApp3D.Buffer;
using SoftRenderingApp3D.Utils;
using System;
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
        public void ScanLineTriangle(Facet facet, int faId)
        {
            var (i0, i1, i2) = PainterUtils.SortIndices(vertexBuffer.ScreenPointVertices, facet.I0, facet.I1, facet.I2);
            if(i0 == i1 || i1 == i2 || i2 == i0)
                return;

            var p0 = vertexBuffer.ScreenPointVertices[i0];
            var p1 = vertexBuffer.ScreenPointVertices[i1];
            var p2 = vertexBuffer.ScreenPointVertices[i2];

            if(p0.IsNaN() || p1.IsNaN() || p2.IsNaN())
                return;

            var yStart = (int)Math.Max(p0.Y, 0);
            var yEnd = (int)Math.Min(p2.Y, height - 1);

            // Out if clipped
            if(yStart > yEnd)
                return;

            var yMiddle = MathUtils.Clamp((int)p1.Y, yStart, yEnd);

            if(PainterUtils.HaveClockwiseOrientation(p0, p1, p2))
            {
                // P0
                //   P1
                // P2
                ScanLineHalfTriangleBottomFlat(yStart, (int)yMiddle - 1, p0, p1, p2, faId);
                ScanLineHalfTriangleTopFlat((int)yMiddle, yEnd, p2, p1, p0, faId);
            }
            else
            {
                //   P0
                // P1 
                //   P2

                ScanLineHalfTriangleBottomFlat(yStart, (int)yMiddle - 1, p0, p2, p1, faId);
                ScanLineHalfTriangleTopFlat((int)yMiddle, yEnd, p2, p0, p1, faId);
            }
        }

        //            P0
        //          .....
        //       ..........
        //   .................P1
        // P2
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ScanLineHalfTriangleBottomFlat(int yStart, int yEnd,
            Vector3 anchor, Vector3 vRight, Vector3 vLeft, int faId)
        {
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

                ScanSingleLine(start, end, faId);
            }
        }

        // P2
        //   .................P1
        //       ..........
        //          .....
        //            P0
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ScanLineHalfTriangleTopFlat(int yStart, int yEnd,
            Vector3 anchor, Vector3 vRight, Vector3 vLeft, int faId)
        {
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

                ScanSingleLine(start, end, faId);
            }
        }

        /// <summary>
        /// Scan line on the x direction
        /// </summary>
        /// <param name="start">Scan line start</param>
        /// <param name="end">Scan line end</param>
        /// <param name="faId">Facet id</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ScanSingleLine(Vector3 start, Vector3 end, int faId)
        {
            var minX = Math.Max(start.X, 0);
            var maxX = Math.Min(end.X, width);

            var deltaX = 1 / (end.X - start.X);

            for(var x = minX; x < maxX; x++)
            {
                var gradient = (x - start.X) * deltaX;
                var point = Vector3.Lerp(start, end, gradient);
                var xInt = (int)x;
                var yInt = (int)point.Y;
                if(frameBuffer.TryUpdateZBuffer(xInt, yInt, point.Z))
                    frameBuffer.SetFacetIdForPixel(xInt, yInt, faId);
            }
        }
    }
}
