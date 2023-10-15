using System.Numerics;

namespace SoftRenderingApp3D {
    struct PaintedVertex
    {
        public Vector3 WorldNormal { get; }
        public Vector3 ScreenPoint { get; }
        public ColoredVertex WorldPoint { get; }


        public PaintedVertex(Vector3 worldNormal, Vector3 screenPoint, ColoredVertex worldPoint)
        {
            WorldNormal = worldNormal;
            ScreenPoint = screenPoint;
            WorldPoint = worldPoint;
        }
    }
}