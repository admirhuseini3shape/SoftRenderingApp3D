using System;

namespace SoftRenderingApp3D.Utils
{
    public class RenderUtils
    {
        public static float surfaceOpacity = 0.5f;
        public static float subsurfaceVisibility = 1.0f - surfaceOpacity;
        public static float lightWeight = 0.6f;
        public static float subsurfaceScatteringWeight = 1.0f - lightWeight;

        // Controls the drop off for the subsurface scattering effect
        public static float subsurfaceDecay = 0.1f;

        // Describes whether subsurface scattering needs recalculation
        public static bool recalcSubsurfaceScattering = true;

        // These are kept permanent so the color can be reverted
        //public static ColorRGB SubsurfaceColor = new ColorRGB(127, 127, 255, (byte)(subsurfaceVisibility * 255));
        public static ColorRGB SubsurfaceColor = ColorRGB.White;
        public static ColorRGB SurfaceColor = ColorRGB.White;

        public static ColorRGB surfaceColor = SurfaceColor;
        public static ColorRGB subsurfaceColor = SubsurfaceColor;

        public static bool GaussianBlur;

        public static float GaussianBlurStDev = 1.0f;

        public static float GaussianNormallizationConst =
            1.0f / (2 * (float)Math.PI * GaussianBlurStDev * GaussianBlurStDev);

        public static int BlurRadiusInPixels = (int)GaussianBlurStDev * 3;

        public static float[,] Gaussian = CalculateGaussianMatrix(BlurRadiusInPixels * 2 + 1, GaussianBlurStDev);

        public static void ToggleGaussianBlur()
        {
            GaussianBlur = !GaussianBlur;
        }

        /// <summary>
        /// </summary>
        /// <param name="x">Distance from center point X.</param>
        /// <param name="y">Distance from center point Y</param>
        /// <returns></returns>
        public static float GetGaussianAt(int x, int y)
        {
            var exponent = -(x * x + y * y) / (2 * GaussianBlurStDev * GaussianBlurStDev);
            return GaussianNormallizationConst * (float)Math.Exp(exponent);
        }

        public static float[,] CalcGaussianMatrix(int radius)
        {
            var result = new float[radius + 1, radius + 1];
            for(var i = 0; i <= radius; i++)
            {
                for(var j = 0; j <= radius; j++)
                {
                    result[i, j] = GetGaussianAt(Math.Abs(i), Math.Abs(j));
                }
            }

            return result;
        }

        /// <summary>
        ///     Other implementation for the calculation of the Gaussian kernel.
        /// </summary>
        /// <param name="length">Diameter of blur effect</param>
        /// <param name="weight">Standard deviation in the formula or strength of the blur effect.</param>
        /// <returns></returns>
        public static float[,] CalculateGaussianMatrix(int length, double weight)
        {
            var Kernel = new float[length, length];
            float sumTotal = 0;


            var kernelRadius = length / 2;
            double distance = 0;


            var calculatedEuler = 1.0 /
                                  (2.0 * Math.PI * Math.Pow(weight, 2));


            for(var filterY = -kernelRadius;
                filterY <= kernelRadius;
                filterY++)
            {
                for(var filterX = -kernelRadius;
                    filterX <= kernelRadius;
                    filterX++)
                {
                    distance = (filterX * filterX +
                                filterY * filterY) /
                               (2 * (weight * weight));


                    Kernel[filterY + kernelRadius,
                            filterX + kernelRadius] =
                        (float)(calculatedEuler * Math.Exp(-distance));


                    sumTotal += Kernel[filterY + kernelRadius,
                        filterX + kernelRadius];
                }
            }


            for(var y = 0; y < length; y++)
            {
                for(var x = 0; x < length; x++)
                {
                    Kernel[y, x] = Kernel[y, x] *
                                   (1.0f / sumTotal);
                }
            }

            return Kernel;
        }


        public static void ChangeVisibility(int value)
        {
            surfaceOpacity = value / 100.0f;
            subsurfaceVisibility = 1.0f - surfaceOpacity;
        }

        public static void ChangeSubsurfaceScatteringStrength(int value)
        {
            lightWeight = 1 - value / 100.0f;
            subsurfaceScatteringWeight = 1.0f - lightWeight;
        }

        public static void ChangeSubsurfaceColor(int value)
        {
            subsurfaceColor = value / 100.0f * SubsurfaceColor;
        }

        public static void ChangeSurfaceColor(int value)
        {
            surfaceColor = value / 100.0f * SurfaceColor;
        }

        public static void ChangeSubsurfaceDecay(int value)
        {
            subsurfaceDecay = value / 100.0f;
            // Subsurface scattering needs recalculation
            recalcSubsurfaceScattering = true;
        }

        public static void ChangeGaussianBlurStDev(int value)
        {
            GaussianBlurStDev = (value + 50.0f) / 100.0f;
            BlurRadiusInPixels = (int)GaussianBlurStDev * 3;
            Gaussian = CalculateGaussianMatrix(BlurRadiusInPixels * 2 + 1, GaussianBlurStDev);
        }
    }
}
