using System.Numerics;

namespace SoftRenderingApp3D {

    public class RenderUtils {

        public static void drawGrid(FrameBuffer surface, WireFramePainter wireFramePainter, Matrix4x4 world2Projection, float from, float to) {
            for(var xz = from; xz <= to; xz++) {
                drawLine(surface, wireFramePainter, world2Projection, new Vector3(xz, 0, from), new Vector3(xz, 0, to), xz == 0 ? ColorRGB.Red : ColorRGB.Green);
                drawLine(surface, wireFramePainter, world2Projection, new Vector3(from, 0, xz), new Vector3(to, 0, xz), ColorRGB.Green);
            }
        }
        
        public static void drawGrid3d(FrameBuffer surface, WireFramePainter wireFramePainter, Matrix4x4 world2Projection, float from, float to, float spacing) {
            for(var i = from; i <= to; i += spacing) {
                // Lines parallel to X-axis
                drawLine(surface, wireFramePainter, world2Projection, new Vector3(from, i, 0), new Vector3(to, i, 0), ColorRGB.Green);
                drawLine(surface, wireFramePainter, world2Projection, new Vector3(from, i, to), new Vector3(to, i, to), ColorRGB.Green); // Y constant, Z at 'to'

                drawLine(surface, wireFramePainter, world2Projection, new Vector3(from, 0, i), new Vector3(to, 0, i), ColorRGB.Green);
                drawLine(surface, wireFramePainter, world2Projection, new Vector3(from, to, i), new Vector3(to, to, i), ColorRGB.Green); // Z constant, Y at 'to'

                // Lines parallel to Y-axis
                drawLine(surface, wireFramePainter, world2Projection, new Vector3(i, from, 0), new Vector3(i, to, 0), ColorRGB.Blue); 
                drawLine(surface, wireFramePainter, world2Projection, new Vector3(i, from, to), new Vector3(i, to, to), ColorRGB.Blue); // X constant, Z at 'to'

                
                drawLine(surface, wireFramePainter, world2Projection, new Vector3(i, 0, from), new Vector3(i, 0, to), ColorRGB.Red); 
                drawLine(surface, wireFramePainter, world2Projection, new Vector3(i, to, from), new Vector3(i, to, to), ColorRGB.Red); // X constant, Y at 'to'
            }
        }
        
        static void drawAxisWithArrowhead(FrameBuffer surface, WireFramePainter wireFramePainter, Matrix4x4 world2Projection, Vector3 start, Vector3 end, ColorRGB color, Vector3[] arrowDirections) {
            drawLine(surface, wireFramePainter, world2Projection, start, end, color);
  
            foreach (var dir in arrowDirections) {
                drawLine(surface, wireFramePainter, world2Projection, end, end + dir, color);
            }
        }
        
        public static void drawAxes(FrameBuffer surface, WireFramePainter wireFramePainter, Matrix4x4 world2Projection) {
            
            Vector3[] xArrowDirections = { new Vector3(-.25f, .25f, 0), new Vector3(-.25f, 0, .25f), new Vector3(-.25f, 0, -.25f), new Vector3(-.25f, -.25f, 0) };
            Vector3[] yArrowDirections = { new Vector3(.25f, -.25f, 0), new Vector3(0, -.25f, .25f), new Vector3(0, -.25f, -.25f), new Vector3(-.25f, -.25f, 0) };
            Vector3[] zArrowDirections = { new Vector3(.25f, 0, -.25f), new Vector3(-.25f, 0, -.25f), new Vector3(0, .25f, -.25f), new Vector3(0, -.25f, -.25f) };
            
            drawAxisWithArrowhead(surface, wireFramePainter, world2Projection, new Vector3(0, 0, 0), new Vector3(1, 0, 0), ColorRGB.Red, xArrowDirections);
            drawAxisWithArrowhead(surface, wireFramePainter, world2Projection, new Vector3(0, 0, 0), new Vector3(0, 1, 0), ColorRGB.Green, yArrowDirections);
            drawAxisWithArrowhead(surface, wireFramePainter, world2Projection, new Vector3(0, 0, 0), new Vector3(0, 0, 1), ColorRGB.Blue, zArrowDirections);
        }

        static void drawLine(FrameBuffer surface, WireFramePainter wireFramePainter, Matrix4x4 world2Projection, Vector3 worldP0, Vector3 worldP1, ColorRGB color) {
            var projectionP0 = Vector4.Transform(worldP0, world2Projection);
            var projectionP1 = Vector4.Transform(worldP1, world2Projection);

            wireFramePainter.DrawLine(surface, color, projectionP0, projectionP1);
        }
    }
}