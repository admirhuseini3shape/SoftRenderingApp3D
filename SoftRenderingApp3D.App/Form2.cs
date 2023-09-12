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

            rdbNoneShading.Checked = panel3D1.Painter == null;
            rdbClassicShading.Checked = panel3D1.Painter is ClassicPainter;
            rdbFlatShading.Checked = panel3D1.Painter is FlatPainter;
            rdbGouraudShading.Checked = panel3D1.Painter is GouraudPainter;

            rdbNoneShading.CheckedChanged += (s, e) => { if(!((RadioButton)s).Checked) return; panel3D1.Painter = null; panel3D1.Invalidate(); };
            rdbClassicShading.CheckedChanged += (s, e) => { if(!((RadioButton)s).Checked) return; panel3D1.Painter = new ClassicPainter(); panel3D1.Invalidate(); };
            rdbFlatShading.CheckedChanged += (s, e) => { if(!((RadioButton)s).Checked) return; panel3D1.Painter = new FlatPainter(); panel3D1.Invalidate(); };
            rdbGouraudShading.CheckedChanged += (s, e) => { if(!((RadioButton)s).Checked) return; panel3D1.Painter = new GouraudPainter(); panel3D1.Invalidate(); };

            rdbSimpleRendererLogic.Checked = panel3D1.Renderer is SimpleRenderer;

            rdbSimpleRendererLogic.CheckedChanged += (s, e) => { if(!((RadioButton)s).Checked) return; panel3D1.Renderer = new SimpleRenderer(); panel3D1.Invalidate(); };

            chkShowTriangles.Checked = panel3D1.RendererSettings.ShowTriangles;
            chkShowBackFacesCulling.Checked = panel3D1.RendererSettings.BackFaceCulling;
            chkShowTrianglesNormals.Checked = panel3D1.RendererSettings.ShowTriangleNormals;
            chkShowXZGrid.Checked = panel3D1.RendererSettings.ShowXZGrid;
            chkShowAxes.Checked = panel3D1.RendererSettings.ShowAxes;
            chkShowTexture.Checked = panel3D1.RendererSettings.ShowTextures;
            chkLinearFiltering.Checked = panel3D1.RendererSettings.LiearTextureFiltering;

            chkShowTriangles.CheckedChanged += (s, e) => { panel3D1.RendererSettings.ShowTriangles = chkShowTriangles.Checked; panel3D1.Invalidate(); };
            chkShowBackFacesCulling.CheckedChanged += (s, e) => { panel3D1.RendererSettings.BackFaceCulling = chkShowBackFacesCulling.Checked; panel3D1.Invalidate(); };
            chkShowTrianglesNormals.CheckedChanged += (s, e) => { panel3D1.RendererSettings.ShowTriangleNormals = chkShowTrianglesNormals.Checked; panel3D1.Invalidate(); };
            chkShowXZGrid.CheckedChanged += (s, e) => { panel3D1.RendererSettings.ShowXZGrid = chkShowXZGrid.Checked; panel3D1.Invalidate(); };
            chkShowAxes.CheckedChanged += (s, e) => { panel3D1.RendererSettings.ShowAxes = chkShowAxes.Checked; panel3D1.Invalidate(); };
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
            var flyCamHandler = new FlyCamHandler(this.panel3D2, flyCam);

            this.arcBallCamControl1.Camera = arcBallCam;

            this.panel3D1.Projection = projection;
            this.panel3D1.Camera = arcBallCam;

            this.panel3D2.Projection = projection;
            this.panel3D2.Camera = flyCam;

            prepareWorld("jaw");
        }

        private void LstDemos_DoubleClick(object sender, EventArgs e) {
            prepareWorld(lstDemos.SelectedValue as string);
        }

        void prepareWorld(string id) {
            var world = new World();

            ITextureReader textureReader = new TextureReaderBMP();

            switch(id) {
                case "jaw":
                    world.Models.Add(ModelReader.ReadFile(@"models\original.stl").First());
                    world.Models.Add(ModelReader.ReadFile(@"models\offset_2.stl").First());
                    arcBallCam.Position += new Vector3(0, 0, -5 - arcBallCam.Position.Z);
                    break;
                case "stl-mesh-1":
                    world.Models.Add(ModelReader.ReadFile(@"models\Planetary_Toy_D80.stl").First());
                    arcBallCam.Position += new Vector3(0, 0, -5 - arcBallCam.Position.Z);
                    break;
                case "stl-mesh-2":
                    world.Models.Add(ModelReader.ReadFile(@"models\Star_Destroyer_Fixed.stl").First());
                    arcBallCam.Position += new Vector3(0, 0, -5 - arcBallCam.Position.Z);
                    break;
                case "skull":
                    var model = ModelReader.ReadFile(@"models\skull.dae").First();
                    if(model is BasicModel && !(model is TexturedModel)) {
                        world.Models.Add(model);
                    }
                    else if(model is TexturedModel) {
                        var texture = new TextureReaderBMP().ReadImage(@"textures\bone_high.bmp");
                        (model as TexturedModel).Texture = texture;
                        world.Models.Add(model);
                    }
                    else {
                        throw new Exception($"Invalid object type when reading creating model, object type: {model.GetType()}");
                    }
                    arcBallCam.Position += new Vector3(0, 0,  -5 - arcBallCam.Position.Z);
                    break;

                case "teapot":
                    model = ModelReader.ReadFile(@"models\teapot.dae").First();
                    if(model is BasicModel && !(model is TexturedModel)) {
                        world.Models.Add(model);
                    }
                    else if(model is TexturedModel) {
                        var texture = new TextureReaderBMP().ReadImage(@"textures\glass_effect.bmp");
                        (model as TexturedModel).Texture = texture;
                        world.Models.Add(model);
                    }
                    else {
                        throw new Exception($"Invalid object type when reading creating model, object type: {model.GetType()}");
                    }
                    arcBallCam.Position += new Vector3(0, 0, -5 - arcBallCam.Position.Z);
                    break;
                    
                case "empty":
                    break;
            }

            world.LightSources.Add(new LightSource { Position = new Vector3(0, 0, 10) });

            // var camObject = new Cube() { Position = arcBallCam.Position };

            arcBallCam.CameraChanged -= MainCam_CameraChanged;
            arcBallCam.CameraChanged += MainCam_CameraChanged;

            // world.Volumes.Add(camObject);

            this.panel3D1.World = world;
            this.panel3D2.World = world;

            this.panel3D1.Invalidate();
            this.panel3D2.Invalidate();

            void MainCam_CameraChanged(object cam, EventArgs _1) {
                // camObject.Position = ((ArcBallCam)cam).Position;
                // this.panel3D2.Invalidate();
            }
        }

        private void btnChangeTexture_Click(object sender, EventArgs e) {
            this.panel3D1.RendererSettings.changeActiveTexture();
        }
    }
}