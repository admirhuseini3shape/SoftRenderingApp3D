using SoftRenderingApp3D.Controls;
using SoftRenderingApp3D.Utils;
using System;
using System.Numerics;

namespace SoftRenderingApp3D.Camera
{
    // Adapted from https://github.com/dwmkerr/sharpgl/blob/master/source/SharpGL/Core/SharpGL.SceneGraph/Core/ArcBall.cs

    public class ArcBallCam : ICamera
    {
        private Vector3 position;

        public float Radius = .3f;
        private Quaternion rotation = Quaternion.Identity;

        private float radius_squared
        {
            get
            {
                return Radius * Radius;
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
                if(value.TryUpdateOther(ref rotation))
                    CameraChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public Vector3 Position
        {
            get
            {
                return position;
            }
            set
            {
                if(!value.TryUpdateOther(ref position))
                    return;

                CameraChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public event EventHandler CameraChanged;

        public Matrix4x4 ViewMatrix()
        {
            return Matrix4x4.CreateFromQuaternion(rotation) * Matrix4x4.CreateTranslation(position);
        }

        public void SetPosition(Vector3 newPosition, Quaternion newRotation)
        {
            if(!newRotation.TryUpdateOther(ref rotation) && !newPosition.TryUpdateOther(ref position))
                return;

            CameraChanged?.Invoke(this, EventArgs.Empty);
        }

        // Hyperboloid mapping taken from https://www.opengl.org/wiki/Object_Mouse_Trackball

        internal Vector3 MapToSphere(Vector2 v)
        {
            var P = new Vector3(v.X, -v.Y, 0);

            var XY_squared = P.LengthSquared();

            if(XY_squared <= .5f * radius_squared)
            {
                P.Z = (float)Math.Sqrt(radius_squared - XY_squared); // Pythagore
            }
            else
            {
                P.Z = 0.5f * radius_squared / (float)Math.Sqrt(XY_squared); // Hyperboloid
            }

            return Vector3.Normalize(P);
        }

        internal Quaternion CalculateQuaternion(Vector3 startV, Vector3 currentV)
        {
            var cross = Vector3.Cross(startV, currentV);

            if(cross.Length() > MathUtils.Epsilon)
            {
                return new Quaternion(cross, Vector3.Dot(startV, currentV));
            }

            return Quaternion.Identity;
        }
    }
}
