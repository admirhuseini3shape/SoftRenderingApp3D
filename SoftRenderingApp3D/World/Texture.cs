using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace WinForms3D.World {
    class Texture {
        public Vector3[] imageData { get; private set; }

        public int width { get; private set; }
        public int height { get; private set; }

        /// <summary>
        /// Creates a texture.
        /// </summary>
        /// <param name="imageData">The color data of the iamge as an array of Vector3 objects.</param>
        /// <param name="width">The width of the texture image.</param>
        /// <param name="height">The height of the texture image.</param>
        public Texture(Vector3[] imageData, int width, int height) {
            this.imageData = imageData;
        }
        /// <summary>
        /// Returns the color value of the pixel nearest to the texture coordinates provided.
        /// </summary>
        /// <param name="u">Float representing the X texture coordinate for a vertex.</param>
        /// <param name="v">Float representing the Y texture coordinate for a vertex.</param>
        /// <returns></returns>
        public Vector3 GetRGBNearestFiltering(float u, float v) {
            
        }
        /// <summary>
        /// Returns a linearly interpolated color value of the pixels around the texture coordinates provided.
        /// </summary>
        /// <param name="u">Float representing the X texture coordinate for a vertex.</param>
        /// <param name="v">Float representing the Y texture coordinate for a vertex.</param>
        /// <returns></returns>
        public Vector3 GetRGBLinearFiltering(float u, float v) {

        }
    }
}
