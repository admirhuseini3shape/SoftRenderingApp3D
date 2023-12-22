using SoftRenderingApp3D.App.DataStructures;
using SoftRenderingApp3D.Camera;
using SoftRenderingApp3D.Controls;
using SoftRenderingApp3D.DataStructures.World;
using SoftRenderingApp3D.Projection;
using System;
using System.Diagnostics;
using System.Numerics;
using System.Windows.Forms;

namespace SoftRenderingApp3D.App {
    public partial class SoftRenderingForm : Form {
        private readonly ArcBallCam arcBallCam;
        private FlyCam flyCam;

        public SoftRenderingForm() {
            InitializeComponent();

            // var v = VolumeFactory.NewImportCollada("Models\\skull.dae").ToList();

            lstDemos.DataSource = DisplayModelData.Models;
            lstDemos.ValueMember = nameof(DisplayModelData.Id);
            lstDemos.DisplayMember = nameof(DisplayModelData.Display);

            lstDemos.DoubleClick += LstDemos_DoubleClick;

            chkShowTexture.Checked = panel3D1.RendererSettings.ShowTextures;
            chkLinearFiltering.Checked = panel3D1.RendererSettings.LiearTextureFiltering;

            chkShowTexture.CheckedChanged += (s, e) => {
                panel3D1.RendererSettings.ShowTextures = chkShowTexture.Checked;
                panel3D1.Invalidate();
            };
            chkLinearFiltering.CheckedChanged += (s, e) => {
                panel3D1.RendererSettings.LiearTextureFiltering = chkLinearFiltering.Checked;
                panel3D1.Invalidate();
            };

            btnBench.Click += (s, e) => {
                var sw = Stopwatch.StartNew();
                arcBallCam.Position = new Vector3(0, 0, -5);
                arcBallCam.Rotation = Quaternion.Identity;
                for(var i = 0; i < 10; i++) {
                    arcBallCam.Rotation *= Quaternion.CreateFromYawPitchRoll(.1f, .1f, .1f);
                    panel3D1.Render();
                }

                sw.Stop();
                lblSw.Text = sw.ElapsedMilliseconds.ToString();
            };

            var projection = new FovPerspectiveProjection(40f * (float)Math.PI / 180f, .01f, 500f);

            arcBallCam = new ArcBallCam { Position = new Vector3(0, 0, -25) };
            flyCam = new FlyCam { Position = new Vector3(0, 0, -25) };

            var arcBallCamHandler = new ArcBallCamHandler(panel3D1, arcBallCam);

            arcBallCamControl1.Camera = arcBallCam;

            panel3D1.Projection = projection;
            panel3D1.Camera = arcBallCam;

            // Deserialize DisplayModelData from a JSON file
            prepareWorld("skull");
        }

        private void LstDemos_DoubleClick(object sender, EventArgs e) {
             var id =lstDemos.SelectedValue as string; 
            //var data  = GetDisplayModelData(id);
            // PrepareWorld(data);
            // PrepareUI(data);
            prepareWorld(id);
        }

        private void prepareWorld(string id) {
            var prepareWorld = new FormMethods.FormMethods();
            var world = prepareWorld.prepareWorld(id, arcBallCam, chkShowTexture, panel3D1);


            world.LightSources.Add(new LightSource { Position = new Vector3(0, 0, 10) });

            // var camObject = new Cube() { Position = arcBallCam.Position };

            arcBallCam.CameraChanged -= MainCam_CameraChanged;
            arcBallCam.CameraChanged += MainCam_CameraChanged;

            // world.Volumes.Add(camObject);


            panel3D1.World = world;

            panel3D1.Invalidate();

            void MainCam_CameraChanged(object cam, EventArgs _1) {
                // camObject.Position = ((ArcBallCam)cam).Position;
                // this.panel3D2.Invalidate();
            }
        }

        private void btnChangeTexture_Click(object sender, EventArgs e) {
            panel3D1.RendererSettings.ChangeActiveTexture();
        }
    }
}