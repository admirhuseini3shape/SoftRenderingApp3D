using System.Numerics;

namespace SoftRenderingApp3D.Painter {
    public struct PaintedVertex {
        public Vector3 WorldNormal { get; }
        public Vector3 ScreenPoint { get; }
        public Vector3 WorldPoint { get; }
        public ColorRGB Color { get; set; }

        public PaintedVertex(Vector3 worldNormal, Vector3 screenPoint, Vector3 worldPoint, ColorRGB color) {
            WorldNormal = worldNormal;
            ScreenPoint = screenPoint;
            WorldPoint = worldPoint;
            Color = color;
        }
    }
}