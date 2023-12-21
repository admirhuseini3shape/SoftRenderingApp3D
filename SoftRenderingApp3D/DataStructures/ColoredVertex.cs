using System.Numerics;


namespace SoftRenderingApp3D {
    public class ColoredVertex {

        public ColoredVertex(Vector3 position) {
            this.position = position;
            this.color = ColorRGB.Black;
        }

        public ColoredVertex(Vector3 position, ColorRGB color) {
            this.position = position;
            this.color = color;
        }
        public Vector3 position { get; }

        public ColorRGB color { get; set; }

    }
}
