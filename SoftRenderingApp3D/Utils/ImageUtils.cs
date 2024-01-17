using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace SoftRenderingApp3D.Utils
{
    public static class ImageUtils
    {
        // Taken from https://stackoverflow.com/a/11740297/24472

        public static void FillBitmap(Bitmap bmp, int[] buffer, int width, int height)
        {
            var rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            var bmpData = bmp.LockBits(rect, ImageLockMode.WriteOnly,
                bmp.PixelFormat);
            
                IntPtr ptr = bmpData.Scan0;
                int bytes = width * height;
                Marshal.Copy(buffer, 0, ptr, bytes);
            
                bmp.UnlockBits(bmpData);
            
        }
    }
}
