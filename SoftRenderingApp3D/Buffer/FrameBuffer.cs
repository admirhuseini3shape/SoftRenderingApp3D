using SoftRenderingApp3D.Utils;
using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace SoftRenderingApp3D.Buffer {

    public class FrameBuffer {
        private readonly int[] emptyZBuffer;
        private readonly int[] emptyBuffer;

        private readonly RenderContext RenderContext;
        private int[] zBuffer;

        public int[] Screen { get; }

        public int Width { get; }
        public int Height { get; }
        internal int Depth { get; set; } = 65535; // Build a true Z buffer based on Zfar/Znear planes

        private float widthMinus1By2 { get; }
        private float heightMinus1By2 { get; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3 ToScreen3(Vector4 p) => new Vector3(
            widthMinus1By2 * (p.X / p.W + 1),  // Using width - 1 to prevent overflow by -1 and 1 NDC coordinates
            -heightMinus1By2 * (p.Y / p.W - 1), // Using height - 1 to prevent overflow by -1 and 1 NDC coordinates
            Depth * p.Z / p.W);

        public FrameBuffer(int width, int height, RenderContext renderContext) {
            this.Screen = new int[width * height];
            this.zBuffer = new int[width * height];

            this.emptyBuffer = new int[width * height];
            this.emptyZBuffer = new int[width * height];
            this.emptyZBuffer.Fill(Depth);

            this.Width = width;
            this.Height = height;
            this.RenderContext = renderContext;
            this.widthMinus1By2 = (width - 1) / 2f;
            this.heightMinus1By2 = (height - 1) / 2f;
        }

        public void Clear() {
            Array.Copy(emptyBuffer, Screen, Screen.Length);
            Array.Copy(emptyZBuffer, zBuffer, zBuffer.Length);
        }

        // Called to put a pixel on screen at a specific X,Y coordinates
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PutPixel(int x, int y, int z, ColorRGB color) {
        #if DEBUG
            if(x > Width - 1 || x < 0 || y > Height - 1 || y < 0) {
                throw new OverflowException($"PutPixel X={x}/{Width}: Y={y}/{Height}, Depth={z}");
            }
        #endif
            var index = x + y * Width;
            if(z > zBuffer[index]) {
                RenderContext.Stats.BehindZPixelCount++;
                return;
            }

            RenderContext.Stats.DrawnPixelCount++;

            zBuffer[index] = z;

            Screen[index] = color.Color;
        }
        
        
        // Bresenham's line algorithm .
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawLine3d(Vector3 p0, Vector3 p1, ColorRGB color) {
            
            // Converts the start and end points from floating point to integer coordinates.
            int x1 = (int)p0.X; var y1 = (int)p0.Y; var z1 = (int)p0.Z;
            int x2 = (int)p1.X; var y2 = (int)p1.Y; var z2 = (int)p1.Z;
            
            // Calculate the absolute differences in each dimension.
            int dx = Math.Abs(x2 - x1); int sx = x1 < x2 ? 1 : -1;
            int dy = Math.Abs(y2 - y1); int sy = y1 < y2 ? 1 : -1;
            int dz = Math.Abs(z2 - z1); int sz = z1 < z2 ? 1 : -1;
            
            // Calculate the major axis.
            int dm = Math.Max(dx, Math.Max(dy, dz));

            // Initialize the decision variables
            int i = dm;
            
            // Set up the decision variables.
            x1 = y1 = z1 = dm / 2;
            
            // Start the infinite drawing loop.
            while(true) {
                // Draw Current Pixel
                PutPixel(x2, y2, z2, color);
                
                // Break the loop if the end point is reached.
                if (i-- == 0) break;
                
                // Update the decision variables and coordinates based on the absolute differences.
                x2 += dx; if(x2 < 0) { x2 += dm; x1 += sx; }
                y2 += dy; if(y2 < 0) { y2 += dm; y1 += sy; }
                z2 += dz; if(z2 < 0) { z2 += dm; z1 += sz; }
            }
        }
    }
}