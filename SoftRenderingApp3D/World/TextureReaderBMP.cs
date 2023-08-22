using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace WinForms3D.World {
    class TextureReaderBMP : ITextureReader {

        // Reads image data from .bmp file and creates a new texture
        public Texture ReadImage(string path) {
            // Load the file 
            Bitmap bmp = new Bitmap(path);

            // Check if file is read sucessfully
            if(bmp == null) {
                throw new Exception($"Error reading texture. File {path} not found!");
            }

            // Create read image data
            Vector3[] imageData = new Vector3[bmp.Width * bmp.Height];
            for (int i = 0; i < bmp.Width; i++) {
                for(int j = 0; j < bmp.Height; j++) {
                    // Get color data for pixel
                    var color = bmp.GetPixel(i, j);

                    imageData[i] = new Vector3(color.R, color.G, color.B);
                }
            }

            return new Texture(imageData, bmp.Width, bmp.Height);
        }
    }
}
