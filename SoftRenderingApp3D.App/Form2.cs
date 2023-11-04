using System;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Windows.Forms;

namespace SoftRenderingApp3D.App {

    public partial class Form2 : Form {

        ArcBallCam arcBallCam;
        FlyCam flyCam;

        public Form2() {
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

            prepareWorld("jaw");
        }

        void prepareWorld(string id) {
            var world = new World();

            ColladaReader colladaReader = new ColladaReader();
            STLReader stlReader = new STLReader();
            ITextureReader textureReader = new TextureReaderBMP();
            world.Textures.Add(textureReader.ReadImage(@"textures\bone.bmp"));
            world.Textures.Add(textureReader.ReadImage(@"textures\glass_effect.bmp"));
            world.Textures.Add(textureReader.ReadImage(@"textures\bone_high.bmp"));


            switch(id) {
                case "jaw":
                    world.Volumes.AddRange(stlReader.ReadFile(@"models\original.stl"));
                    // Add a cube that represents the light
                    world.Volumes.AddRange(stlReader.ReadFile(@"models\offset_3.stl"));
                    (world.Volumes[1] as Volume).InitializeTrianglesColor(ColorRGB.Black);
                    arcBallCam.Position += new Vector3(0, 10, -50 - arcBallCam.Position.Z);
                    break;
                case "stl-mesh-1":
                    world.Volumes.AddRange(stlReader.ReadFile(@"models\Planetary_Toy_D80.stl"));
                    arcBallCam.Position += new Vector3(0, 0, -5 - arcBallCam.Position.Z);
                    break;
                case "stl-mesh-2":
                    world.Volumes.AddRange(stlReader.ReadFile(@"models\Star_Destroyer_Fixed.stl"));
                    arcBallCam.Position += new Vector3(0, 0, -5 - arcBallCam.Position.Z);
                    break;
                case "skull":
                    world.Volumes.AddRange(colladaReader.ReadFile(@"models\skull.dae"));
                    arcBallCam.Position += new Vector3(0, 0, -5 - arcBallCam.Position.Z);
                    break;

                case "teapot":
                    world.Volumes.AddRange(colladaReader.ReadFile(@"models\teapot.dae"));
                    break;

                case "empty":
                    break;

                case "town": {
                        var d = 50; var s = 2;
                        for(var x = -d; x <= d; x += s)
                            for(var z = -d; z <= d; z += s) {
                                world.Volumes.Add(
                                    new Cube() {
                                        Position = new Vector3(x, 0, z),
                                        // Scale = new Vector3(1, r.Next(1, 50), 1)
                                    });
                            }
                        break;
                    }

                case "littletown": {
                        var d = 10; var s = 2;
                        for(var x = -d; x <= d; x += s)
                            for(var z = -d; z <= d; z += s) {
                                world.Volumes.Add(
                                    new Cube() {
                                        Position = new Vector3(x, 0, z),
                                        // Scale = new Vector3(1, r.Next(1, 50), 1)
                                    });
                            }
                        break;
                    }

                case "bigtown": {
                        var d = 200; var s = 2;
                        for(var x = -d; x <= d; x += s)
                            for(var z = -d; z <= d; z += s) {
                                world.Volumes.Add(
                                    new Cube() {
                                        Position = new Vector3(x, 0, z),
                                        // Scale = new Vector3(1, r.Next(1, 50), 1)
                                    });
                            }
                        break;
                    }

                case "cube":
                    world.Volumes.Add(new Cube());
                    break;

                case "bigcube":
                    world.Volumes.Add(new Cube() { Scale = new Vector3(100, 100, 100) });
                    break;

                case "spheres": {
                        var d = 5; var s = 2; var r = new Random();
                        for(var x = -d; x <= d; x += s)
                            for(var y = -d; y <= d; y += s)
                                for(var z = -d; z <= d; z += s) {
                                    world.Volumes.Add(
                                        new IcoSphere(2) {
                                            Position = new System.Numerics.Vector3(x, y, z),
                                            Rotation = new Rotation3D(
                                                (float)r.Next(-90, 90),
                                                (float)r.Next(-90, 90),
                                                (float)r.Next(-90, 90)).ToRad()
                                        });
                                }
                        break;
                    }

                case "cubes": {
                        var d = 20; var s = 2; var r = new Random();
                        for(var x = -d; x <= d; x += s)
                            for(var y = -d; y <= d; y += s)
                                for(var z = -d; z <= d; z += s) {
                                    world.Volumes.Add(
                                        new Cube() {
                                            Position = new System.Numerics.Vector3(x, y, z),
                                            Rotation = new Rotation3D(
                                                (float)r.Next(-90, 90),
                                                (float)r.Next(-90, 90),
                                                (float)r.Next(-90, 90)).ToRad()
                                        });
                                }
                        break;
                    }
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
            this.panel3D1.RendererSettings.changeActiveTexture();
        }

        private void label1_Click(object sender, EventArgs e) {

        }

        private void trackBar1_Scroll(object sender, EventArgs e) {
            RenderUtils.ChangeVisibility(this.trackBar1.Value);
            this.panel3D1.Invalidate();
        }

        private void trackBar2_Scroll(object sender, EventArgs e) {
            RenderUtils.ChangeSubsurfaceScatteringStrength(this.trackBar2.Value);
            this.panel3D1.Invalidate();
        }

        private void trackBar4_Scroll(object sender, EventArgs e) {
            RenderUtils.ChangeSurfaceColor(this.trackBar4.Value);
            this.panel3D1.Invalidate();
        }

        private void trackBar3_Scroll(object sender, EventArgs e) {
            RenderUtils.ChangeSubsurfaceColor(this.trackBar3.Value);
            this.panel3D1.Invalidate();

        }

        private void trackBar5_Scroll(object sender, EventArgs e) {
            RenderUtils.ChangeSubsurfaceDecay(this.trackBar5.Value);
            this.panel3D1.Invalidate();

        }

        private void trackBar6_Scroll(object sender, EventArgs e) {
            RenderUtils.ChangeGaussianBlurStDev(trackBar6.Value);
            this.panel3D1.Invalidate();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e) {
            RenderUtils.ToggleGaussianBlur();
            this.trackBar6.Enabled = RenderUtils.GaussianBlur;
            this.panel3D1.Invalidate();
        }
    }
}