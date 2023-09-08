using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Drawing;

namespace SoftRenderingApp3D {

    public class FrameBuffer {
        private readonly int[] emptyZBuffer;
        private readonly int[] emptyBuffer;
        
        private readonly RenderContext renderContext;
        private int[] zBuffer;
        private int[] zScatterBuffer;

        public int[] Screen { get; }
        public int[] ScatterScreen { get; }

        internal int Width { get; }
        internal int Height { get; }
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
            this.ScatterScreen = new int[width * height];

            this.zBuffer = new int[width * height];
            this.zScatterBuffer = new int[width * height];


            this.emptyBuffer = new int[width * height];
            this.emptyZBuffer = new int[width * height];
            this.emptyZBuffer.Fill(Depth);

            this.Width = width;
            this.Height = height;
            this.renderContext = renderContext;
            this.widthMinus1By2 = (width - 1) / 2f;
            this.heightMinus1By2 = (height - 1) / 2f;
        }

        public void Clear() {
            Array.Copy(emptyBuffer, Screen, Screen.Length);
            Array.Copy(emptyBuffer, ScatterScreen, ScatterScreen.Length);

            Array.Copy(emptyZBuffer, zBuffer, zBuffer.Length);
            Array.Copy(emptyZBuffer, zScatterBuffer, zScatterBuffer.Length);

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
                renderContext.Stats.BehindZPixelCount++;
                return;
            }

            renderContext.Stats.DrawnPixelCount++;

            zBuffer[index] = z;


            Screen[index] = color.Color;
        }

        // Called to add the subsurface scattering effect at a specific X,Y coordinate
        [MethodImpl(MethodImplOptions.AggressiveInlining)]

        public void ScatterPixel(int x, int y, int z, ColorRGB color) {
#if DEBUG
            if(x > Width - 1 || x < 0 || y > Height - 1 || y < 0) {
                throw new OverflowException($"PutPixel X={x}/{Width}: Y={y}/{Height}, Depth={z}");
            }

            var index = x + y * Width;

            if(z >= zScatterBuffer[index]) {
                renderContext.Stats.BehindZPixelCount++;
                return;
            }

            renderContext.Stats.DrawnPixelCount++;

            zScatterBuffer[index] = z;

            //var previous_color = Color.FromArgb(Screen[index]);

            //var previousRGB = new ColorRGB(previous_color.R, previous_color.G, previous_color.B);

            //var new_color = new ColorRGB((byte)(previousRGB.R + (color.R * 0.2)), (byte)(previousRGB.G + (color.G * 0.2)), (byte)(previousRGB.B + (color.B * 0.2)));

            //float decay = 0;

            //if (Math.Abs(zBuffer[index] - zScatterBuffer[index]) > 10)
            //    decay = 10.0f / Math.Abs(zBuffer[index] - zScatterBuffer[index]);

            //ScatterScreen[index] = new ColorRGB((byte)(color.R * decay), (byte)(color.G * decay), (byte)(color.B * decay)).Color;
            ScatterScreen[index] = color.Color;
#endif
        }

        public void CombineScreens() {
            for (int i  = 0; i < Screen.Length; i++) {
                Screen[i] += ScatterScreen[i]; 
            }
        }


        public float ExponentialDecay(int z) {
            return 10000000 * (float)Math.Exp(-z);
        }



        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawLine(Vector3 p0, Vector3 p1, ColorRGB color) {

            var x0 = (int)p0.X; var y0 = (int)p0.Y; var z0 = (int)p0.Z;
            var x1 = (int)p1.X; var y1 = (int)p1.Y; var z1 = (int)p1.Z;

            var dx = Math.Abs(x1 - x0); var dy = Math.Abs(y1 - y0); var dz = Math.Abs(z1 - z0);

            var sx = x0 < x1 ? 1 : -1; var sy = y0 < y1 ? 1 : -1; var sz = z0 < z1 ? 1 : -1;

            var ex = 0; var ey = 0; var ez = 0;

            var dmax = Math.Max(dx, dy);

            int i = 0;
            while(i++ < dmax) {
                ex += dx; if(ex >= dmax) { ex -= dmax; x0 += sx; PutPixel(x0, y0, z0, color); }
                ey += dy; if(ey >= dmax) { ey -= dmax; y0 += sy; PutPixel(x0, y0, z0, color); }
                ez += dz; if(ez >= dmax) { ez -= dmax; z0 += sz; PutPixel(x0, y0, z0, color); }
            }
        }
    }
}