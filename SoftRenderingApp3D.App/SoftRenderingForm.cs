using SoftRenderingApp3D.Camera;
using SoftRenderingApp3D.Controls;
using SoftRenderingApp3D.DataStructures;
using SoftRenderingApp3D.Painter;
using SoftRenderingApp3D.Renderer;
using System;
using System.Diagnostics;
using System.Numerics;
using System.Windows.Forms;

namespace SoftRenderingApp3D.App {

    public partial class SoftRenderingForm : Form {

        ArcBallCam arcBallCam;
        FlyCam flyCam;

        public SoftRenderingForm() {
            InitializeComponent();

            // var v = VolumeFactory.NewImportCollada("Models\\skull.dae").ToList();

            lstDemos.DataSource = new[] {
                new { display = "Crane", id = "skull" },
                new { display = "Teapot", id = "teapot" },
                new { display = "Cubes", id = "cubes" },
                new { display = "Spheres", id = "spheres" },
                new { display = "Little town", id = "littletown" },
                new { display = "Town", id = "town" },
                new { display = "Big town", id = "bigtown" },
                new { display = "Cube", id = "cube" },
                new { display = "Big cube", id = "bigcube" },
                new { display = "Empty", id = "empty" },
                new { display = "Planetary Toy STL", id = "stl-mesh-1"},
                new { display = "Star Destroyer STL", id = "stl-mesh-2"},
                new { display = "Jaw", id = "jaw"}
            };

            lstDemos.ValueMember = "id";
            lstDemos.DisplayMember = "display";

            lstDemos.DoubleClick += LstDemos_DoubleClick;

            chkShowTexture.Checked = panel3D1.RendererSettings.ShowTextures;
            chkLinearFiltering.Checked = panel3D1.RendererSettings.LiearTextureFiltering;

            chkShowTexture.CheckedChanged += (s, e) => { panel3D1.RendererSettings.ShowTextures = chkShowTexture.Checked; panel3D1.Invalidate(); };
            chkLinearFiltering.CheckedChanged += (s, e) => { panel3D1.RendererSettings.LiearTextureFiltering = chkLinearFiltering.Checked; panel3D1.Invalidate(); };

            btnBench.Click += (s, e) => {
                var sw = Stopwatch.StartNew();
                arcBallCam.Position = new Vector3(0, 0, -5);
                arcBallCam.Rotation = Quaternion.Identity;
                for(var i = 0; i < 10; i++) {
                    arcBallCam.Rotation *= Quaternion.CreateFromYawPitchRoll(.1f, .1f, .1f);
                    this.panel3D1.Render();
                }
                sw.Stop();
                lblSw.Text = sw.ElapsedMilliseconds.ToString();
            };

            var projection = new FovPerspectiveProjection(40f * (float)Math.PI / 180f, .01f, 500f);

            arcBallCam = new ArcBallCam { Position = new Vector3(0, 0, -25) };
            flyCam = new FlyCam { Position = new Vector3(0, 0, -25) };

            var arcBallCamHandler = new ArcBallCamHandler(this.panel3D1, arcBallCam);

            this.arcBallCamControl1.Camera = arcBallCam;

            this.panel3D1.Projection = projection;
            this.panel3D1.Camera = arcBallCam;


            prepareWorld("skull");
        }

        private void LstDemos_DoubleClick(object sender, EventArgs e) {
            prepareWorld(lstDemos.SelectedValue as string);
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
                    panel3D1.RendererSettings.ShowTextures = false;
                    chkShowTexture.Enabled = false;
                    chkShowTexture.Checked = false;
                    world.Volumes.AddRange(stlReader.ReadFile(@"models\original.stl"));
                    world.Volumes.AddRange(stlReader.ReadFile(@"models\offset_2.stl"));
                    arcBallCam.Position += new Vector3(0, 0, -5 - arcBallCam.Position.Z);
                    break;
                case "stl-mesh-1":
                    panel3D1.RendererSettings.ShowTextures = false;
                    chkShowTexture.Enabled = false;
                    chkShowTexture.Checked = false;
                    world.Volumes.AddRange(stlReader.ReadFile(@"models\Planetary_Toy_D80.stl"));
                    arcBallCam.Position += new Vector3(0, 0, -5 - arcBallCam.Position.Z);
                    break;
                case "stl-mesh-2":
                    panel3D1.RendererSettings.ShowTextures = false;
                    chkShowTexture.Enabled = false;
                    chkShowTexture.Checked = false;
                    world.Volumes.AddRange(stlReader.ReadFile(@"models\Star_Destroyer_Fixed.stl"));
                    arcBallCam.Position += new Vector3(0, 0, -5 - arcBallCam.Position.Z);
                    break;
                case "skull":
                    panel3D1.RendererSettings.ShowTextures = true;
                    chkShowTexture.Enabled = true;
                    chkShowTexture.Checked = true;
                    world.Volumes.AddRange(colladaReader.ReadFile(@"models\skull.dae"));
                    arcBallCam.Position += new Vector3(0, 0, -5 - arcBallCam.Position.Z);
                    break;

                case "teapot":
                    panel3D1.RendererSettings.ShowTextures = false;
                    chkShowTexture.Enabled = false;
                    chkShowTexture.Checked = false;
                    world.Volumes.AddRange(colladaReader.ReadFile(@"models\teapot.dae"));
                    break;

                case "empty":
                    panel3D1.RendererSettings.ShowTextures = false;
                    chkShowTexture.Enabled = false;
                    chkShowTexture.Checked = false;
                    break;

                case "town": {
                    panel3D1.RendererSettings.ShowTextures = false;
                    chkShowTexture.Enabled = false;
                    chkShowTexture.Checked = false;
                        var d = 50; var s = 2;
                        for(var x = -d; x <= d; x += s)
                            for(var z = -d; z <= d; z += s) {
                                world.Volumes.Add(
                                    new Cube() {
                                        Position = new Vector3(x, 0, z),
                                        //Scale = new Vector3(1, r.Next(1, 50), 1)
                                    });
                            }
                        break;
                    }

                case "littletown": {
                    panel3D1.RendererSettings.ShowTextures = false;
                    chkShowTexture.Enabled = false;
                    chkShowTexture.Checked = false;
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
                    panel3D1.RendererSettings.ShowTextures = false;
                    chkShowTexture.Enabled = false;
                    chkShowTexture.Checked = false;
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
                    panel3D1.RendererSettings.ShowTextures = false;
                    chkShowTexture.Enabled = false;
                    chkShowTexture.Checked = false;
                    world.Volumes.Add(new Cube());
                    break;

                case "bigcube":
                    panel3D1.RendererSettings.ShowTextures = false;
                    chkShowTexture.Enabled = false;
                    chkShowTexture.Checked = false;
                    world.Volumes.Add(new Cube() { Scale = new Vector3(100, 100, 100) });
                    break;

                case "spheres": {
                    panel3D1.RendererSettings.ShowTextures = false;
                    chkShowTexture.Enabled = false;
                    chkShowTexture.Checked = false;
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
                    panel3D1.RendererSettings.ShowTextures = false;
                    chkShowTexture.Enabled = false;
                    chkShowTexture.Checked = false;
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

            // var camObject = new Cube() { Position = arcBallCam.Position };

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
    }
}