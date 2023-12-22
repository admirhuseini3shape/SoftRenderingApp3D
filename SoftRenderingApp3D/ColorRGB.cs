using SoftRenderingApp3D.Utils;
using System;
using System.Drawing;

namespace SoftRenderingApp3D {

    // Taken from the highly efficient https://referencesource.microsoft.com/#System.Drawing/commonui/System/Drawing/Color.cs

    public struct ColorRGB {

        const int ARGBAlphaShift = 24;
        const int ARGBRedShift = 16;
        const int ARGBGreenShift = 8;
        const int ARGBBlueShift = 0;

        UInt32 value; // since rgb is 32bit, why yse 64 bit long?

        public ColorRGB(byte r, byte g, byte b) => value = (unchecked((uint)(r << ARGBRedShift | g << ARGBGreenShift | b << ARGBBlueShift | 255 << ARGBAlphaShift))) & 0xffffffff;
        public ColorRGB(byte r, byte g, byte b, byte Alpha) => value = (unchecked((uint)(r << ARGBRedShift | g << ARGBGreenShift | b << ARGBBlueShift | Alpha << ARGBAlphaShift))) & 0xffffffff;
        public ColorRGB(Color color) => value = (unchecked((uint)(color.R << ARGBRedShift | color.G << ARGBGreenShift | color.B << ARGBBlueShift | color.A << ARGBAlphaShift))) & 0xffffffff;
        
        
        
        // public byte G { get => (byte)((value >> ARGBGreenShift) & 0xFF); set => this.value = (unchecked((uint)(R << ARGBRedShift | value << ARGBGreenShift | B << ARGBBlueShift | Alpha << ARGBAlphaShift))) & 0xffffffff; }
        /*
         * The code above seems to have a certain issue. What I think it was trying to do was to shift all values in
         * their respective 32bit fields, so it would shift everything. e.g
         * Assume a ARGB value of :
         *
         *  A        R        G        B
         * 11111111 00000000 11111111 11111111 - Initial Values , the values are shifted to their respective positions and added with the operations,
         *
         * but when performing value << ARGBGreenShift/8, it will destory the structure, the final result would become
         *
         * 00000000 11111111 11111111 00000000, which doesnt isolate the green component at all. 
         * 
         *I think the original implementation tried to use a snippet from the  https://referencesource.microsoft.com/#System.Drawing/commonui/System/Drawing/Color.cs,
         *
         *  private static long MakeArgb(byte alpha, byte red, byte green, byte blue) {
            return(long)(unchecked((uint)(red << ARGBRedShift |
                         green << ARGBGreenShift | 
                         blue << ARGBBlueShift | 
                         alpha << ARGBAlphaShift))) & 0xffffffff;
            }
         *
         *  but maybe there was some mistake along the way
         *
         *
         * The current implementation relies on using a mask , which isolates the required color value with an xor
         * operation with 0xFFFFFFFF, 
         * making G 0 and everything else 1, running a bitwise and operation between them and the values, which
         * of course will result in everything being 1 expect the color value. Now when combining the oringinal value with
         * the mask ,  and adding with or the new value which is shifted by 16 bits, you get a final modifiable value
         */
        
        public byte R 
        {
            get => (byte)((value >> ARGBRedShift) & 0xFF);
            set 
            {
                uint mask = 0xFFFFFFFF ^ (0xFFu << ARGBRedShift); // Mask to clear the Red component
                this.value = (this.value & mask) | ((uint)value << ARGBRedShift); // Set new Red value
            }
        }
        public byte G 
        {
            get => (byte)((value >> ARGBGreenShift) & 0xFF);
            set 
            {
                uint mask = 0xFFFFFFFF ^ (0xFFu << ARGBGreenShift); // Mask to clear the Green component
                this.value = (this.value & mask) | ((uint)value << ARGBGreenShift); // Set new Green value
            }
        }
        public byte B 
        {
            get => (byte)((value >> ARGBBlueShift) & 0xFF);
            set 
            {
                uint mask = 0xFFFFFFFF ^ (0xFFu << ARGBBlueShift); // Mask to clear the Blue component
                this.value = (this.value & mask) | ((uint)value << ARGBBlueShift); // Set new Blue value
            }
        }

        public byte Alpha 
        {
            get => (byte)((value >> ARGBAlphaShift) & 0xFF);
            set 
            {
                uint mask = 0xFFFFFFFF ^ (0xFFu << ARGBAlphaShift); // Mask to clear the Alpha component
                this.value = (this.value & mask) | ((uint)value << ARGBAlphaShift); // Set new Alpha value
            }
        }

        public int Color { get => (int)value; }

        public static ColorRGB operator *(float f, ColorRGB color) => new ColorRGB((byte)(f * color.R), (byte)(f * color.G), (byte)(f * color.B), color.Alpha);

        public static ColorRGB operator +(ColorRGB color1, ColorRGB color2) => AlphaBlend(color1, color2);

        public static ColorRGB operator -(ColorRGB color1, ColorRGB color2) => new ColorRGB((byte)(MathUtils.Clamp(color1.R - color2.R, 0, 255)), (byte)(MathUtils.Clamp(color1.G - color2.G, 0, 255)), (byte)(MathUtils.Clamp(color1.B - color2.B, 0, 255)));

        public static ColorRGB AlphaBlend(ColorRGB color1, ColorRGB color2) {
            var alpha1 = color1.Alpha / 255.0f;
            var alpha2 = color2.Alpha / 255.0f;
            var alpha0 = alpha1 + alpha2 * (1 - alpha1);

            byte R = (byte)((1.0f / alpha0) * (alpha1 * color1.R + (1.0f - alpha1) * alpha2 * color2.R));
            byte G = (byte)((1.0f / alpha0) * (alpha1 * color1.G + (1.0f - alpha1) * alpha2 * color2.G));
            byte B = (byte)((1.0f / alpha0) * (alpha1 * color1.B + (1.0f - alpha1) * alpha2 * color2.B));
            ColorRGB color0 = new ColorRGB(R, G, B, (byte)(255 * alpha0));
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
