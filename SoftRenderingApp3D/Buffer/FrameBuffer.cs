using SoftRenderingApp3D.DataStructures;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace SoftRenderingApp3D.Buffer
{
    public class FrameBuffer : IDisposable
    {
        private readonly ArrayPool<int> intPool = ArrayPool<int>.Shared;
        private readonly ArrayPool<float> floatPool = ArrayPool<float>.Shared;

        private readonly object syncRoot = new object();
        private readonly float[] zBuffer;
        private readonly int[] facetIdForPixel;
        private readonly Stats stats;
        private const int NoFacet = -1;

        public FrameBuffer(int width, int height)
        {
            Screen = intPool.Rent(width * height);
            facetIdForPixel = intPool.Rent(width * height);
            zBuffer = floatPool.Rent(width * height);

            stats = StatsSingleton.Instance;

            Width = width;
            Height = height;
        }

        public int[] Screen { get; }
        public IReadOnlyList<int> FacetIdForPixel => facetIdForPixel;
        public int Width { get; }
        public int Height { get; }
        private int Depth { get; set; } = 65535; // Build a true Z buffer based on Zfar/Znear planes

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3 ToScreen3(Vector4 p)
        {
            return new Vector3(
                ((Width - 1) / 2f) * (p.X / p.W + 1), // Using width - 1 to prevent overflow by -1 and 1 NDC coordinates
                -((Height - 1) / 2f) * (p.Y / p.W - 1), // Using height - 1 to prevent overflow by -1 and 1 NDC coordinates
                Depth * p.Z / p.W);
        }

        public void Clear()
        {

            Span<int> screenSpan = Screen;
            Span<int> facetIdForPixelSpan = facetIdForPixel;
            Span<float> zBufferSpan = zBuffer.AsSpan();

            screenSpan.Fill(0);
            facetIdForPixelSpan.Fill(NoFacet);
            zBufferSpan.Fill(Depth);
        }

        private void ReleaseBuffers()
        {
            intPool.Return(Screen);
            floatPool.Return(zBuffer);
        }

        // Called to put a pixel on screen at a specific X,Y coordinates
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void PutPixel(int x, int y, float z, int color, int faId = NoFacet)
        {
#if DEBUG
            if(x > Width - 1 || x < 0 || y > Height - 1 || y < 0)
            {
                throw new OverflowException($"PutPixel X={x}/{Width}: Y={y}/{Height}, Depth={z}");
            }
#endif
            var index = x + y * Width;
            if(z > zBuffer[index])
            {
                stats.BehindZPixelCount++;
                return;
            }

            stats.DrawnPixelCount++;

            zBuffer[index] = z;
            Screen[index] = color;
            facetIdForPixel[index] = faId;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetFacetIdForPixel(int x, int y)
        {
#if DEBUG
            if(x > Width - 1 || x < 0 || y > Height - 1 || y < 0)
            {
                throw new OverflowException($"PutPixel X={x}/{Width}: Y={y}/{Height}");
            }
#endif
            var index = x + y * Width;
            return facetIdForPixel[index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PutPixels(IReadOnlyList<FacetPixelData> perPixelColors)
        {
            lock(syncRoot)
            {
                for(var i = 0; i < perPixelColors.Count; i++)
                {
                    var p = perPixelColors[i];
                    PutPixel(p.xScreen, p.yScreen, p.zDepth, p.ColorAsInt, p.FacetId);
                }
            }
        }

        // Called to put a pixel on screen at a specific X,Y coordinates
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryUpdateZBuffer(int x, int y, float z)
        {
#if DEBUG
            if(x > Width - 1 || x < 0 || y > Height - 1 || y < 0)
            {
                throw new OverflowException($"PutPixel X={x}/{Width}: Y={y}/{Height}, Depth={z}");
            }
#endif
            var index = x + y * Width;
            if(z >= zBuffer[index])
            {
                stats.BehindZPixelCount++;
                return false;
            }

            zBuffer[index] = z;

            return true;

        }


        // Bresenham's line algorithm .
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawLine3d(Vector3 p0, Vector3 p1, ColorRGB color)
        {
            // Converts the start and end points from floating point to integer coordinates.
            var x1 = (int)p0.X;
            var y1 = (int)p0.Y;
            var z1 = (int)p0.Z;
            var x2 = (int)p1.X;
            var y2 = (int)p1.Y;
            var z2 = (int)p1.Z;

            // Calculate the absolute differences in each dimension.
            var dx = Math.Abs(x2 - x1);
            var sx = x1 < x2 ? 1 : -1;
            var dy = Math.Abs(y2 - y1);
            var sy = y1 < y2 ? 1 : -1;
            var dz = Math.Abs(z2 - z1);
            var sz = z1 < z2 ? 1 : -1;

            // Calculate the major axis.
            var dm = Math.Max(dx, Math.Max(dy, dz));

            // Initialize the decision variables
            var i = dm;

            // Set up the decision variables.
            x1 = y1 = z1 = dm / 2;

            var colorAsInt = color.Color;
            // Start the infinite drawing loop.
            while(true)
            {
                // Draw Current Pixel
                PutPixel(x2, y2, z2, colorAsInt);

                // Break the loop if the end point is reached.
                if(i-- == 0)
                {
                    break;
                }

                // Update the decision variables and coordinates based on the absolute differences.
                x2 += dx;
                if(x2 < 0) { x2 += dm; x1 += sx; }

                y2 += dy;
                if(y2 < 0) { y2 += dm; y1 += sy; }

                z2 += dz;
                if(z2 < 0) { z2 += dm; z1 += sz; }
            }
        }

        public void Dispose()
        {
            ReleaseBuffers();
        }
    }
}
