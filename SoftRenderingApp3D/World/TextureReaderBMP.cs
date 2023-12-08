using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;

namespace SoftRenderingApp3D {
    public class TextureReaderBMP : ITextureReader {

        // Checks if file-path is valid
        
        public Texture ReadImage(string filepath) {
            if (!File.Exists(filepath)) {
                throw new FileNotFoundException($"Error reading texture. File {filepath} not found!");
            }

            using Bitmap bmp = new Bitmap(filepath);
            var imageData = new Vector3[bmp.Width * bmp.Height];

            
            // Locks bitmap in memory allowing direct access to data. Faster the using 'GetPixel'. Benchmarks reduced time from 2,276ms to 402ms  
            
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

            // The code below extracts daat from colors and normalizes them
            
            for (int i = 0; i < bmp.Height; i++) {
                for (int j = 0; j < bmp.Width; j++) {
                    int position = (i * bitmapData.Stride) + j * bytesPerPixel;
                    float r = rgbValues[position + 2] / 255f;
                    float g = rgbValues[position + 1] / 255f;
                    float b = rgbValues[position] / 255f;

                    imageData[i * bmp.Width + j] = new Vector3(r, g, b);
                }
            }

            // Unlocks data, making it accessible to other proccessess
            
            bmp.UnlockBits(bitmapData);

            return new Texture(imageData, bmp.Width, bmp.Height);
        }
    }
}