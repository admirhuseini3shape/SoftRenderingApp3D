using SoftRenderingApp3D.Camera;
using SoftRenderingApp3D.Controls;
using SoftRenderingApp3D.DataStructures.World;
using SoftRenderingApp3D.Projection;
using System;
using System.Diagnostics;
using System.Numerics;
using System.Windows.Forms;

namespace SoftrenderingApp3D.App {

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


            FormMethods.FormMethods prepareWorld = new FormMethods.FormMethods();
            var world = prepareWorld.prepareWorld( id , arcBallCam, chkShowTexture, panel3D1);

            

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