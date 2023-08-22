using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SoftRenderingApp3D {
    public class TextureReaderBMP : ITextureReader {

        // Reads image data from .bmp file and creates a new texture
        public Texture ReadImage(string filepath) {
            // Load the file 
            Bitmap bmp;

            using(Stream bmpStream = System.IO.File.Open(filepath, System.IO.FileMode.Open)) {
                Image image = Image.FromStream(bmpStream);

                bmp = new Bitmap(image);

            }

            // Check if file is read sucessfully
            if(bmp == null) {
                throw new Exception($"Error reading texture. File {filepath} not found!");
            }

            // Create read image data
            Vector3[] imageData = new Vector3[bmp.Width * bmp.Height];

            for (int i = 0; i < bmp.Height; i++) {
                for(int j = 0; j < bmp.Width; j++) {
                    // Get color data for pixel
                    var color = bmp.GetPixel(i, j);

                    imageData[i * bmp.Width + j] = new Vector3(color.R, color.G, color.B);
                }
            }

            return new Texture(imageData, bmp.Width, bmp.Height);
        }
    }
}
