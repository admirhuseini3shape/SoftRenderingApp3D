using SoftRenderingApp3D.Utils;
using System.Numerics;

namespace SoftRenderingApp3D.DataStructures.Texture {
    public class Texture {
        /// <summary>
        ///     Creates a texture.
        /// </summary>
        /// <param name="imageData">The color data of the iamge as an array of Vector3 objects.</param>
        /// <param name="width">The width of the texture image.</param>
        /// <param name="height">The height of the texture image.</param>
        public Texture(Vector3[] imageData, int width, int height) {
            this.imageData = imageData;
            this.width = width;
            this.height = height;
        }

        public Vector3[] imageData { get; }

        public int width { get; }
        public int height { get; }

        /// <summary>
        ///     Returns the color value of the pixel nearest to the texture coordinates provided.
        /// </summary>
        /// <param name="u">Float representing the X texture coordinate for a vertex.</param>
        /// <param name="v">Float representing the Y texture coordinate for a vertex.</param>
        /// <returns></returns>
        public ColorRGB GetPixelColorNearestFiltering(float u, float v) {
            var pixel_x = (int)MathUtils.Clamp((int)(u * width), 0, width - 1);
            var pixel_y = (int)MathUtils.Clamp((int)(v * height), 0, height - 1);

            var color = imageData[pixel_y * width + pixel_x];

            return new ColorRGB((byte)color.X, (byte)color.Y, (byte)color.Z);
        }

        /// <summary>
        ///     Returns a linearly interpolated color value of the pixels around the texture coordinates provided.
        /// </summary>
        /// <param name="u">Float representing the X texture coordinate for a vertex.</param>
        /// <param name="v">Float representing the Y texture coordinate for a vertex.</param>
        /// <returns></returns>
        public ColorRGB GetPixelColorLinearFiltering(float u, float v) {
            var pixelCoordinateX = (u * width).Clamp(0, width - 1);
            var pixelCoordinateY = (v * height).Clamp(0, height - 1);

            // Get the ratio between neighbouring pixels
            var xCoordinateRatio = pixelCoordinateX - (int)pixelCoordinateX;
            var yCoordinateRatio = pixelCoordinateY - (int)pixelCoordinateY;

            var color =
                // Interpolate between upper two pixels
                (1 - yCoordinateRatio) *
                ((1 - xCoordinateRatio) * imageData[(int)pixelCoordinateY * width + (int)pixelCoordinateX] +
                 xCoordinateRatio *
                 imageData[
                     (int)pixelCoordinateY * width + (int)MathUtils.Clamp((int)pixelCoordinateX + 1, 0, width - 1)])
                +
                // Interpolate between lower two pixels
                yCoordinateRatio *
                ((1 - xCoordinateRatio) *
                 imageData[
                     (int)MathUtils.Clamp((int)pixelCoordinateY + 1, 0, height - 1) * width + (int)pixelCoordinateX] +
                 xCoordinateRatio *
                 imageData[
                     (int)MathUtils.Clamp((int)pixelCoordinateY + 1, 0, height - 1) * width +
                     (int)MathUtils.Clamp((int)pixelCoordinateX + 1, 0, width - 1)]);

            return new ColorRGB((byte)color.X, (byte)color.Y, (byte)color.Z);
        }
    }
}