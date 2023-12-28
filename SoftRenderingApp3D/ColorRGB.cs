using SoftRenderingApp3D.Utils;
using System;
using System.Drawing;

namespace SoftRenderingApp3D {
    // Taken from the highly efficient https://referencesource.microsoft.com/#System.Drawing/commonui/System/Drawing/Color.cs

    public struct ColorRGB {
        private const int ARGBAlphaShift = 24;
        private const int ARGBRedShift = 16;
        private const int ARGBGreenShift = 8;
        private const int ARGBBlueShift = 0;

        private UInt32 value; 
        public ColorRGB(byte r, byte g, byte b) {
            value = unchecked((uint)((r << ARGBRedShift) | (g << ARGBGreenShift) | (b << ARGBBlueShift) |
                                     (255 << ARGBAlphaShift))) & 0xffffffff;
        }

        public ColorRGB(byte r, byte g, byte b, byte Alpha) {
            value = unchecked((uint)((r << ARGBRedShift) | (g << ARGBGreenShift) | (b << ARGBBlueShift) |
                                     (Alpha << ARGBAlphaShift))) & 0xffffffff;
        }

        public ColorRGB(Color color) {
            value = unchecked((uint)((color.R << ARGBRedShift) | (color.G << ARGBGreenShift) |
                                     (color.B << ARGBBlueShift) | (color.A << ARGBAlphaShift))) & 0xffffffff;
        }
        
        public byte R {
            get {
                return (byte)((value >> ARGBRedShift) & 0xFF);
            }
            set {
                var mask = 0xFFFFFFFF ^ (0xFFu << ARGBRedShift); // Mask to clear the Red component
                this.value = (this.value & mask) | ((uint)value << ARGBRedShift); // Set new Red value
            }
        }

        public byte G {
            get {
                return (byte)((value >> ARGBGreenShift) & 0xFF);
            }
            set {
                var mask = 0xFFFFFFFF ^ (0xFFu << ARGBGreenShift); // Mask to clear the Green component
                this.value = (this.value & mask) | ((uint)value << ARGBGreenShift); // Set new Green value
            }
        }

        public byte B {
            get {
                return (byte)((value >> ARGBBlueShift) & 0xFF);
            }
            set {
                var mask = 0xFFFFFFFF ^ (0xFFu << ARGBBlueShift); // Mask to clear the Blue component
                this.value = (this.value & mask) | ((uint)value << ARGBBlueShift); // Set new Blue value
            }
        }

        public byte Alpha {
            get {
                return (byte)((value >> ARGBAlphaShift) & 0xFF);
            }
            set {
                var mask = 0xFFFFFFFF ^ (0xFFu << ARGBAlphaShift); // Mask to clear the Alpha component
                this.value = (this.value & mask) | ((uint)value << ARGBAlphaShift); // Set new Alpha value
            }
        }

        public int Color {
            get {
                return (int)value;
            }
        }

        public static ColorRGB operator *(float f, ColorRGB color) {
            return new ColorRGB((byte)(f * color.R), (byte)(f * color.G), (byte)(f * color.B), color.Alpha);
        }

        public static ColorRGB operator +(ColorRGB color1, ColorRGB color2) {
            return AlphaBlend(color1, color2);
        }

        public static ColorRGB operator -(ColorRGB color1, ColorRGB color2) {
            return new ColorRGB((byte)MathUtils.Clamp(color1.R - color2.R, 0, 255),
                (byte)MathUtils.Clamp(color1.G - color2.G, 0, 255), (byte)MathUtils.Clamp(color1.B - color2.B, 0, 255));
        }

        public static ColorRGB AlphaBlend(ColorRGB color1, ColorRGB color2) {
            var alpha1 = color1.Alpha / 255.0f;
            var alpha2 = color2.Alpha / 255.0f;
            var alpha0 = alpha1 + alpha2 * (1 - alpha1);

            var R = (byte)(1.0f / alpha0 * (alpha1 * color1.R + (1.0f - alpha1) * alpha2 * color2.R));
            var G = (byte)(1.0f / alpha0 * (alpha1 * color1.G + (1.0f - alpha1) * alpha2 * color2.G));
            var B = (byte)(1.0f / alpha0 * (alpha1 * color1.B + (1.0f - alpha1) * alpha2 * color2.B));
            var color0 = new ColorRGB(R, G, B, (byte)(255 * alpha0));
            //Console.WriteLine($"color: {color0}, alpha1 {alpha1}, alpha2 {alpha2}, alpha0 {alpha0}");
            return color0;
        }

        public static ColorRGB Yellow { get; } = new ColorRGB(255, 255, 0, 255);
        public static ColorRGB Blue { get; } = new ColorRGB(0, 0, 255, 255);
        public static ColorRGB LightBlue { get; } = new ColorRGB(127, 127, 255);

        public static ColorRGB Gray { get; } = new ColorRGB(127, 127, 127);
        public static ColorRGB Green { get; } = new ColorRGB(0, 255, 0);
        public static ColorRGB Red { get; } = new ColorRGB(255, 0, 0);
        public static ColorRGB Magenta { get; } = new ColorRGB(255, 0, 255);
        public static ColorRGB White { get; } = new ColorRGB(255, 255, 255);
        public static ColorRGB Black { get; } = new ColorRGB(0, 0, 0, 255);

        public static ColorRGB Purple { get; } = new ColorRGB(128, 0, 128);

        public override string ToString() {
            return $"Color: ({R}, {G}, {B}, {Alpha})";
        }
    }
}