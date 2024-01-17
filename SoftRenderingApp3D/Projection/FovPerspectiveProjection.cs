using SoftRenderingApp3D.Controls;
using System;
using System.Numerics;

namespace SoftRenderingApp3D.Projection
{
    public class FovPerspectiveProjection : IProjection
    {
        private float fOV;
        private float zFar;
        private float zNear;

        public FovPerspectiveProjection(float fOV, float zNear, float zFar)
        {
            this.zNear = zNear;
            this.zFar = zFar;
            this.fOV = fOV;
        }

        public float FOV
        {
            get
            {
                return fOV;
            }

            set
            {
                if(!value.TryUpdateOther(ref fOV))
                    return;

                ProjectionChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public float ZNear
        {
            get
            {
                return zNear;
            }

            set
            {
                if(!value.TryUpdateOther(ref zNear))
                    return;

                ProjectionChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public float ZFar
        {
            get
            {
                return zFar;
            }

            set
            {
                if(!value.TryUpdateOther(ref zFar))
                    return;

                ProjectionChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public event EventHandler ProjectionChanged;

        public Matrix4x4 ProjectionMatrix(float width, float height)
        {
            return Matrix4x4.CreatePerspectiveFieldOfView(fOV, width / height, zNear, zFar);
        }
    }
}
