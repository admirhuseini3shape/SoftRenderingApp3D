using SoftRenderingApp3D;
using SoftRenderingApp3D.Utils;
using SubsurfaceScatteringLibrary.Renderer;
using SubsurfaceScatteringLibrary.Utils;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SubsurfaceScatteringLibrary.Buffer
{
    public class SubsurfaceScatteringFrameBuffer
    {
        private readonly SubsurfaceScatteringRenderContext _subsurfaceScatteringRenderContext;
        private readonly int[] emptyBuffer;
        private readonly Vector3[] emptyWorldBuffer;
        private readonly int[] emptyZBuffer;
        private readonly Vector3[] SubsurfaceWorldBuffer;
        private readonly Vector3[] WorldBuffer;
        private readonly float[] zBuffer;
        private readonly float[] zCariesBuffer;
        private readonly float[] zSubsurfaceBuffer;

        public SubsurfaceScatteringFrameBuffer(int width, int height,
            SubsurfaceScatteringRenderContext subsurfaceScatteringRenderContext)
        {
            emptyWorldBuffer = new Vector3[width * height];
            Screen = new int[width * height];
            TempScreen = new int[width * height];
            CariesScreen = new int[width * height];
            SubsurfaceScreen = new int[width * height];


            zBuffer = new float[width * height];
            zSubsurfaceBuffer = new float[width * height];
            zCariesBuffer = new float[width * height];
            WorldBuffer = new Vector3[width * height];
            SubsurfaceWorldBuffer = new Vector3[width * height];

            emptyBuffer = new int[width * height];
            emptyBuffer.Fill(new ColorRGB(0, 0, 0, 255).Color);
            emptyZBuffer = new int[width * height];
            emptyZBuffer.Fill(Depth);
            emptyWorldBuffer.Fill(Vector3.Zero);
            Width = width;
            Height = height;
            _subsurfaceScatteringRenderContext = subsurfaceScatteringRenderContext;
            widthMinus1By2 = (width - 1) / 2f;
            heightMinus1By2 = (height - 1) / 2f;
        }

        public int[] Screen { get; }
        public int[] TempScreen { get; }
        public int[] SubsurfaceScreen { get; }
        public int[] BlurScreen { get; set; }
        public int[] CariesScreen { get; }

        internal int Width { get; }
        internal int Height { get; }
        internal int Depth { get; set; } = 65535; // Build a true Z buffer based on Zfar/Znear planes

        private float widthMinus1By2 { get; }
        private float heightMinus1By2 { get; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3 ToScreen3(Vector4 p)
        {
            return new Vector3(
                widthMinus1By2 * (p.X / p.W + 1), // Using width - 1 to prevent overflow by -1 and 1 NDC coordinates
                -heightMinus1By2 * (p.Y / p.W - 1), // Using height - 1 to prevent overflow by -1 and 1 NDC coordinates
                Depth * p.Z / p.W);
        }

        public void Clear()
        {
            Array.Copy(emptyBuffer, Screen, Screen.Length);
            Array.Copy(emptyBuffer, TempScreen, TempScreen.Length);
            Array.Copy(emptyBuffer, SubsurfaceScreen, SubsurfaceScreen.Length);
            Array.Copy(emptyBuffer, CariesScreen, CariesScreen.Length);


            Array.Copy(emptyZBuffer, zBuffer, zBuffer.Length);
            Array.Copy(emptyZBuffer, zSubsurfaceBuffer, zSubsurfaceBuffer.Length);
            Array.Copy(emptyZBuffer, zCariesBuffer, zCariesBuffer.Length);
            Array.Copy(emptyWorldBuffer, WorldBuffer, WorldBuffer.Length);
            Array.Copy(emptyWorldBuffer, SubsurfaceWorldBuffer, SubsurfaceWorldBuffer.Length);
        }

        // Called to put a pixel on screen at a specific X,Y coordinates
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PutPixel(int x, int y, float z, ColorRGB color)
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
                _subsurfaceScatteringRenderContext.Stats.BehindZPixelCount++;
                return;
            }

            _subsurfaceScatteringRenderContext.Stats.DrawnPixelCount++;
            zBuffer[index] = z;
            color.Alpha = (byte)(RenderUtils.surfaceOpacity * 255);
            TempScreen[index] = color.Color;
            Screen[index] = color.Color;
        }

        // Called to add the subsurface scattering effect at a specific X,Y coordinate
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PutSubsurfacePixel(int x, int y, float z, ColorRGB color, Vector3 world)
        {
#if DEBUG
            if(x > Width - 1 || x < 0 || y > Height - 1 || y < 0)
            {
                throw new OverflowException($"PutPixel X={x}/{Width}: Y={y}/{Height}, Depth={z}");
            }

            var index = x + y * Width;
            if(z >= zSubsurfaceBuffer[index])
            {
                _subsurfaceScatteringRenderContext.Stats.BehindZPixelCount++;
                return;
            }

            _subsurfaceScatteringRenderContext.Stats.DrawnPixelCount++;
            zSubsurfaceBuffer[index] = z;
            SubsurfaceWorldBuffer[index] = world;

            color.Alpha = 255;

            zCariesBuffer[index] = z;

            SubsurfaceScreen[index] = color.Color;
#endif
        }

        public void ApplyGaussianBlurToSubsurface()
        {
            var bmp = new Bitmap(Width, Height, PixelFormat.Format32bppPArgb);
            ImageUtils.FillBitmap(bmp, SubsurfaceScreen, Width, Height);
            var gaussian = new GaussianBlur(bmp);

            var result = gaussian.Process(RenderUtils.BlurRadiusInPixels);
            BlurScreen = BitmapToIntArray(result);

            CombineAllScreens();
        }

        private void CombineAllScreens()
        {
            for(var i = 0; i < Height; i++)
            {
                for(var j = 0; j < Width; j++)
                {
                    var index = j + i * Width;
                    var subsurfacePixelColor = new ColorRGB(Color.FromArgb(SubsurfaceScreen[index]));
                    var surfacePixelColor = new ColorRGB(Color.FromArgb(Screen[index]));
                    var blurPixelColor = new ColorRGB(Color.FromArgb(BlurScreen[index]));
                    var combinedColor = surfacePixelColor + blurPixelColor;
                    if(SubsurfaceScreen[index] != emptyBuffer[0] && CariesScreen[index] != 1 &&
                       !SubsurfaceScatteringRenderUtils.OnlySubsurfaceBlur)
                    {
                        combinedColor = surfacePixelColor + subsurfacePixelColor;
                    }

                    Screen[index] = combinedColor.Color;
                }
            }
        }

        private static int[] BitmapToIntArray(Bitmap bitmap)
        {
            var width = bitmap.Width;
            var height = bitmap.Height;

            // Lock the bitmap's bits.
            var bmpData = bitmap.LockBits(
                new Rectangle(0, 0, width, height),
                ImageLockMode.ReadOnly,
                bitmap.PixelFormat);

            // Get the address of the first line.
            var ptr = bmpData.Scan0;

            // Declare an array to hold the bytes of the bitmap.
            var bytes = Math.Abs(bmpData.Stride) * height;
            var rgbValues = new byte[bytes];

            // Copy the RGB values into the array.
            Marshal.Copy(ptr, rgbValues, 0, bytes);

            // Unlock the bits.
            bitmap.UnlockBits(bmpData);

            // Convert byte array to int array (assuming RGBA format)
            var result = new int[width * height];

            for(int i = 0, j = 0; i < result.Length; i++, j += 4)
            {
                var pixelValue = (rgbValues[j + 3] << 24) | (rgbValues[j] << 16) | (rgbValues[j + 1] << 8) |
                                 rgbValues[j + 2];
                result[i] = pixelValue;
            }

            return result;
        }

        public void ApplyClearSubsurface()
        {
            for(var i = 0; i < Height; i++)
            {
                for(var j = 0; j < Width; j++)
                {
                    var index = j + i * Width;
                    // check if subsurface visible at that pixel
                    //if(zSubsurfaceBuffer[index] != Depth) {
                    var subsurfacePixelColor = new ColorRGB(Color.FromArgb(SubsurfaceScreen[index]));
                    var surfacePixelColor = new ColorRGB(Color.FromArgb(Screen[index]));
                    var pixelColorWithSubsurface = surfacePixelColor + subsurfacePixelColor;
                    // Console.WriteLine($"subsurface color: {color}, surface color: {priorPixelColor}, final color: {pixelColorWithSubsurface}");
                    Screen[index] = pixelColorWithSubsurface.Color;
                    //}
                    //}
                }
            }
        }

        public void PutCariesPixel(int x, int y, float z, ColorRGB color, Vector3 zWorld)
        {
            if(x > Width - 1 || x < 0 || y > Height - 1 || y < 0)
            {
                throw new OverflowException($"PutPixel X={x}/{Width}: Y={y}/{Height}, Depth={z}");
            }

            var index = x + y * Width;

            if(z >= zSubsurfaceBuffer[index])
            {
                _subsurfaceScatteringRenderContext.Stats.BehindZPixelCount++;
                return;
            }

            _subsurfaceScatteringRenderContext.Stats.DrawnPixelCount++;

            zCariesBuffer[index] = z;

            CariesScreen[index] = 1;
        }
    }
}
