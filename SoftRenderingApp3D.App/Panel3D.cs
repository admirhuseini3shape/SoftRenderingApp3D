using SoftRenderingApp3D.Buffer;
using SoftRenderingApp3D.Camera;
using SoftRenderingApp3D.Controls;
using SoftRenderingApp3D.DataStructures.Drawables;
using SoftRenderingApp3D.Painter;
using SoftRenderingApp3D.Projection;
using SoftRenderingApp3D.Renderer;
using SoftRenderingApp3D.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.Windows.Forms;

namespace SoftRenderingApp3D.App
{
    public partial class Panel3D : UserControl
    {
        private const string Format =
            "Meshes:{0}\nTriangles:{1} - Back:{2} - Out:{3} - Behind:{4}\nPixels:{9} drawn:{5} - Z behind:{6}\nCalc time:{7} - Paint time:{8}";

        private RendererSettings rendererSettings;

        private Bitmap bmp;

        private ICamera camera;
        private IPainterProvider painterProvider;
        private IProjection projection;
        private IRenderer renderer;
        private PickingControl pickingControl;
        private MouseControlBase mouseControlBase;

        private readonly StringBuilder sb = new StringBuilder();
        private IDrawable drawable;

        public Panel3D()
        {
            InitializeComponent();

            RendererSettings = new RendererSettings { BackFaceCulling = false };
            
            PainterProvider = new GouraudPainterProvider();
            Renderer = new SimpleRenderer(new VertexBuffer(drawable), new FrameBuffer(Width, Height));
            ResizeRedraw = true;

            pickingControl = new PickingControl(this, Renderer.FrameBuffer);
            
            Layout += Panel3D_Layout;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public RendererSettings RendererSettings
        {
            get
            {
                return rendererSettings;
            }
            private set
            {
                if(!value.TryUpdateOther(ref rendererSettings))
                    return;


                rendererSettings.ShowTriangleNormals = true;
                rendererSettings = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IDrawable Drawable
        {
            get
            {
                return drawable;
            }
            set
            {
                if(!value.TryUpdateOther(ref drawable))
                    return;

                Renderer?.VertexBuffer?.Dispose();
                var frameBuffer = Renderer?.FrameBuffer;
                Renderer = new SimpleRenderer(new VertexBuffer(drawable), frameBuffer);
                HookPaintEvent();
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IPainterProvider PainterProvider
        {
            get
            {
                return painterProvider;
            }
            set
            {
                value.TryUpdateOther(ref painterProvider);
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IRenderer Renderer
        {
            get
            {
                return renderer;
            }
            set
            {
                value.TryUpdateOther(ref renderer);
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ICamera Camera
        {
            get
            {
                return camera;
            }
            set
            {
                var oldCamera = camera;

                if(!value.TryUpdateOther(ref camera))
                    return;


                if(oldCamera != null)
                {
                    oldCamera.CameraChanged -= CameraChanged;
                }

                if(camera != null)
                {
                    camera.CameraChanged += CameraChanged;
                }

                HookPaintEvent();
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IProjection Projection
        {
            get
            {
                return projection;
            }
            set
            {
                var oldProjection = projection;

                if(!value.TryUpdateOther(ref projection))
                    return;

                if(oldProjection != null)
                {
                    oldProjection.ProjectionChanged -= ProjectionChanged;
                }

                if(projection != null)
                {
                    projection.ProjectionChanged += ProjectionChanged;
                }

                HookPaintEvent();
            }
        }

        private void ProjectionChanged(object sender, EventArgs e)
        {
            Invalidate();
        }

        private void CameraChanged(object sender, EventArgs e)
        {
            Invalidate();
        }

        private void HookPaintEvent()
        {
            Paint -= Panel3D_Paint;
            if(camera != null && drawable != null && projection != null)
            {
                Paint += Panel3D_Paint;
            }
        }

        public int[] Render()
        {
            var viewMatrix = Camera.ViewMatrix();
            var projectionMatrix = Projection.ProjectionMatrix(Width, Height);
            return renderer.Render(PainterProvider, viewMatrix, projectionMatrix, RendererSettings);
        }

        private void Panel3D_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;

            BuildFrame();
            // pickingControl.OnMouseClicked(PointToClient(MousePosition));
            g.DrawImage(bmp, Point.Empty);
            var stats = StatsSingleton.Instance;
            sb.Clear();
            sb.AppendFormat(Format,
                1,
                stats.TotalTriangleCount,
                stats.FacingBackTriangleCount,
                stats.OutOfViewTriangleCount,
                stats.BehindViewTriangleCount,
                stats.DrawnPixelCount,
                stats.BehindZPixelCount,
                stats.CalculationTimeMs,
                stats.PainterTimeMs,
                stats.DrawnPixelCount + stats.BehindZPixelCount
            );

            TextRenderer.DrawText(g, sb.ToString(), Font, Point.Empty, Color.BlueViolet, BackColor,
                TextFormatFlags.ExpandTabs);
        }

        private void Panel3D_Layout(object sender, LayoutEventArgs e)
        {
            if(Size.Height == 0 || Size.Width == 0)
            {
                return;
            }

            var vertexBuffer = Renderer.VertexBuffer;
            Renderer = new SimpleRenderer(vertexBuffer, new FrameBuffer(Width, Height));
            var pickingControl = new PickingControl(this, Renderer.FrameBuffer);
            var selectedFacets = new List<int>(pickingControl.SelectedFacets);
            bmp = new Bitmap(Width, Height, PixelFormat.Format32bppPArgb);
        }

        private void BuildFrame()
        {
            var buffer = Render();
            ImageUtils.FillBitmap(bmp, buffer, Width, Height);
        }
    }
}
