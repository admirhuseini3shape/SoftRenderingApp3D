using System.Numerics;

namespace SoftRenderingApp3D.Clipping {
    public interface IClippingHomogeneous {
        bool Clip(ref Vector4 begin, ref Vector4 end);
    }
}