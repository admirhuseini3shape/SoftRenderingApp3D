using System.Numerics;

namespace SoftRenderingApp3D {

    public class RenderUtils {
        public static float surfaceOpacity = 0.5f;
        public static float subsurfaceVisibility = 1.0f - surfaceOpacity;
        public static float lightWeight = 0.6f;
        public static float subsurfaceScatteringWeight = 1.0f - lightWeight;

        // These are kept permanent so the color can be reverted
        public static ColorRGB SubsurfaceColor = new ColorRGB(255, 255, 255);
        public static ColorRGB SurfaceColor = new ColorRGB(255, 255, 255);

        public static ColorRGB surfaceColor = SubsurfaceColor;
        public static ColorRGB subsurfaceColor = SurfaceColor;

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
    }
}