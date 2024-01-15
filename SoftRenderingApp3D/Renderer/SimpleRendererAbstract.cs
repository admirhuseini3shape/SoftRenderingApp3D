using SoftRenderingApp3D.Buffer;
using SoftRenderingApp3D.DataStructures.Drawables;
using SoftRenderingApp3D.DataStructures.Materials;
using SoftRenderingApp3D.Painter;
using SoftRenderingApp3D.Rasterizers;
using SoftRenderingApp3D.Utils;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace SoftRenderingApp3D.Renderer
{
    public abstract class SimpleRendererAbstract : IRenderer
    {
        public VertexBuffer VertexBuffer { get; }
        public FrameBuffer FrameBuffer { get; }

        protected SimpleRendererAbstract(VertexBuffer vertexBuffer, FrameBuffer frameBuffer)
        {
            VertexBuffer = vertexBuffer;
            FrameBuffer = frameBuffer;
        }

        public int[] Render(IPainter painter, IDrawable drawable, Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix, RendererSettings rendererSettings)
        {
            FrameBuffer.Clear();
            if(drawable == null || painter == null || rendererSettings == null || drawable.Mesh.FacetCount == 0)
            {
                return FrameBuffer.Screen;
            }

            var stats = StatsSingleton.Instance;
            stats.Clear();

            UpdateVertexBuffer(drawable, viewMatrix, projectionMatrix, stats);
            stats.paintSw.Restart();

            //var zSortedFacets = drawable.Mesh.Facets
            //.Select((fa, i) => new { FaId = i, zDepth = fa.CalculateZAverages(vertexBuffer.ProjectionVertices) })
            //.ToList();
            //zSortedFacets.Sort((x, y) => (int)(1000 * x.zDepth - 1000 * y.zDepth));


            DrawFacets(painter, drawable, rendererSettings, stats);
            stats.paintSw.Stop();

            return FrameBuffer.Screen;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract void DrawFacets(IPainter painter, IDrawable drawable, RendererSettings rendererSettings, Stats stats);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UpdateVertexBuffer(IDrawable drawable, Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix, Stats stats)
        {
            stats.calcSw.Restart();
            VertexBuffer.Clear();

            // model => worldMatrix => world => viewMatrix => view => projectionMatrix => projection => toNdc => ndc => toScreen => screen

            // Transform and store vertices to View
            VertexBuffer.Drawable = drawable;

            UpdateVertexBuffers(viewMatrix, projectionMatrix);
            var facetsCount = drawable.Mesh.Facets.Count;
            stats.TotalTriangleCount += facetsCount;
            
            stats.calcSw.Stop();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract void UpdateVertexBuffers(Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UpdateSingleVertexBuffer(Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix, int veId)
        {
            VertexBuffer.WorldVertices[veId] = viewMatrix.Transform(VertexBuffer.Drawable.Mesh.Vertices[veId]);
            VertexBuffer.WorldVertexNormals[veId] =
                viewMatrix.TransformWithoutTranslation(VertexBuffer.Drawable.Mesh.VertexNormals[veId]);
            VertexBuffer.ViewVertices[veId] = viewMatrix.Transform(VertexBuffer.Drawable.Mesh.Vertices[veId]);
            VertexBuffer.ProjectionVertices[veId] = Vector4.Transform(VertexBuffer.ViewVertices[veId], projectionMatrix);
            VertexBuffer.ScreenPointVertices[veId] = FrameBuffer.ToScreen3(VertexBuffer.ProjectionVertices[veId]);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void DrawFacet(IPainter painter, IDrawable drawable, RendererSettings rendererSettings, int faId, Stats stats)
        {
            var pixels = Rasterizer.RasterizeFacet(VertexBuffer, FrameBuffer, drawable, rendererSettings, faId, stats);
            if(pixels == null)
                return;
            var perPixelColors = CalculateShadingColors(painter, rendererSettings, drawable, VertexBuffer, pixels, faId);

            FrameBuffer.PutPixels(perPixelColors);
            stats.DrawnTriangleCount++;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected  List<(int x, int y, float z, ColorRGB color)> CalculateShadingColors(IPainter painter, RendererSettings rendererSettings,
            IDrawable drawable, VertexBuffer vertexBuffer, List<Vector3> pixels, int faId)
        {
            var perPixelColors = new List<(int x, int y, float z, ColorRGB color)>();
            var textureMaterial = drawable.Material as ITextureMaterial;
            var hasTexture = textureMaterial != null && textureMaterial.Texture != null;
            if(!hasTexture || !rendererSettings.ShowTextures)
                perPixelColors = painter.DrawTriangle(vertexBuffer, pixels, faId);
            else if(painter is GouraudPainter gouraudPainter)
            {
                perPixelColors = gouraudPainter.DrawTriangleTextured(textureMaterial.Texture,
                    vertexBuffer, pixels, faId, rendererSettings.LinearTextureFiltering);
            }

            return perPixelColors;
        }
    }
}
