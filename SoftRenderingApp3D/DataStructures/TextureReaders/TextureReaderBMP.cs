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

            using Bitmap bmp = new Bitmap(filepath);
            var imageData = new Vector3[bmp.Width * bmp.Height];
            
            var bitmapData = bmp.LockBits(
                new Rectangle(0, 0, bmp.Width, bmp.Height), 
                ImageLockMode.ReadOnly, 
                bmp.PixelFormat);

            // Calculates bytes per pixel

            int bytesPerPixel = Image.GetPixelFormatSize(bmp.PixelFormat) / 8;
            IntPtr ptr = bitmapData.Scan0;
            
            // Copies pixel data from bitmap to rbgValues array. Marshalling is used to copy data from memory that is managed to one that is not.

            int dataLength = Math.Abs(bitmapData.Stride) * bmp.Height;
            byte[] rgbValues = new byte[dataLength];
            Marshal.Copy(ptr, rgbValues, 0, dataLength);
            
            for (int i = 0; i < bmp.Height; i++) {
                for (int j = 0; j < bmp.Width; j++) {
                    int position = (i * bitmapData.Stride) + j * bytesPerPixel;
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
