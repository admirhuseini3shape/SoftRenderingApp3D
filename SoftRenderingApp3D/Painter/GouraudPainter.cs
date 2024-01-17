using SoftRenderingApp3D.Buffer;
using SoftRenderingApp3D.DataStructures.Materials;
using SoftRenderingApp3D.Renderer;
using SoftRenderingApp3D.Utils;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace SoftRenderingApp3D.Painter
{
    public interface IGouraudPainter : IPainter
    {
        Vector3[] FacetNormalLightContributions { get; }
    }

    public class GouraudPainterProvider : IPainterProvider
    {
        public IPainter GetPainter<T>(T material, VertexBuffer vertexBuffer,
            FrameBuffer frameBuffer, RendererSettings rendererSettings)
             where T : IMaterial
        {
            IPainter painter;
            if(material is IVertexColorMaterial colorMaterial)
                painter = new GouraudVertexColorPainter().Create(colorMaterial, vertexBuffer, frameBuffer);
            else if(material is IFacetColorMaterial facetColorMaterial)
                painter = new GouraudFacetColorPainter().Create(facetColorMaterial, vertexBuffer, frameBuffer);
            else if(material is ITextureMaterial textureMaterial && rendererSettings.ShowTextures)
                painter = new GouraudTexturePainter().Create(textureMaterial, vertexBuffer, frameBuffer);
            else
                painter = new GouraudStandardColorPainter().Create(material, vertexBuffer, frameBuffer);

            if(painter == null)
                throw new Exception($"Could not provide a painter for {nameof(T)}!");

            return painter;
        }
    }

    public class GouraudColorPainterBase : IGouraudPainter
    {
        public VertexBuffer VertexBuffer { get; }
        public FrameBuffer FrameBuffer { get; }
        public Barycentric2d BarycentricMapper { get; }

        public Vector3[] FacetNormalLightContributions { get; }

        private readonly HashSet<int> updatedFacetBuffers;

        protected GouraudColorPainterBase() { }

        protected GouraudColorPainterBase(VertexBuffer vertexBuffer, FrameBuffer frameBuffer)
        {
            VertexBuffer = vertexBuffer;
            FrameBuffer = frameBuffer;
            var count = vertexBuffer?.Drawable?.Mesh?.FacetCount ?? 0;
            BarycentricMapper = new Barycentric2d(count);
            FacetNormalLightContributions = new Vector3[count];
            FacetNormalLightContributions.Fill(Vector3.Zero);
            updatedFacetBuffers = new HashSet<int>(count);
        }

        public void UpdateBuffers(int faId)
        {
            if(updatedFacetBuffers.Contains(faId))
                return;

            var facet = VertexBuffer.Drawable.Mesh.Facets[faId];
            BarycentricMapper.UpdateFacet(faId,
                VertexBuffer.ScreenPointVertices[facet.I0],
                VertexBuffer.ScreenPointVertices[facet.I1],
                VertexBuffer.ScreenPointVertices[facet.I2]);


            // This has to move elsewhere
            var lightPos = new Vector3(0, 10, 10);

            var nl0 = MathUtils.ComputeNDotL(
                VertexBuffer.WorldVertices[facet.I0],
                VertexBuffer.WorldVertexNormals[facet.I0],
                lightPos);

            var nl1 = MathUtils.ComputeNDotL(
                VertexBuffer.WorldVertices[facet.I1],
                VertexBuffer.WorldVertexNormals[facet.I1],
                lightPos);
            var nl2 = MathUtils.ComputeNDotL(
                VertexBuffer.WorldVertices[facet.I2],
                VertexBuffer.WorldVertexNormals[facet.I2],
                lightPos);
            FacetNormalLightContributions[faId] = new Vector3(nl0, nl1, nl2);

            updatedFacetBuffers.Add(faId);
        }

        public void ClearBuffers()
        {
            BarycentricMapper.Clear();
            FacetNormalLightContributions.Fill(Vector3.Zero);
            updatedFacetBuffers.Clear();
        }

        public virtual int DrawPixel(int x, int y, RendererSettings rendererSettings)
        {
            return 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected int DrawPixel(int x, int y, RendererSettings rendererSettings, ColorRGB color)
        {
            return DrawPixel(x, y, rendererSettings, color, color, color);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected int DrawPixel(int x, int y,
            RendererSettings rendererSettings, ColorRGB color0, ColorRGB color1, ColorRGB color2)
        {
            var faId = FrameBuffer.GetFacetIdForPixel(x, y);

            var barycentric = BarycentricMapper.GetBarycentric(x, y, faId);
            var alpha = barycentric.X;
            var beta = barycentric.Y;
            var gamma = barycentric.Z;
            var normLight = FacetNormalLightContributions[faId];
            var lightContribution = (alpha * normLight.X + beta * normLight.Y + gamma * normLight.Z).Clamp();

            // interpolate
            var r = (byte)(alpha * color0.R + beta * color1.R + gamma * color2.R); //.Clamp(0, maxR);
            var g = (byte)(alpha * color0.G + beta * color1.G + gamma * color2.G); //.Clamp(0, maxG);
            var b = (byte)(alpha * color0.B + beta * color1.B + gamma * color2.B); //.Clamp(0, maxB);
            var color = (lightContribution * new ColorRGB(r, g, b, 255)).Color;

            return color;
        }
    }

    public class GouraudStandardColorPainter : GouraudColorPainterBase, IPainter<IMaterial>
    {
        public IMaterial Material { get; private set; }

        public IPainter<IMaterial> Create(IMaterial material,
            VertexBuffer vertexBuffer, FrameBuffer frameBuffer)
        {
            return new GouraudStandardColorPainter(material, vertexBuffer, frameBuffer);
        }

        public GouraudStandardColorPainter() { }

        private GouraudStandardColorPainter(IMaterial material,
            VertexBuffer vertexBuffer, FrameBuffer frameBuffer) : base(vertexBuffer, frameBuffer)
        {
            Material = material;
        }

        public int DrawPixel(int x, int y, RendererSettings rendererSettings)
        {
            return DrawPixel(x, y, rendererSettings, Constants.StandardColor);
        }
    }

    public class GouraudVertexColorPainter : GouraudColorPainterBase, IPainter<IVertexColorMaterial>
    {
        public IVertexColorMaterial Material { get; private set; }

        public IPainter<IVertexColorMaterial> Create(IVertexColorMaterial material,
            VertexBuffer vertexBuffer, FrameBuffer frameBuffer)
        {
            return new GouraudVertexColorPainter(material, vertexBuffer, frameBuffer);
        }

        public GouraudVertexColorPainter() { }

        public GouraudVertexColorPainter(IVertexColorMaterial material,
            VertexBuffer vertexBuffer, FrameBuffer frameBuffer) : base(vertexBuffer, frameBuffer)
        {
            Material = material;
        }

        public int DrawPixel(int x, int y, RendererSettings rendererSettings)
        {
            var faId = FrameBuffer.GetFacetIdForPixel(x, y);
            var facet = VertexBuffer.Drawable.Mesh.Facets[faId];

            var color0 = Material.VertexColors[facet.I0];
            var color1 = Material.VertexColors[facet.I1];
            var color2 = Material.VertexColors[facet.I2];

            return DrawPixel(x, y, rendererSettings, color0, color1, color2);
        }
    }

    public class GouraudFacetColorPainter : GouraudColorPainterBase, IPainter<IFacetColorMaterial>
    {
        public IFacetColorMaterial Material { get; private set; }

        public IPainter<IFacetColorMaterial> Create(IFacetColorMaterial material,
            VertexBuffer vertexBuffer, FrameBuffer frameBuffer)
        {
            return new GouraudFacetColorPainter(material, vertexBuffer, frameBuffer);
        }

        public GouraudFacetColorPainter() { }

        public GouraudFacetColorPainter(IFacetColorMaterial material,
            VertexBuffer vertexBuffer, FrameBuffer frameBuffer) : base(vertexBuffer, frameBuffer)
        {
            Material = material;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int DrawPixel(int x, int y, RendererSettings rendererSettings)
        {
            var faId = FrameBuffer.GetFacetIdForPixel(x, y);
            return DrawPixel(x, y, rendererSettings, Material.FacetColors[faId]);
        }
    }

    public class GouraudTexturePainter : GouraudColorPainterBase, IPainter<ITextureMaterial>
    {
        public ITextureMaterial Material { get; }

        public IPainter<ITextureMaterial> Create(ITextureMaterial material,
            VertexBuffer vertexBuffer, FrameBuffer frameBuffer)
        {
            return new GouraudTexturePainter(material, vertexBuffer, frameBuffer);
        }

        public GouraudTexturePainter() { }

        public GouraudTexturePainter(ITextureMaterial material, VertexBuffer vertexBuffer, FrameBuffer frameBuffer)
        : base(vertexBuffer, frameBuffer)
        {
            Material = material;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int DrawPixel(int x, int y, RendererSettings rendererSettings)
        {
            var faId = FrameBuffer.GetFacetIdForPixel(x, y);
            var facet = VertexBuffer.Drawable.Mesh.Facets[faId];

            // Get the texture coordinates of each point of the triangle
            var uv0 = VertexBuffer.Drawable.Mesh.TextureCoordinates[facet.I0];
            var uv1 = VertexBuffer.Drawable.Mesh.TextureCoordinates[facet.I1];
            var uv2 = VertexBuffer.Drawable.Mesh.TextureCoordinates[facet.I2];

            var barycentric = BarycentricMapper.GetBarycentric(x, y, faId);
            var alpha = barycentric.X;
            var beta = barycentric.Y;
            var gamma = barycentric.Z;
            var normLight = FacetNormalLightContributions[faId];
            var lightContribution = (alpha * normLight.X + beta * normLight.Y + gamma * normLight.Z).Clamp();

            var texX = uv0.X * alpha + uv1.X * beta + uv2.X * gamma;
            var texY = uv0.Y * alpha + uv1.Y * beta + uv2.Y * gamma;

            var initialColor = rendererSettings.LinearTextureFiltering ?
                Material.Texture.GetPixelColorLinearFiltering(texX, texY) :
                Material.Texture.GetPixelColorNearestFiltering(texX, texY);

            var color = (lightContribution * initialColor).Color;
            return color;
        }


    }
}
