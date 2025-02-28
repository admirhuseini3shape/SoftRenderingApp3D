﻿using SoftRenderingApp3D;
using SoftRenderingApp3D.Camera;
using SoftRenderingApp3D.Controls;
using SoftRenderingApp3D.DataStructures.Drawables;
using SoftRenderingApp3D.DataStructures.FileReaders;
using SoftRenderingApp3D.DataStructures.Materials;
using SoftRenderingApp3D.Projection;
using SoftRenderingApp3D.Utils;
using SubsurfaceScatteringLibrary.Renderer;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Windows.Forms;

namespace SubsurfaceScatteringSoftRenderingApp3D
{
    public partial class Form1 : Form
    {
        private readonly ArcBallCam arcBallCam;
        private FlyCam flyCam;

        public Form1()
        {
            InitializeComponent();
            var projection = new FovPerspectiveProjection(40f * (float)Math.PI / 180f, .01f, 500f);
            arcBallCam = new ArcBallCam { Position = new Vector3(0, 0, -25) };
            flyCam = new FlyCam { Position = new Vector3(0, 0, -25) };
            new ArcBallCamHandler(panel3D1, arcBallCam);
            arcBallCamControl1.Camera = arcBallCam;
            panel3D1.Projection = projection;
            panel3D1.Camera = arcBallCam;
            trackBar1.Value = 50;
            trackBar2.Value = 40;
            trackBar5.Value = 10;
            trackBar6.Value = 50;
            trackBar3.Value = 100;
            trackBar4.Value = 100;

            checkBox1.Checked = false;
            checkBox2.Checked = false;

            PrepareWorld("jaw");
        }

        private void PrepareWorld(string id)
        {
            const int drawablesCount = 4;
            var stlReader = new STLReader();
            var drawables = new List<IDrawable>(drawablesCount);
            switch(id)
            {
                case "jaw":
                    drawables.Add(stlReader.ReadFile(@"models\original.stl"));
                    // Add a cube that represents the light
                    drawables.Add(stlReader.ReadFile(@"models\offset_3.stl"));
                    drawables.Add(stlReader.ReadFile(@"models\caries.stl"));
                    var material1 = new FacetColorMaterial(drawables[1].Mesh.FacetCount, ColorRGB.Gray);
                    drawables[1] = new Drawable(drawables[1].Mesh, material1);

                    var material2 = new FacetColorMaterial(drawables[2].Mesh.FacetCount, ColorRGB.Gray);
                    drawables[2] = new Drawable(drawables[2].Mesh, material2);

                    arcBallCam.Position += new Vector3(0, 10, -50 - arcBallCam.Position.Z);
                    break;
            }

            arcBallCam.CameraChanged -= MainCam_CameraChanged;
            arcBallCam.CameraChanged += MainCam_CameraChanged;
            // world.Drawables.Add(camObject);
            panel3D1.Drawables = drawables;
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

        private void label1_Click(object sender, EventArgs e)
        {
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            SubsurfaceScatteringRenderUtils.ChangeVisibility(trackBar1.Value);
            panel3D1.Invalidate();
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            SubsurfaceScatteringRenderUtils.ChangeSubsurfaceScatteringStrength(trackBar2.Value);
            panel3D1.Invalidate();
        }

        private void trackBar4_Scroll(object sender, EventArgs e)
        {
            SubsurfaceScatteringRenderUtils.ChangeSurfaceColor(trackBar4.Value);
            panel3D1.Invalidate();
        }

        private void trackBar3_Scroll(object sender, EventArgs e)
        {
            SubsurfaceScatteringRenderUtils.ChangeSubsurfaceColor(trackBar3.Value);
            panel3D1.Invalidate();
        }

        private void trackBar5_Scroll(object sender, EventArgs e)
        {
            SubsurfaceScatteringRenderUtils.ChangeSubsurfaceDecay(trackBar5.Value);
            panel3D1.Invalidate();
        }

        private void trackBar6_Scroll(object sender, EventArgs e)
        {
            SubsurfaceScatteringRenderUtils.ChangeGaussianBlurStDev(trackBar6.Value);
            panel3D1.Invalidate();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            SubsurfaceScatteringRenderUtils.ToggleGaussianBlur();
            trackBar6.Enabled = RenderUtils.GaussianBlur;
            panel3D1.Invalidate();
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            SubsurfaceScatteringRenderUtils.ToggleOnlySubsurfaceBlur();
            panel3D1.Invalidate();
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            SubsurfaceScatteringRenderUtils.ToggleCaries();
            panel3D1.Invalidate();
        }
    }
}
