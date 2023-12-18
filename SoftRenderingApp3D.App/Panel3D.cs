using SoftRenderingApp3D;
using SoftRenderingApp3D.DataStructures;
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
        SubsurfaceScatteringRenderContext renderContext { get; }

        ICamera camera;
        IWorld world;
        IProjection projection;
        ISubsurfaceScatteringRenderer renderer;
        ISubsurfaceScatteringPainter painter;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public SubsurfaceScatteringRendererSettings RendererSettings {
            get => rendererSettings;
            set {
                if(PropertyChangedHelper.ChangeValue<SubsurfaceScatteringRendererSettings>(ref rendererSettings, value)) {
                    rendererSettings.ShowTriangleNormals = true;
                    renderContext.RendererSettings = value;
                    rendererSettings = value;
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IWorld World {
            get => world;
            set {
                if(PropertyChangedHelper.ChangeValue(ref world, value)) {
                    renderContext.World = world;
                    hookPaintEvent();
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ISubsurfaceScatteringPainter Painter {
            get => painter;
            set {
                if(PropertyChangedHelper.ChangeValue<ISubsurfaceScatteringPainter>(ref painter, value)) {
                    if(painter != null) {
                        painter.RendererContext = renderContext;
                    }
                    assign(renderer, painter);
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ISubsurfaceScatteringRenderer Renderer {
            get => renderer;
            set {
                if(PropertyChangedHelper.ChangeValue(ref renderer, value)) {
                    renderer.SubsurfaceScatteringRenderContext = renderContext;
                    assign(renderer, painter);
                }
            }
        }

        void assign(ISubsurfaceScatteringRenderer renderer, ISubsurfaceScatteringPainter painter) {
            if(renderer == null)
                return;
            else
                renderer.SubsurfaceScatteringPainter = painter;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ICamera Camera {
            get => camera;
            set {
                var oldCamera = camera;

                if(PropertyChangedHelper.ChangeValue(ref camera, value)) {

                    if(oldCamera != null)
                        oldCamera.CameraChanged -= cameraChanged;

                    if(camera != null)
                        camera.CameraChanged += cameraChanged;

                    renderContext.Camera = value;
                    hookPaintEvent();
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
                        oldProjection.ProjectionChanged -= projectionChanged;

                    if(projection != null)
                        projection.ProjectionChanged += projectionChanged; ;

                    renderContext.Projection = value;
                    hookPaintEvent();
                }
            }
        }

        void projectionChanged(object sender, EventArgs e) => this.Invalidate();
        void cameraChanged(object sender, EventArgs e) => this.Invalidate();

        void hookPaintEvent() {
            this.Paint -= Panel3D_Paint;
            if(camera != null && world != null && projection != null) {
                this.Paint += Panel3D_Paint;
            }
        }

        public Panel3D() {
            InitializeComponent();

            renderContext = new SubsurfaceScatteringRenderContext();
            renderContext.Stats = new Stats();

            RendererSettings = new SubsurfaceScatteringRendererSettings() { BackFaceCulling = true };

            Renderer = new SubsurfaceScatteringRenderer();
            Painter = new GouraudSubsurfaceScatteringPainter();

            this.ResizeRedraw = true;

            this.Layout += Panel3D_Layout;
        }

        string format = "Volumes:{0}\nTriangles:{1} - Back:{2} - Out:{3} - Behind:{4}\nPixels:{9} drawn:{5} - Z behind:{6}\nCalc time:{7} - Paint time:{8}";

        StringBuilder sb = new StringBuilder();
        private SubsurfaceScatteringRendererSettings rendererSettings;

        public int[] Render() {
            return renderer.Render();
        }

        void Panel3D_Paint(object sender, PaintEventArgs e) {

            var g = e.Graphics;

            // g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;
            // g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighSpeed;
            // g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighSpeed;

            buildFrame();
            g.DrawImage(bmp, Point.Empty);

            sb.Clear();
            sb.AppendFormat(format,
                world.Volumes.Count,
                renderContext.Stats.TotalTriangleCount,
                renderContext.Stats.FacingBackTriangleCount,
                renderContext.Stats.OutOfViewTriangleCount,
                renderContext.Stats.BehindViewTriangleCount,
                renderContext.Stats.DrawnPixelCount,
                renderContext.Stats.BehindZPixelCount,
                renderContext.Stats.CalculationTimeMs,
                renderContext.Stats.PainterTimeMs,
                renderContext.Stats.DrawnPixelCount + renderContext.Stats.BehindZPixelCount
            );

            TextRenderer.DrawText(g, sb.ToString(), this.Font, Point.Empty, Color.BlueViolet, this.BackColor, TextFormatFlags.ExpandTabs);
        }

        private void Panel3D_Layout(object sender, LayoutEventArgs e) {
            if(this.Size.Height == 0 || this.Size.Width == 0)
                return;

            renderContext.Surface = new SubsurfaceScatteringFrameBuffer(this.Width, this.Height, renderContext);
            bmp = new Bitmap(this.Width, this.Height, PixelFormat.Format32bppPArgb);
        }

        Bitmap bmp;

        void buildFrame() {
            var buffer = renderer.Render();
            ImageUtils.FillBitmap(bmp, buffer, this.Width, this.Height);
        }
    }
}