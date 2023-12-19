using SoftRenderingApp3D;
using SoftRenderingApp3D.Buffer;
using SoftRenderingApp3D.DataStructures;
using SoftRenderingApp3D.Painter;
using SoftRenderingApp3D.Renderer;
using SubsurfaceScatteringLibrary.Buffer;
using SubsurfaceScatteringLibrary.Painter;
using SubsurfaceScatteringLibrary.Renderer;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.Windows.Forms;

namespace SoftRenderingApp {
    public partial class Panel3D : UserControl {
        RenderContext RenderContext { get; }

        ICamera camera;
        IWorld world;
        IProjection projection;
        IRenderer renderer;
        IPainter painter;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public RendererSettings RendererSettings {
            get => _rendererSettings;
            set {
                if(PropertyChangedHelper.ChangeValue<RendererSettings>(ref _rendererSettings, value)) {
                    _rendererSettings.ShowTriangleNormals = true;
                    RenderContext.RendererSettings = value;
                    _rendererSettings = value;
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IWorld World {
            get => world;
            set {
                if(PropertyChangedHelper.ChangeValue(ref world, value)) {
                    RenderContext.World = world;
                    HookPaintEvent();
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IPainter Painter {
            get => painter;
            set {
                if(PropertyChangedHelper.ChangeValue<IPainter>(ref painter, value)) {
                    if(painter != null) {
                        painter.RendererContext = RenderContext;
                    }
                    Assign(renderer, painter);
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IRenderer Renderer {
            get => renderer;
            set {
                if(PropertyChangedHelper.ChangeValue(ref renderer, value)) {
                    renderer.RenderContext = RenderContext;
                    Assign(renderer, painter);
                }
            }
        }

        static void Assign(IRenderer renderer, IPainter painter) {
            if(renderer == null)
                return;
            else
                renderer.Painter = painter;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ICamera Camera {
            get => camera;
            set {
                var oldCamera = camera;

                if(PropertyChangedHelper.ChangeValue(ref camera, value)) {

                    if(oldCamera != null)
                        oldCamera.CameraChanged -= CameraChanged;

                    if(camera != null)
                        camera.CameraChanged += CameraChanged;

                    RenderContext.Camera = value;
                    HookPaintEvent();
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IProjection Projection {
            get => projection;
            set {
                var oldProjection = projection;

                if(PropertyChangedHelper.ChangeValue(ref projection, value)) {

                    if(oldProjection != null)
                        oldProjection.ProjectionChanged -= ProjectionChanged;

                    if(projection != null)
                        projection.ProjectionChanged += ProjectionChanged; ;

                    RenderContext.Projection = value;
                    HookPaintEvent();
                }
            }
        }

        void ProjectionChanged(object sender, EventArgs e) => this.Invalidate();
        void CameraChanged(object sender, EventArgs e) => this.Invalidate();

        void HookPaintEvent() {
            this.Paint -= Panel3D_Paint;
            if(camera != null && world != null && projection != null) {
                this.Paint += Panel3D_Paint;
            }
        }

        public Panel3D() {
            InitializeComponent();

            RenderContext = new RenderContext();
            RenderContext.Stats = new Stats();

            RendererSettings = new RendererSettings() { BackFaceCulling = true };

            Renderer = new SimpleRenderer();
            Painter = new GouraudPainter();

            this.ResizeRedraw = true;

            this.Layout += Panel3D_Layout;
        }

        private const string Format = "Volumes:{0}\nTriangles:{1} - Back:{2} - Out:{3} - Behind:{4}\nPixels:{9} drawn:{5} - Z behind:{6}\nCalc time:{7} - Paint time:{8}";

        StringBuilder sb = new StringBuilder();
        private RendererSettings _rendererSettings;

        public int[] Render() {
            return renderer.Render();
        }

        void Panel3D_Paint(object sender, PaintEventArgs e) {

            var g = e.Graphics;

            BuildFrame();
            g.DrawImage(bmp, Point.Empty);

            sb.Clear();
            sb.AppendFormat(Format,
                world.Volumes.Count,
                RenderContext.Stats.TotalTriangleCount,
                RenderContext.Stats.FacingBackTriangleCount,
                RenderContext.Stats.OutOfViewTriangleCount,
                RenderContext.Stats.BehindViewTriangleCount,
                RenderContext.Stats.DrawnPixelCount,
                RenderContext.Stats.BehindZPixelCount,
                RenderContext.Stats.CalculationTimeMs,
                RenderContext.Stats.PainterTimeMs,
                RenderContext.Stats.DrawnPixelCount + RenderContext.Stats.BehindZPixelCount
            );

            TextRenderer.DrawText(g, sb.ToString(), this.Font, Point.Empty, Color.BlueViolet, this.BackColor, TextFormatFlags.ExpandTabs);
        }

        private void Panel3D_Layout(object sender, LayoutEventArgs e) {
            if(this.Size.Height == 0 || this.Size.Width == 0)
                return;

            RenderContext.Surface = new FrameBuffer(this.Width, this.Height, RenderContext);
            bmp = new Bitmap(this.Width, this.Height, PixelFormat.Format32bppPArgb);
        }

        Bitmap bmp;

        void BuildFrame() {
            var buffer = renderer.Render();
            ImageUtils.FillBitmap(bmp, buffer, this.Width, this.Height);
        }
    }
}