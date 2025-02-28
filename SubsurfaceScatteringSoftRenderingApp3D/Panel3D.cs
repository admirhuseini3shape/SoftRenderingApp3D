﻿using SoftRenderingApp3D;
using SoftRenderingApp3D.Camera;
using SoftRenderingApp3D.Controls;
using SoftRenderingApp3D.DataStructures.Drawables;
using SoftRenderingApp3D.Projection;
using SoftRenderingApp3D.Utils;
using SubsurfaceScatteringLibrary.Buffer;
using SubsurfaceScatteringLibrary.Painter;
using SubsurfaceScatteringLibrary.Renderer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.Windows.Forms;

namespace SubsurfaceScatteringSoftRenderingApp3D
{
    public partial class Panel3D : UserControl
    {

        private Bitmap bmp;
        private ICamera camera;

        private readonly string format =
            "Meshes:{0}\nTriangles:{1} - Back:{2} - Out:{3} - Behind:{4}\nPixels:{9} drawn:{5} - Z behind:{6}\nCalc time:{7} - Paint time:{8}";

        private ISubsurfaceScatteringPainter painter;
        private IProjection projection;
        private ISubsurfaceScatteringRenderer renderer;
        private List<IDrawable> drawables;
        private SubsurfaceScatteringRendererSettings rendererSettings;

        private readonly StringBuilder sb = new StringBuilder();

        public Panel3D()
        {
            InitializeComponent();

            renderContext = new SubsurfaceScatteringRenderContext();
            renderContext.Stats = new Stats();

            RendererSettings = new SubsurfaceScatteringRendererSettings { BackFaceCulling = true };

            Renderer = new SubsurfaceScatteringRenderer();
            Painter = new GouraudSubsurfaceScatteringPainter();

            ResizeRedraw = true;

            Layout += Panel3D_Layout;
        }

        private SubsurfaceScatteringRenderContext renderContext { get; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public SubsurfaceScatteringRendererSettings RendererSettings
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
                renderContext.RendererSettings = value;
                rendererSettings = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public List<IDrawable> Drawables
        {
            get
            {
                return drawables;
            }
            set
            {
                if(!value.TryUpdateOther(ref drawables))
                    return;

                renderContext.Drawables = drawables;
                hookPaintEvent();
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ISubsurfaceScatteringPainter Painter
        {
            get
            {
                return painter;
            }
            set
            {
                if(!value.TryUpdateOther(ref painter))
                    return;

                if(painter != null)
                {
                    painter.RendererContext = renderContext;
                }

                assign(renderer, painter);
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ISubsurfaceScatteringRenderer Renderer
        {
            get
            {
                return renderer;
            }
            set
            {
                if(!value.TryUpdateOther(ref renderer))
                    return;

                renderer.SubsurfaceScatteringRenderContext = renderContext;
                assign(renderer, painter);
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
                    oldCamera.CameraChanged -= cameraChanged;
                }

                if(camera != null)
                {
                    camera.CameraChanged += cameraChanged;
                }

                renderContext.Camera = value;
                hookPaintEvent();
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
                    oldProjection.ProjectionChanged -= projectionChanged;
                }

                if(projection != null)
                {
                    projection.ProjectionChanged += projectionChanged;
                }

                ;

                renderContext.Projection = value;
                hookPaintEvent();
            }
        }

        private void assign(ISubsurfaceScatteringRenderer renderer, ISubsurfaceScatteringPainter painter)
        {
            if(renderer == null)
            {
                return;
            }

            renderer.SubsurfaceScatteringPainter = painter;
        }

        private void projectionChanged(object sender, EventArgs e)
        {
            Invalidate();
        }

        private void cameraChanged(object sender, EventArgs e)
        {
            Invalidate();
        }

        private void hookPaintEvent()
        {
            Paint -= Panel3D_Paint;
            if(camera != null && drawables != null && projection != null)
            {
                Paint += Panel3D_Paint;
            }
        }

        public int[] Render()
        {
            return renderer.Render();
        }

        private void Panel3D_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;

            // g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;
            // g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighSpeed;
            // g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighSpeed;

            buildFrame();
            g.DrawImage(bmp, Point.Empty);

            sb.Clear();
            sb.AppendFormat(format,
                drawables.Count,
                StatsSingleton.Instance.TotalTriangleCount,
                StatsSingleton.Instance.FacingBackTriangleCount,
                StatsSingleton.Instance.OutOfViewTriangleCount,
                StatsSingleton.Instance.BehindViewTriangleCount,
                StatsSingleton.Instance.DrawnPixelCount,
                StatsSingleton.Instance.BehindZPixelCount,
                StatsSingleton.Instance.CalculationTimeMs,
                StatsSingleton.Instance.PainterTimeMs,
                StatsSingleton.Instance.DrawnPixelCount + StatsSingleton.Instance.BehindZPixelCount
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

            renderContext.Surface = new SubsurfaceScatteringFrameBuffer(Width, Height, renderContext);
            bmp = new Bitmap(Width, Height, PixelFormat.Format32bppPArgb);
        }

        private void buildFrame()
        {
            var buffer = renderer.Render();
            ImageUtils.FillBitmap(bmp, buffer, Width, Height);
        }
    }
}
