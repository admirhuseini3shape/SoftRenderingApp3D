using SoftRenderingApp3D.App.DataStructures;
using SoftRenderingApp3D.App.Utils;
using SoftRenderingApp3D.Camera;
using SoftRenderingApp3D.Controls;
using SoftRenderingApp3D.DataStructures.World;
using SoftRenderingApp3D.Projection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Windows.Forms;

namespace SoftRenderingApp3D.App
{

    public partial class SoftRenderingForm : Form
    {
        private Panel3D panel3D1;
        private readonly ArcBallCam arcBallCam;
        private FlyCam flyCam;
        private readonly List<DisplayModelData> displayModels;

        public SoftRenderingForm()
        {

            InitializeComponent();
            Initialize3DPanel();
            displayModels = JsonHelpers.GetDisplayModelsFromJson();
            PopulateLstDemos(displayModels);

            lstDemos.DoubleClick += LstDemos_DoubleClick;

            chkShowTexture.Checked = panel3D1.RendererSettings.ShowTextures;
            chkLinearFiltering.Checked = panel3D1.RendererSettings.LiearTextureFiltering;

            chkShowTexture.CheckedChanged += ChkShowTextureOnCheckedChanged;
            chkLinearFiltering.CheckedChanged += ChkLinearFilteringOnCheckedChanged;

            btnBench.Click += BtnBenchOnClick;

            var projection = new FovPerspectiveProjection(40f * (float)Math.PI / 180f, .01f, 500f);

            arcBallCam = new ArcBallCam { Position = new Vector3(0, 0, -25) };
            flyCam = new FlyCam { Position = new Vector3(0, 0, -25) };

            var arcBallCamHandler = new ArcBallCamHandler(panel3D1, arcBallCam);

            arcBallCamControl1.Camera = arcBallCam;

            panel3D1.Projection = projection;
            panel3D1.Camera = arcBallCam;

            LstDemos_DoubleClick(this, null);
        }

        private void Initialize3DPanel()
        {
            this.panel3D1 = new SoftRenderingApp3D.App.Panel3D();
            this.groupBox6.Controls.Add(this.panel3D1);
            // 
            // panel3D1
            // 
            this.panel3D1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.panel3D1.BackColor = System.Drawing.Color.WhiteSmoke;
            this.panel3D1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.panel3D1.Location = new System.Drawing.Point(11, 16);
            this.panel3D1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.panel3D1.Name = "panel3D1";
            this.panel3D1.Size = new System.Drawing.Size(716, 566);
            this.panel3D1.TabIndex = 0;
        }

        private void BtnBenchOnClick(object s, EventArgs e)
        {
            var sw = Stopwatch.StartNew();
            arcBallCam.Position = new Vector3(0, 0, -5);
            arcBallCam.Rotation = Quaternion.Identity;
            for(var i = 0; i < 10; i++)
            {
                arcBallCam.Rotation *= Quaternion.CreateFromYawPitchRoll(.1f, .1f, .1f);
                panel3D1.Render();
            }

            sw.Stop();
            lblSw.Text = sw.ElapsedMilliseconds.ToString();
        }

        private void ChkLinearFilteringOnCheckedChanged(object s, EventArgs e)
        {
            panel3D1.RendererSettings.LiearTextureFiltering = chkLinearFiltering.Checked;
            panel3D1.Invalidate();
        }

        private void ChkShowTextureOnCheckedChanged(object s, EventArgs e)
        {
            panel3D1.RendererSettings.ShowTextures = chkShowTexture.Checked;
            panel3D1.Invalidate();
        }

        private void PopulateLstDemos(IEnumerable<DisplayModelData> data)
        {
            var dataSource = data
                .Select(x => new { x.Id, x.DisplayName })
                .ToList();
            lstDemos.DataSource = dataSource;
            lstDemos.ValueMember = nameof(DisplayModelData.Id);
            lstDemos.DisplayMember = nameof(DisplayModelData.DisplayName);
        }

        private void LstDemos_DoubleClick(object sender, EventArgs e)
        {
            var id = lstDemos.SelectedValue as string;
            var currentModel = displayModels.FirstOrDefault(x => x.Id == id);
            if(currentModel == null)
                return;

            var generatedWorld = DisplayModelHelpers.GenerateWorld(currentModel);
            if(currentModel.InitialZoomLevel != 0)
            {
                var zoom = currentModel.InitialZoomLevel;
                var cameraPositionDelta = new Vector3(0, 0, zoom - arcBallCam.Position.Z);
                arcBallCam.Position += cameraPositionDelta;
            }
            generatedWorld.RaisePropertyChanged();
            chkShowTexture.Enabled = currentModel.HasTexture;
            panel3D1.RendererSettings.ShowTextures = currentModel.ShowTexture;
            chkShowTexture.Checked = currentModel.ShowTexture;

            PrepareWorld(generatedWorld);
        }

        private void PrepareWorld(IWorld world)
        {

            world.LightSources.Add(new LightSource { Position = new Vector3(0, 0, 10) });

            arcBallCam.CameraChanged -= MainCam_CameraChanged;
            arcBallCam.CameraChanged += MainCam_CameraChanged;

            panel3D1.World = world;

            panel3D1.Invalidate();

            void MainCam_CameraChanged(object cam, EventArgs _1)
            {
                // camObject.Position = ((ArcBallCam)cam).Position;
                // this.panel3D2.Invalidate();
            }
        }

        private void btnChangeTexture_Click(object sender, EventArgs e)
        {
            panel3D1.RendererSettings.ChangeActiveTexture();
        }
    }
}
