using System;
using System.Numerics;

namespace SoftRenderingApp3D {
    public interface IProjection {
        event EventHandler ProjectionChanged;
        Matrix4x4 ProjectionMatrix(float w, float h);
    }
}