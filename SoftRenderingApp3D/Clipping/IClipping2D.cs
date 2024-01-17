using System.Numerics;

namespace SoftRenderingApp3D.Clipping
{
    public interface IClipping2D
    {
        bool Clip(ref Vector2 begin, ref Vector2 end);
    }
}
