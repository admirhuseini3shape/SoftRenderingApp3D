using SoftRenderingApp3D.Camera;
using System;
using System.Drawing;
using System.Numerics;
using System.Windows.Forms;

namespace SoftRenderingApp3D.Controls {
    // Must adapt move

    public partial class ArcBallCamControl : UserControl {
        private ArcBallCam camera;
        private Quaternion curRotation;
        private ArcBallCamHandler handler;

        public ArcBallCamControl() {
            InitializeComponent();

            slider1.ValueChanged += (s, e) =>
                camera.Position = new Vector3(camera.Position.X, camera.Position.Y, -slider1.Value);
            panel1.Paint += Panel1_Paint;
        }

        public ArcBallCam Camera {
            get {
                return camera;
            }
            set {
                var oldCamera = camera;

                if(PropertyChangedHelper.ChangeValue(ref camera, value)) {
                    if(oldCamera != null) {
                        oldCamera.CameraChanged -= Camera_CameraChanged;
                        handler = null;
                    }

                    if(camera != null) {
                        camera.CameraChanged += Camera_CameraChanged;
                        handler = new ArcBallCamHandler(panel1, Camera);
                    }

                    slider1.Value = -camera.Position.Z;
                }
            }
        }

        private void Panel1_Paint(object sender, PaintEventArgs e) {
            var g = e.Graphics;
            drawGlobe(g);
            drawZIndex(g);
        }

        private void drawZIndex(Graphics g) {
        }

        private void drawGlobe(Graphics g) {
            var nR = camera?.Radius ?? 0f;

            var rectF = new RectangleF(
                (.5f - nR) * panel1.Width,
                (.5f - nR) * panel1.Height,
                nR * 2f * panel1.Width, // width
                nR * 2f * panel1.Height // height
            );

            g.DrawEllipse(Pens.LightBlue, rectF);
        }

        private void Camera_CameraChanged(object sender, EventArgs e) {
            if(-camera.Position.Z != slider1.Value) {
                slider1.Value = -camera.Position.Z;
            }

            if(camera.Rotation != curRotation) {
                curRotation = camera.Rotation;
                panel1.Invalidate();
            }
        }
    }
}