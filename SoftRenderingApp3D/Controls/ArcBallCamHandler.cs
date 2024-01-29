using SoftRenderingApp3D.Camera;
using SoftRenderingApp3D.Utils;
using System;
using System.Drawing;
using System.Numerics;
using System.Windows.Forms;

namespace SoftRenderingApp3D.Controls
{
    // Must adapt moving

     public class ArcBallCamHandler : MouseControlBase
    {
        private ArcBallCam Camera;
        private Quaternion OldCameraRotation;
        private Vector3 OldCameraPosition;
        private Point OldMousePosition;
        private readonly float YCoeff = 10f;
        private bool Left, Right, Middle;

        public event EventHandler<Point> MouseClicked;

        
        public ArcBallCamHandler(Control control, ArcBallCam camera) : base(control)
        {
            Camera = camera;
        }

        protected override void Control_MouseDown(object sender, MouseEventArgs e)
        {
            base.Control_MouseDown(sender, e); // Call base to raise MouseClicked event

            GetMouseButtons(out Left, out Right, out Middle);
            OldMousePosition = e.Location;

            if (Left && Right)
            {
                OldCameraPosition = Camera.Position;
                Control.Cursor = Cursors.SizeNS;
            }
            else if (Left)
            {
                OldCameraRotation = Camera.Rotation;
                Control.Cursor = Cursors.NoMove2D;
            }
            else if (Right || Middle)
            {
                OldCameraPosition = Camera.Position;
                Control.Cursor = Cursors.SizeAll;
            }
        }

        protected override void Control_MouseMove(object sender, MouseEventArgs e)
        {
            if (Left && Right)
            {
                var deltaY = OldMousePosition.Y - e.Location.Y;
                Camera.Position = OldCameraPosition + new Vector3(0, 0, deltaY / YCoeff);
            }
            else if (Left)
            {
                var oldNpc = NormalizePointClient(OldMousePosition);
                var oldVector = Camera.MapToSphere(oldNpc);

                var curNpc = NormalizePointClient(e.Location);
                var curVector = Camera.MapToSphere(curNpc);

                var deltaRotation = Camera.CalculateQuaternion(oldVector, curVector);
                Camera.Rotation = deltaRotation * OldCameraRotation;
            }
            else if (Right || Middle)
            {
                var deltaPosition = new Vector3(e.Location.ToVector2() - OldMousePosition.ToVector2(), 0);
                Camera.Position = OldCameraPosition + deltaPosition * new Vector3(1, -1, 1) / 100;
            }
        }

        protected override void Control_MouseUp(object sender, MouseEventArgs e)
        {
            Left = Right = Middle = false;
            Control.Cursor = Cursors.Default;
        }

        protected override void Control_MouseWheel(object sender, MouseEventArgs e)
        {
            var deltaY = 0.1 * e.Delta;
            OldCameraPosition = Camera.Position;
            Camera.Position = OldCameraPosition + new Vector3(0, 0, (float)deltaY / YCoeff);
        }

        private Vector2 NormalizePointClient(Point point)
        {
            return new Vector2(
                2.0f * (point.X / (float)Control.Width) - 1.0f,
                2.0f * (point.Y / (float)Control.Height) - 1.0f);
        }

        private void GetMouseButtons(out bool left, out bool right, out bool middle)
        {
            left = Control.MouseButtons.HasFlag(MouseButtons.Left);
            right = Control.MouseButtons.HasFlag(MouseButtons.Right);
            middle = Control.MouseButtons.HasFlag(MouseButtons.Middle);
        }

        public override void OnMouseClicked(Point point)
        {
            MouseClicked?.Invoke(this, point);
        }
    }
}

