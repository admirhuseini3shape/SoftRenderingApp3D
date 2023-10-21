using System;
using System.Numerics;

namespace SoftRenderingApp3D {

    public class RenderUtils {
        public static float surfaceOpacity = 0.5f;
        public static float subsurfaceVisibility = 1.0f - surfaceOpacity;
        public static float lightWeight = 0.6f;
        public static float subsurfaceScatteringWeight = 1.0f - lightWeight;

        // Controls the drop off for the subsurface scattering effect
        public static float subsurfaceDecay = 0.1f;

        // Describes whether subsurface scattering needs recalculation
        public static bool recalcSubsurfaceScattering = true;

        // These are kept permanent so the color can be reverted
        public static ColorRGB SubsurfaceColor = ColorRGB.Purple;
        public static ColorRGB SurfaceColor = ColorRGB.White;

        public static ColorRGB surfaceColor = SurfaceColor;
        public static ColorRGB subsurfaceColor = SubsurfaceColor;

        public static bool GaussianBlur = false;

        public static void ToggleGaussianBlur() {
            GaussianBlur = !GaussianBlur;
        }

        public static float GaussianBlurStDev = 1.0f;

        public static float GaussianNormallizationConst = 1.0f / (2 * (float)Math.PI * GaussianBlurStDev * GaussianBlurStDev);

        public static int BlurRadiusInPixels = (int)GaussianBlurStDev * 3;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x">Distance from center point X.</param>
        /// <param name="y">Distance from center point Y</param>
        /// <returns></returns>
        public static float GetGaussianAt(int x, int y) {
            float exponent = -(x * x + y * y) / (2 * RenderUtils.GaussianBlurStDev * RenderUtils.GaussianBlurStDev);
            return RenderUtils.GaussianNormallizationConst * (float)Math.Exp(exponent);
        }

        public static float[,] CalcGaussianMatrix(int radius) {
            float[,] result = new float[radius + 1, radius + 1];
            for(int i = 0; i <= radius; i++) {
                for(int j = 0; j <= radius; j++) {
                    result[i, j] = GetGaussianAt(Math.Abs(i), Math.Abs(j));
                }
            }
            return result;
        }

        public static float[,] Gaussian = CalcGaussianMatrix(BlurRadiusInPixels);


        public static void ChangeVisibility(int value) {
            surfaceOpacity = value / 100.0f;
            subsurfaceVisibility = 1.0f - surfaceOpacity;
        }
        public static void ChangeSubsurfaceScatteringStrength(int value) {
            lightWeight = 1 - (value / 100.0f);
            subsurfaceScatteringWeight = 1.0f - lightWeight;
        }

        public static void ChangeSubsurfaceColor(int value) {
            subsurfaceColor = (value / 100.0f) * SubsurfaceColor;

        }
        public static void ChangeSurfaceColor(int value) {
            surfaceColor = (value / 100.0f) * SurfaceColor;
            
        }

        public static void ChangeSubsurfaceDecay(int value) {
            subsurfaceDecay = value / 100.0f;
            // Subsurface scattering needs recalculation
            recalcSubsurfaceScattering = true;
        }

        public static void ChangeGaussianBlurStDev(int value) {
            GaussianBlurStDev = (value  + 50.0f) / 100.0f;
            BlurRadiusInPixels = (int)GaussianBlurStDev * 3;
            Gaussian = CalcGaussianMatrix(BlurRadiusInPixels);
        }
    }
}