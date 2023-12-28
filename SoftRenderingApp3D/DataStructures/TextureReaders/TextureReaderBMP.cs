using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;

namespace SoftRenderingApp3D.DataStructures.TextureReaders {
    public class TextureReaderBMP : ITextureReader {
        // Reads image data from .bmp file and creates a new texture
        public Texture.Texture ReadImage(string filepath) {
            if(!File.Exists(filepath)) {
                throw new FileNotFoundException($"Error reading texture. File {filepath} not found!");
            }

            using var bmp = new Bitmap(filepath);
            var imageData = new Vector3[bmp.Width * bmp.Height];

            var bitmapData = bmp.LockBits(
                new Rectangle(0, 0, bmp.Width, bmp.Height),
                ImageLockMode.ReadOnly,
                bmp.PixelFormat);

            // Calculates bytes per pixel

            var bytesPerPixel = Image.GetPixelFormatSize(bmp.PixelFormat) / 8;
            var ptr = bitmapData.Scan0;

            // Copies pixel data from bitmap to rbgValues array. Marshalling is used to copy data from memory that is managed to one that is not.

            var dataLength = Math.Abs(bitmapData.Stride) * bmp.Height;
            var rgbValues = new byte[dataLength];
            Marshal.Copy(ptr, rgbValues, 0, dataLength);

            for(var i = 0; i < bmp.Height; i++) {
                for(var j = 0; j < bmp.Width; j++) {
                    var position = i * bitmapData.Stride + j * bytesPerPixel;
                    float r = rgbValues[position + 2];
                    float g = rgbValues[position + 1];
                    float b = rgbValues[position];

                    imageData[i * bmp.Width + j] = new Vector3(r, g, b);
                }
            }

            // Unlocks data, making it accessible to other processes.

            bmp.UnlockBits(bitmapData);

            return new Texture.Texture(imageData, bmp.Width, bmp.Height);
        }
    }
}