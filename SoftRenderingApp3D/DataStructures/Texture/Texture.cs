using SoftRenderingApp3D.Utils;
using System.Numerics;

namespace SoftRenderingApp3D.DataStructures.Texture {
    public class Texture {
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
            this.width = width;
            this.height = height;
        }
        /// <summary>
        /// Returns the color value of the pixel nearest to the texture coordinates provided.
        /// </summary>
        /// <param name="u">Float representing the X texture coordinate for a vertex.</param>
        /// <param name="v">Float representing the Y texture coordinate for a vertex.</param>
        /// <returns></returns>
        public ColorRGB GetPixelColorNearestFiltering(float u, float v) {
            int pixel_x = (int)(MathUtils.Clamp((int)(u * width), 0, width - 1));
            int pixel_y = (int)(MathUtils.Clamp((int)(v * height), 0, height - 1));
           
            var color = imageData[pixel_y * width + pixel_x];

            return new ColorRGB((byte)color.X, (byte)color.Y, (byte)color.Z);
        }
        /// <summary>
        /// Returns a linearly interpolated color value of the pixels around the texture coordinates provided.
        /// </summary>
        /// <param name="u">Float representing the X texture coordinate for a vertex.</param>
        /// <param name="v">Float representing the Y texture coordinate for a vertex.</param>
        /// <returns></returns>
        public ColorRGB GetPixelColorLinearFiltering(float u, float v) {
            float pixelCoordinateX = (MathUtils.Clamp((u * width), 0, width - 1));
            float pixelCoordinateY = (MathUtils.Clamp((v * height), 0, height - 1));

            // Get the ratio between neighbouring pixels
            float xCoordinateRatio = pixelCoordinateX - (int)(pixelCoordinateX);
            float yCoordinateRatio = pixelCoordinateY - (int)(pixelCoordinateY);

            var color =
                    // Interpolate between upper two pixels
                    (1 - yCoordinateRatio) *
                        ((1 - xCoordinateRatio) * imageData[(int)pixelCoordinateY * width + (int)(pixelCoordinateX)] +
                         (xCoordinateRatio) * imageData[(int)pixelCoordinateY * width + (int)(MathUtils.Clamp((int)(pixelCoordinateX) + 1, 0, width - 1))])
                    +
                    // Interpolate between lower two pixels
                    (yCoordinateRatio *
                       ((1 - xCoordinateRatio) * imageData[(int)(MathUtils.Clamp((int)(pixelCoordinateY) + 1, 0, height - 1)) * width + (int)(pixelCoordinateX)] +
                         (xCoordinateRatio) * imageData[(int)(MathUtils.Clamp((int)(pixelCoordinateY) + 1, 0, height - 1)) * width + (int)(MathUtils.Clamp((int)(pixelCoordinateX) + 1, 0, width - 1))]));

            return new ColorRGB((byte)color.X, (byte)color.Y, (byte)color.Z);
        }
    }
}
