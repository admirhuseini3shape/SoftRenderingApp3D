using SoftRenderingApp3D.Camera;
using SoftRenderingApp3D.Utils;
using System.Drawing;
using System.Numerics;
using System.Windows.Forms;

namespace SoftRenderingApp3D.Controls
{
    // Must adapt moving

    public class ArcBallCamHandler
    {
        private ArcBallCam camera;

        private Control control;

        private Vector3 oldCameraPosition;

        private Quaternion oldCameraRotation;

        private Point oldMousePosition;

        private bool left;
        private bool right;
        private bool middle;

        private readonly float yCoeff = 10f;

        public ArcBallCamHandler(Control control, ArcBallCam camera)
        {
            Control = control;
            Camera = camera;
        }

        public Control Control
        {
            get
            {
                return control;
            }
            set
            {
                var oldControl = control;

                if(!value.TryUpdateOther(ref control))
                    return;


                if(oldControl != null)
                {
                    oldControl.MouseDown -= control_MouseDown;
                    oldControl.MouseMove -= control_MouseMove;
                    control.MouseUp -= Control_MouseUp;
                    control.MouseWheel += control_MouseWheel;
                }

                if(control != null)
                {
                    control.MouseDown += control_MouseDown;
                    control.MouseMove += control_MouseMove;
                    control.MouseUp += Control_MouseUp;
                    control.MouseWheel += control_MouseWheel;
                }
            }
        }

        private void control_MouseWheel(object sender, MouseEventArgs e)
        {
            var deltaY = 0.1 * e.Delta;
            oldCameraPosition = camera.Position;
            camera.Position = oldCameraPosition + new Vector3(0, 0, (float)deltaY / yCoeff);
        }

        public ArcBallCam Camera
        {
            get
            {
                return camera;
            }
            set
            {
                value.TryUpdateOther(ref camera);
            }
        }

        private void Control_MouseUp(object sender, MouseEventArgs e)
        {
            left = false;
            right = false;
            middle = false;
            control.Cursor = Cursors.Default;
        }

        private void control_MouseDown(object sender, MouseEventArgs e)
        {
            ControlHelper.getMouseButtons(out left, out right, out middle);
            oldMousePosition = e.Location;

            if(left && right)
            {
                oldCameraPosition = camera.Position;
                control.Cursor = Cursors.SizeNS;
            }
            else if(left)
            {
                oldCameraRotation = camera.Rotation;
                control.Cursor = Cursors.NoMove2D;
            }
            else if(right || middle)
            {
                oldCameraPosition = camera.Position;
                control.Cursor = Cursors.SizeAll;
            }
        }

        private void control_MouseMove(object sender, MouseEventArgs e)
        {
            if(left && right)
            {
                var deltaY = oldMousePosition.Y - e.Location.Y;
                camera.Position = oldCameraPosition + new Vector3(0, 0, deltaY / yCoeff);
            }
            else if(left)
            {
                var oldNpc = control.NormalizePointClient(oldMousePosition);
                var oldVector = camera.MapToSphere(oldNpc);

                var curNpc = control.NormalizePointClient(e.Location);
                var curVector = camera.MapToSphere(curNpc);

                var deltaRotation = camera.CalculateQuaternion(oldVector, curVector);
                camera.Rotation = deltaRotation * oldCameraRotation;
            }
            else if(right || middle)
            {
                var deltaPosition = new Vector3(e.Location.ToVector2() - oldMousePosition.ToVector2(), 0);
                camera.Position = oldCameraPosition + deltaPosition * new Vector3(1, -1, 1) / 100;
            }
        }
    }
}
