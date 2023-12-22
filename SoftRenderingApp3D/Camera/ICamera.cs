using System;
using System.Numerics;

namespace SoftRenderingApp3D.Camera {
    public interface ICamera {
        event EventHandler CameraChanged;
        Matrix4x4 ViewMatrix();
    }
}