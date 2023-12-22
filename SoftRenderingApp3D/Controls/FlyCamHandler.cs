using SoftRenderingApp3D.Camera;
using System;
using System.Drawing;
using System.Numerics;
using System.Windows.Forms;

namespace SoftRenderingApp3D.Controls {
    // Buggy

    public class FlyCamHandler {
        private FlyCam camera;

        private Control control;
        private bool down;
        private bool left;

        private bool leftB;

        private Point mouse;
        private bool right;
        private bool rightB;

        private bool up;

        public FlyCamHandler(Control control, FlyCam camera) {
            Control = control;
            Camera = camera;
        }

        public Control Control {
            get {
                return control;
            }
            set {
                var oldControl = control;

                if(PropertyChangedHelper.ChangeValue(ref control, value)) {
                    if(oldControl != null) {
                        oldControl.MouseDown -= control_MouseDown;
                        oldControl.MouseMove -= control_MouseMove;
                        oldControl.MouseUp -= Control_MouseUp;
                        control.MouseWheel -= Control_MouseWheel;
                        oldControl.MouseEnter -= Control_MouseEnter;
                        oldControl.MouseLeave -= Control_MouseLeave;
                    }

                    if(control != null) {
                        control.MouseDown += control_MouseDown;
                        control.MouseMove += control_MouseMove;
                        control.MouseUp += Control_MouseUp;
                        control.MouseWheel += Control_MouseWheel;
                        control.MouseEnter += Control_MouseEnter;
                        control.MouseLeave += Control_MouseLeave;
                    }
                }
            }
        }

        public FlyCam Camera {
            get {
                return camera;
            }
            set {
                PropertyChangedHelper.ChangeValue(ref camera, value);
            }
        }

        private void Control_MouseWheel(object sender, MouseEventArgs e) {
            const float c = 1f;

            if(e.Delta > 0) {
                move(0, 0, 1f * c);
            }
            else {
                move(0, 0, -1f * c);
            }
        }

        private void Control_MouseLeave(object sender, EventArgs e) {
            var frm = control;
            frm.KeyDown -= Frm_KeyDown;
            frm.KeyUp -= Frm_KeyUp;
        }

        private void Control_MouseEnter(object sender, EventArgs e) {
            var frm = control;
            frm.KeyDown += Frm_KeyDown;
            frm.KeyUp += Frm_KeyUp;
        }

        private void Frm_KeyUp(object sender, KeyEventArgs e) {
            handleKeyCode(e, false);
            handleMove();
        }

        private void Frm_KeyDown(object sender, KeyEventArgs e) {
            handleKeyCode(e, true);
            handleMove();
        }

        private void handleMove() {
            if(up) {
                move(0, 0, 1);
            }
            else if(down) {
                move(0, 0, -1);
            }

            if(left) {
                move(1, 0, 0);
            }
            else if(right) {
                move(-1, 0, 0);
            }
        }

        private void move(float dx, float dy, float dz) {
            camera.Position += Vector3.Transform(new Vector3(dx, dy, dz), camera.Rotation);
        }

        private void rotate(float p, float y, float r) {
            camera.Rotation = Quaternion.CreateFromYawPitchRoll(y, p, r) * camera.Rotation;
        }

        private void handleKeyCode(KeyEventArgs e, bool status) {
            switch(e.KeyCode) {
                case Keys.Q: // Qwerty lovers are my friends...
                case Keys.A: // ...but Azerty rules
                    left = status;
                    break;
                case Keys.D:
                    right = status;
                    break;
                case Keys.W:
                case Keys.Z: // :p
                    up = status;
                    break;
                case Keys.S:
                    down = status;
                    break;
            }
        }

        private void Control_MouseUp(object sender, MouseEventArgs e) {
            ControlHelper.getMouseButtons(out leftB, out rightB);
        }

        private void control_MouseDown(object sender, MouseEventArgs e) {
            ControlHelper.getMouseButtons(out leftB, out rightB);
            mouse = e.Location;
        }

        private void control_MouseMove(object sender, MouseEventArgs e) {
            if(rightB) {
                const float c = .01f;
                var delta = Point.Subtract(mouse, (Size)e.Location);
                move(-delta.X * c, delta.Y * c, 0);
                mouse = e.Location;
            }

            if(leftB) {
                const float c = .01f;
                var delta = Point.Subtract(mouse, (Size)e.Location);
                rotate(-delta.Y * c, delta.X * c, 0);
                mouse = e.Location;
            }
        }
    }
}