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
        private int[] zSubsurfaceBuffer;
        private Vector3[] WorldBuffer;
        private Vector3[] SubsurfaceWorldBuffer;

        public int[] Screen { get; }
        public int[] TempScreen { get; }
        public int[] SubsurfaceScreen { get; }

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
            this.TempScreen = new int[width * height];

            this.SubsurfaceScreen = new int[width * height];


            this.zBuffer = new int[width * height];
            this.zSubsurfaceBuffer = new int[width * height];
            this.WorldBuffer = new Vector3[width * height];
            this.SubsurfaceWorldBuffer = new Vector3[width * height];

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
            Array.Copy(emptyBuffer, TempScreen, TempScreen.Length);
            Array.Copy(emptyBuffer, SubsurfaceScreen, SubsurfaceScreen.Length);

            Array.Copy(emptyZBuffer, zBuffer, zBuffer.Length);
            Array.Copy(emptyZBuffer, zSubsurfaceBuffer, zSubsurfaceBuffer.Length);
            Array.Copy(emptyWorldBuffer, WorldBuffer, WorldBuffer.Length);
            Array.Copy(emptyWorldBuffer, SubsurfaceWorldBuffer, SubsurfaceWorldBuffer.Length);

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
            if(z >= zBuffer[index]) {
                renderContext.Stats.BehindZPixelCount++;
                return;
            }

            renderContext.Stats.DrawnPixelCount++;

            zBuffer[index] = z;

            TempScreen[index] = color.Color;
            Screen[index] = color.Color;
        }

        // Called to add the subsurface scattering effect at a specific X,Y coordinate
        [MethodImpl(MethodImplOptions.AggressiveInlining)]

        public void PutSubsurfacePixel(int x, int y, int z, ColorRGB color, Vector3 world) {
#if DEBUG
            if(x > Width - 1 || x < 0 || y > Height - 1 || y < 0) {
                throw new OverflowException($"PutPixel X={x}/{Width}: Y={y}/{Height}, Depth={z}");
            }

            var index = x + y * Width;

            if(z >= zSubsurfaceBuffer[index]) {
                renderContext.Stats.BehindZPixelCount++;
                return;
            }

            renderContext.Stats.DrawnPixelCount++;

            zSubsurfaceBuffer[index] = z;
            SubsurfaceWorldBuffer[index] = world;

            SubsurfaceScreen[index] = color.Color;
#endif
        }

        public void ApplyGaussianBlurToSubsurface() {
            for ( int i = 0; i < Height; i++) {
                for (int j = 0; j < Width; j++) {
                    var index = j + i * Width;
                    // check if subsurface visible at that pixel
                    if ( zSubsurfaceBuffer[index] != 0) {
                        var color = new ColorRGB(Color.FromArgb(SubsurfaceScreen[index]));
                        GaussianBlurAtPixel(j, i, color);                        
                    }
                }
            }
        }

        public void ApplyClearSubsurface() {
            for(int i = 0; i < Height; i++) {
                for(int j = 0; j < Width; j++) {
                    var index = j + i * Width;
                    // check if subsurface visible at that pixel
                    if(zSubsurfaceBuffer[index] != 0) {
                        var color = new ColorRGB(Color.FromArgb(SubsurfaceScreen[index]));
                        var newcolor = new ColorRGB(color.R, color.G, color.B, (byte)(RenderUtils.subsurfaceVisibility * 255));

                        ColorRGB priorPixelColor = new ColorRGB(Color.FromArgb(Screen[index]));
                        var newPriorPixelColor = new ColorRGB(priorPixelColor.R, priorPixelColor.G, priorPixelColor.B, (byte)(RenderUtils.surfaceOpacity * 255));
                        ColorRGB pixelColorWithSubsurface = ColorRGB.AlphaBlend(newPriorPixelColor, newcolor);
                        // Console.WriteLine($"subsurface color: {color}, surface color: {priorPixelColor}, final color: {pixelColorWithSubsurface}");
                        Screen[index] = pixelColorWithSubsurface.Color;
                    }
                }
            }
        }

        public void GaussianBlurAtPixel(int x, int y, ColorRGB color) {
            int radius = RenderUtils.BlurRadiusInPixels;
            ColorRGB result = ColorRGB.Black;
            for (int i = y - radius; i <= y + radius; i++) {
                for (int j = x - radius; j <= x + radius; j++) {
                    if(WithinHeight(i) && WithinWidth(j)) {
                        var index = j + i * Width;
                        float gaussianWeight = RenderUtils.Gaussian[j - x + radius, i - y + radius];
                        ColorRGB gaussianColor = gaussianWeight * new ColorRGB(Color.FromArgb(SubsurfaceScreen[index]));
                        result += gaussianColor;
                        
                    }
                }
            }
            var pixelIndex = x + y * Width;
            ColorRGB priorPixelColor = new ColorRGB(Color.FromArgb(Screen[pixelIndex]));
            var newPriorPixelColor = new ColorRGB(priorPixelColor.R, priorPixelColor.G, priorPixelColor.B, (byte)(RenderUtils.surfaceOpacity * 255));
            var newResult = new ColorRGB(result.R, result.G, result.B, (byte)(RenderUtils.surfaceOpacity * 255));
            var finalColor = ColorRGB.AlphaBlend(newPriorPixelColor, newResult);
            Screen[pixelIndex] = finalColor.Color;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]

        public bool WithinHeight(int x) {
            return x < Height && x >= 0;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]

        public bool WithinWidth(int x) {
            return x < Width && x >= 0;
        }
    }
}