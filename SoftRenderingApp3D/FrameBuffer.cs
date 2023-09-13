using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Drawing;

namespace SoftRenderingApp3D {

    public class FrameBuffer {
        private readonly int[] emptyZBuffer;
        private readonly int[] emptyBuffer;
        private readonly Vector3[] emptyWorldBuffer;
        
        private readonly RenderContext renderContext;
        private int[] zBuffer;
        private int[] zScatterBuffer;
        private Vector3[] WorldBuffer;
        private Vector3[] WorldScatterBuffer;

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
            this.emptyWorldBuffer = new Vector3[width * height];
            this.Screen = new int[width * height];
            this.ScatterScreen = new int[width * height];

            this.zBuffer = new int[width * height];
            this.zScatterBuffer = new int[width * height];
            this.WorldBuffer = new Vector3[width * height];
            this.WorldScatterBuffer = new Vector3[width * height];

            this.emptyBuffer = new int[width * height];
            this.emptyBuffer.Fill(ColorRGB.Black.Color);
            this.emptyZBuffer = new int[width * height];
            this.emptyZBuffer.Fill(Depth);
            this.emptyWorldBuffer.Fill(Vector3.Zero);


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
            Array.Copy(emptyWorldBuffer, WorldBuffer, WorldBuffer.Length);
            Array.Copy(emptyWorldBuffer, WorldScatterBuffer, WorldScatterBuffer.Length);

        }

        // Called to put a pixel on screen at a specific X,Y coordinates
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PutPixel(int x, int y, int z, ColorRGB color, Vector3 World) {
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
            WorldBuffer[index] = World;

            Screen[index] = color.Color;
        }

        // Called to add the subsurface scattering effect at a specific X,Y coordinate
        [MethodImpl(MethodImplOptions.AggressiveInlining)]

        public void ScatterPixel(int x, int y, int z, ColorRGB color, Vector3 World) {
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
            WorldScatterBuffer[index] = World;

            ScatterScreen[index] = color.Color;
#endif
        }
        // Combines the effect of the subsurface lighting and the surface lighting using a gaussian blur on the subsurface model.
        /*public void CombineScreens() {
            ColorRGB color;
            for (int i = 0; i < Height; i++) {
                for (int j = 0; j < Width; j++) {
                    var index = j + i * Width;

                    if(Screen[index] == ColorRGB.Black.Color)
                        continue;

                    float r = 0;
                    float g = 0;
                    float b = 0;

                    for(int x = i - 25; x < i + 25; x++) {
                        for (int y = j - 25; y < j + 25; y++) {
                            if(x < 0 ||
                                y < 0 ||
                                x >= Height ||
                                y >= Width ||
                                ScatterScreen[x * Width + y] == ColorRGB.Black.Color ||
                                (int)(r) + Color.FromArgb(Screen[index]).R >= 255 ||
                                (int)(b) + Color.FromArgb(Screen[index]).B >= 255 ||
                                (int)(g) + Color.FromArgb(Screen[index]).G >= 255)
                                continue;

                            var original_color = new ColorRGB(Color.FromArgb(ScatterScreen[x * Width + y]));

                            // Add light to the target pixel
                            r += original_color.R * CalculateGaussian(Math.Abs(x - i), Math.Abs(y - j));
                            g += original_color.G * CalculateGaussian(Math.Abs(x - i), Math.Abs(y - j));
                            b += original_color.B * CalculateGaussian(Math.Abs(x - i), Math.Abs(y - j));
                            //color += CalculateGaussian(Math.Abs(x - i), Math.Abs(y - j)) * original_color;
                            
                        }
                    }
                    color = new ColorRGB((byte)r, (byte)g, (byte)b);

                    // Add the subsurface light to the surface light
                    Screen[index] = (new ColorRGB(Color.FromArgb(Screen[index])) + color).Color;
                }
            }
        }*/

        public void CombineScreens() {
            for(int i = 0; i < Screen.Length; i++) {
                if(WorldScatterBuffer[i] == emptyWorldBuffer[i])
                    continue;
                float distance = Vector3.Distance(WorldScatterBuffer[i], WorldBuffer[i]);
                if( distance < 10) {
                    Screen[i] = (new ColorRGB(Color.FromArgb(Screen[i])) + (float)Math.Exp(-distance) * new ColorRGB(Color.FromArgb(ScatterScreen[i]))).Color;
                }
            }
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
                ex += dx; if(ex >= dmax) { ex -= dmax; x0 += sx; PutPixel(x0, y0, z0, color, Vector3.Zero); }
                ey += dy; if(ey >= dmax) { ey -= dmax; y0 += sy; PutPixel(x0, y0, z0, color, Vector3.Zero); }
                ez += dz; if(ez >= dmax) { ez -= dmax; z0 += sz; PutPixel(x0, y0, z0, color, Vector3.Zero); }
            }
        }

        // Calculates the weight for a pixel x pixels away from the original on the x-axis,
        // and y pixels away from the original on the y-axis
        public float CalculateGaussian(int x, int y) {
            var sigma = 4f;
            return (float)(1.0f / (2.0f * Math.PI * sigma) * Math.Exp(-(x * x + y * y) / (2 * sigma * sigma)));
        }
    }
}