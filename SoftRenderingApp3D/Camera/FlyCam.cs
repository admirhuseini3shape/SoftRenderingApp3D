using System;
using System.Numerics;

namespace SoftRenderingApp3D.Camera
{
    // Buggy

    public class FlyCam : ICamera
    {
        private Vector3 position;
        private Quaternion rotation = Quaternion.Identity;

        public Vector3 Position
        {
            get
            {
                return position;
            }
            set
            {
                if(position == value)
                {
                    return;
                }

                position = value;

                CameraChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public Quaternion Rotation
        {
            get
            {
                return rotation;
            }
            set
            {
                if(rotation == value)
                {
                    return;
                }

                rotation = value;

                CameraChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public event EventHandler CameraChanged;

        public Matrix4x4 ViewMatrix()
        {
            return Matrix4x4.CreateTranslation(position) * Matrix4x4.CreateFromQuaternion(rotation);
        }

        public override string ToString()
        {
            return $"Camera: P: {Position},R: {Rotation}";
        }
    }
}
