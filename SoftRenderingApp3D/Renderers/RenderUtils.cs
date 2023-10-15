using System.Numerics;

namespace SoftRenderingApp3D {

    public class RenderUtils {
        public static float surfaceOpacity = 0.5f;
        public static float subsurfaceVisibility = 1.0f - surfaceOpacity;
        public static float lightWeight = 0.6f;
        public static float subsurfaceScatteringWeight = 1.0f - lightWeight;


        public static ColorRGB subsurfaceColor = new ColorRGB(255, 255, 255);
        public static ColorRGB surfaceColor = new ColorRGB(255, 255, 255);
    }
}