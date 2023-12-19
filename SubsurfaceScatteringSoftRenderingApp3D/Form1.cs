using SoftRenderingApp3D;
using SoftRenderingApp3D.Camera;
using SoftRenderingApp3D.Controls;
using SoftRenderingApp3D.DataStructures;
using SubsurfaceScatteringLibrary.Renderer;
using System;
using System.Numerics;
using System.Windows.Forms;

namespace SubsurfaceScatteringSoftRenderingApp3D {
    public partial class Form1 : Form {
        ArcBallCam arcBallCam;
        FlyCam flyCam;
        public Form1() {
            InitializeComponent();
            var projection = new FovPerspectiveProjection(40f * (float)Math.PI / 180f, .01f, 500f);
            arcBallCam = new ArcBallCam { Position = new Vector3(0, 0, -25) };
            flyCam = new FlyCam { Position = new Vector3(0, 0, -25) };
            var arcBallCamHandler = new ArcBallCamHandler(this.panel3D1, arcBallCam);
            this.arcBallCamControl1.Camera = arcBallCam;
            this.panel3D1.Projection = projection;
            this.panel3D1.Camera = arcBallCam;
            this.trackBar1.Value = 50;
            this.trackBar2.Value = 40;
            this.trackBar5.Value = 10;
            this.trackBar6.Value = 50;
            this.trackBar3.Value = 100;
            this.trackBar4.Value = 100;

            this.checkBox1.Checked = false;
            this.checkBox2.Checked = false;

            PrepareWorld("jaw");
        }
        void PrepareWorld(string id) {
            var world = new World();
            STLReader stlReader = new STLReader();

            switch(id) {
                case "jaw":
                    world.Volumes.AddRange(stlReader.ReadFile(@"models\original.stl"));
                    // Add a cube that represents the light
                    world.Volumes.AddRange(stlReader.ReadFile(@"models\offset_3.stl"));
                    world.Volumes.AddRange(stlReader.ReadFile(@"models\caries.stl"));
                    (world.Volumes[1] as Volume).InitializeTrianglesColor(ColorRGB.Black);
                    (world.Volumes[2] as Volume).InitializeTrianglesColor(ColorRGB.Black);
                    arcBallCam.Position += new Vector3(0, 10, -50 - arcBallCam.Position.Z);
                    break;
                default:
                    break;
            }

            world.LightSources.Add(new LightSource { Position = new Vector3(0, 0, 10) });
            arcBallCam.CameraChanged -= MainCam_CameraChanged;
            arcBallCam.CameraChanged += MainCam_CameraChanged;
            // world.Volumes.Add(camObject);
            this.panel3D1.World = world;
            this.panel3D1.Invalidate();
            void MainCam_CameraChanged(object cam, EventArgs _1) {
                // camObject.Position = ((ArcBallCam)cam).Position;
                // this.panel3D2.Invalidate();
            }
        }
        private void btnChangeTexture_Click(object sender, EventArgs e) {
            this.panel3D1.RendererSettings.ChangeActiveTexture();
        }
        private void label1_Click(object sender, EventArgs e) {
        }
        private void trackBar1_Scroll(object sender, EventArgs e) {
            SubsurfaceScatteringRenderUtils.ChangeVisibility(this.trackBar1.Value);
            this.panel3D1.Invalidate();
        }
        private void trackBar2_Scroll(object sender, EventArgs e) {
            SubsurfaceScatteringRenderUtils.ChangeSubsurfaceScatteringStrength(this.trackBar2.Value);
            this.panel3D1.Invalidate();
        }
        private void trackBar4_Scroll(object sender, EventArgs e) {
            SubsurfaceScatteringRenderUtils.ChangeSurfaceColor(this.trackBar4.Value);
            this.panel3D1.Invalidate();
        }
        private void trackBar3_Scroll(object sender, EventArgs e) {
            SubsurfaceScatteringRenderUtils.ChangeSubsurfaceColor(this.trackBar3.Value);
            this.panel3D1.Invalidate();
        }
        private void trackBar5_Scroll(object sender, EventArgs e) {
            SubsurfaceScatteringRenderUtils.ChangeSubsurfaceDecay(this.trackBar5.Value);
            this.panel3D1.Invalidate();
        }
        private void trackBar6_Scroll(object sender, EventArgs e) {
            SubsurfaceScatteringRenderUtils.ChangeGaussianBlurStDev(trackBar6.Value);
            this.panel3D1.Invalidate();
        }
        private void checkBox1_CheckedChanged(object sender, EventArgs e) {
            SubsurfaceScatteringRenderUtils.ToggleGaussianBlur();
            this.trackBar6.Enabled = RenderUtils.GaussianBlur;
            this.panel3D1.Invalidate();
        }
        private void checkBox2_CheckedChanged(object sender, EventArgs e) {
            SubsurfaceScatteringRenderUtils.ToggleOnlySubsurfaceBlur();
            this.panel3D1.Invalidate();
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e) {
            SubsurfaceScatteringRenderUtils.ToggleCaries();
            this.panel3D1.Invalidate();
        }
    }
}