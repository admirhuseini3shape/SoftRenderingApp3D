using SoftRenderingApp3D.Buffer;
using SoftRenderingApp3D.Camera;
using SoftRenderingApp3D.Controls;
using SoftRenderingApp3D.DataStructures.World;
using SoftRenderingApp3D.Painter;
using SoftRenderingApp3D.Projection;
using SoftRenderingApp3D.Renderer;
using SoftRenderingApp3D.Utils;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.Windows.Forms;

namespace SoftRenderingApp3D.App {
    public partial class Panel3D : UserControl {
        private const string Format =
            "Volumes:{0}\nTriangles:{1} - Back:{2} - Out:{3} - Behind:{4}\nPixels:{9} drawn:{5} - Z behind:{6}\nCalc time:{7} - Paint time:{8}";

        private RendererSettings _rendererSettings;

        private Bitmap bmp;

        private ICamera camera;
        private IPainter painter;
        private IProjection projection;
        private IRenderer renderer;

        private readonly StringBuilder sb = new StringBuilder();
        private IWorld world;

        public Panel3D() {
            InitializeComponent();

            RenderContext = new RenderContext();
            RenderContext.Stats = new Stats();

            RendererSettings = new RendererSettings { BackFaceCulling = true };

            Renderer = new SimpleRenderer();
            Painter = new GouraudPainter();

            ResizeRedraw = true;

            Layout += Panel3D_Layout;
        }

        private RenderContext RenderContext { get; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public RendererSettings RendererSettings {
            get {
                return _rendererSettings;
            }
            set {
                if(PropertyChangedHelper.ChangeValue(ref _rendererSettings, value)) {
                    _rendererSettings.ShowTriangleNormals = true;
                    RenderContext.RendererSettings = value;
                    _rendererSettings = value;
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IWorld World {
            get {
                return world;
            }
            set {
                if(PropertyChangedHelper.ChangeValue(ref world, value)) {
                    RenderContext.World = world;
                    HookPaintEvent();
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IPainter Painter {
            get {
                return painter;
            }
            set {
                if(PropertyChangedHelper.ChangeValue(ref painter, value)) {
                    if(painter != null) {
                        painter.RendererContext = RenderContext;
                    }

                    Assign(renderer, painter);
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IRenderer Renderer {
            get {
                return renderer;
            }
            set {
                if(PropertyChangedHelper.ChangeValue(ref renderer, value)) {
                    renderer.RenderContext = RenderContext;
                    Assign(renderer, painter);
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ICamera Camera {
            get {
                return camera;
            }
            set {
                var oldCamera = camera;

                if(PropertyChangedHelper.ChangeValue(ref camera, value)) {
                    if(oldCamera != null) {
                        oldCamera.CameraChanged -= CameraChanged;
                    }

                    if(camera != null) {
                        camera.CameraChanged += CameraChanged;
                    }

                    RenderContext.Camera = value;
                    HookPaintEvent();
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IProjection Projection {
            get {
                return projection;
            }
            set {
                var oldProjection = projection;

                if(PropertyChangedHelper.ChangeValue(ref projection, value)) {
                    if(oldProjection != null) {
                        oldProjection.ProjectionChanged -= ProjectionChanged;
                    }

                    if(projection != null) {
                        projection.ProjectionChanged += ProjectionChanged;
                    }

                    ;

                    RenderContext.Projection = value;
                    HookPaintEvent();
                }
            }
        }

        private static void Assign(IRenderer renderer, IPainter painter) {
            if(renderer == null) {
                return;
            }

            renderer.Painter = painter;
        }

        private void ProjectionChanged(object sender, EventArgs e) {
            Invalidate();
        }

        private void CameraChanged(object sender, EventArgs e) {
            Invalidate();
        }

        private void HookPaintEvent() {
            Paint -= Panel3D_Paint;
            if(camera != null && world != null && projection != null) {
                Paint += Panel3D_Paint;
            }
        }

        public int[] Render() {
            return renderer.Render();
        }

        private void Panel3D_Paint(object sender, PaintEventArgs e) {
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

            TextRenderer.DrawText(g, sb.ToString(), Font, Point.Empty, Color.BlueViolet, BackColor,
                TextFormatFlags.ExpandTabs);
        }

        private void Panel3D_Layout(object sender, LayoutEventArgs e) {
            if(Size.Height == 0 || Size.Width == 0) {
                return;
            }

            RenderContext.Surface = new FrameBuffer(Width, Height, RenderContext);
            bmp = new Bitmap(Width, Height, PixelFormat.Format32bppPArgb);
        }

        private void BuildFrame() {
            var buffer = renderer.Render();
            ImageUtils.FillBitmap(bmp, buffer, Width, Height);
        }
    }
}