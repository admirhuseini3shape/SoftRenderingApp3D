using System;
using System.Numerics;

namespace SoftRenderingApp3D {
    public interface ICamera {
        event EventHandler CameraChanged;
        Matrix4x4 ViewMatrix();
    }
}