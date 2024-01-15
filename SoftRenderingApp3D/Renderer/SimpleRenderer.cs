using SoftRenderingApp3D.Buffer;
using SoftRenderingApp3D.DataStructures.Drawables;
using SoftRenderingApp3D.DataStructures.Materials;
using SoftRenderingApp3D.Painter;
using SoftRenderingApp3D.Rasterizers;
using SoftRenderingApp3D.Utils;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace SoftRenderingApp3D.Renderer
{
    public class SimpleRenderer : IRenderer
    {
        public int[] Render(VertexBuffer vertexBuffer, FrameBuffer frameBuffer, IPainter painter,
            IDrawable drawable, Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix, RendererSettings rendererSettings)
        {
            if(drawable == null || painter == null || rendererSettings == null || drawable.Mesh.FacetCount == 0)
            {
                frameBuffer.Clear();
                return frameBuffer.Screen;
            }

            const int minFacetsForParallelization = 10000;
            return drawable.Mesh.FacetCount < minFacetsForParallelization
                ? RenderSequential(vertexBuffer, frameBuffer, painter, drawable, viewMatrix, projectionMatrix,
                    rendererSettings)
                : RenderParallel(vertexBuffer, frameBuffer, painter, drawable, viewMatrix, projectionMatrix,
                    rendererSettings);
        }

        public int[] RenderParallel(VertexBuffer vertexBuffer, FrameBuffer frameBuffer, IPainter painter,
            IDrawable drawable, Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix, RendererSettings rendererSettings)
        {
            if(drawable == null || painter == null || rendererSettings == null || drawable.Mesh.FacetCount == 0)
            {
                frameBuffer.Clear();
                return frameBuffer.Screen;
            }

            var stats = StatsSingleton.Instance;
            stats.Clear();

            frameBuffer.Clear();

            UpdateVertexBufferParallel(vertexBuffer, frameBuffer, drawable, viewMatrix, projectionMatrix, stats);
            stats.paintSw.Restart();

            //var zSortedFacets = drawable.Mesh.Facets
            //.Select((fa, i) => new { FaId = i, zDepth = fa.CalculateZAverages(vertexBuffer.ProjectionVertices) })
            //.ToList();
            //zSortedFacets.Sort((x, y) => (int)(1000 * x.zDepth - 1000 * y.zDepth));


            DrawFacetsParallel(vertexBuffer, frameBuffer, painter, drawable, rendererSettings, stats);
            stats.paintSw.Stop();

            return frameBuffer.Screen;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void DrawFacetsParallel(VertexBuffer vertexBuffer, FrameBuffer frameBuffer, IPainter painter,
            IDrawable drawable, RendererSettings rendererSettings, Stats stats)
        {
            Parallel.ForEach(Partitioner.Create(0, drawable.Mesh.FacetCount), range =>
            {
                for(var faId = range.Item1; faId < range.Item2; faId++)
                {
                    //var facetData = zSortedFacets[faId];
                    //if(facetData.zDepth < 0)
                    //    continue;

                    DrawFacet(vertexBuffer, frameBuffer, painter, drawable, rendererSettings, faId, stats);
                }
            });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void UpdateVertexBufferParallel(VertexBuffer vertexBuffer, FrameBuffer frameBuffer, IDrawable drawable,
            Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix, Stats stats)
        {
            stats.calcSw.Restart();
            vertexBuffer.Clear();

            // model => worldMatrix => world => viewMatrix => view => projectionMatrix => projection => toNdc => ndc => toScreen => screen

            // Transform and store vertices to View
            vertexBuffer.Drawable = drawable;

            UpdateVertexBuffersParallel(vertexBuffer, frameBuffer, viewMatrix, projectionMatrix);
            var facetsCount = drawable.Mesh.Facets.Count;
            stats.TotalTriangleCount += facetsCount;
            
            stats.calcSw.Stop();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void UpdateVertexBuffersParallel(VertexBuffer vertexBuffer, FrameBuffer frameBuffer,
            Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix)
        {
            Parallel.ForEach(Partitioner.Create(0, vertexBuffer.Drawable.Mesh.VertexCount), range =>
            {
                for (var veId = range.Item1; veId < range.Item2; veId++)
                {
                    UpdateVertexBuffer(vertexBuffer, frameBuffer, viewMatrix, projectionMatrix, veId);
                }
            });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void UpdateVertexBuffer(VertexBuffer vertexBuffer, FrameBuffer frameBuffer, Matrix4x4 viewMatrix,
            Matrix4x4 projectionMatrix, int veId)
        {
            vertexBuffer.WorldVertices[veId] = viewMatrix.Transform(vertexBuffer.Drawable.Mesh.Vertices[veId]);
            vertexBuffer.WorldVertexNormals[veId] =
                viewMatrix.TransformWithoutTranslation(vertexBuffer.Drawable.Mesh.VertexNormals[veId]);
            vertexBuffer.ViewVertices[veId] = viewMatrix.Transform(vertexBuffer.Drawable.Mesh.Vertices[veId]);
            vertexBuffer.ProjectionVertices[veId] = Vector4.Transform(vertexBuffer.ViewVertices[veId], projectionMatrix);
            vertexBuffer.ScreenPointVertices[veId] = frameBuffer.ToScreen3(vertexBuffer.ProjectionVertices[veId]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int[] RenderSequential(VertexBuffer vertexBuffer, FrameBuffer frameBuffer, IPainter painter,
            IDrawable drawable, Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix,
            RendererSettings rendererSettings)
        {
            if(drawable == null || painter == null || rendererSettings == null || drawable.Mesh.FacetCount == 0)
            {
                frameBuffer.Clear();
                return frameBuffer.Screen;
            }

            var stats = StatsSingleton.Instance;
            stats.Clear();


            frameBuffer.Clear();

            UpdateVertexBufferSequential(vertexBuffer, frameBuffer, drawable, viewMatrix, projectionMatrix, stats);
            stats.paintSw.Restart();

            //var zSortedFacets = drawable.Mesh.Facets
            //.Select((fa, i) => new { FaId = i, zDepth = fa.CalculateZAverages(vertexBuffer.ProjectionVertices) })
            //.ToList();
            //zSortedFacets.Sort((x, y) => (int)(1000 * x.zDepth - 1000 * y.zDepth));

            DrawFacetsSequential(vertexBuffer, frameBuffer, painter, drawable, rendererSettings, stats);
            stats.paintSw.Stop();


            return frameBuffer.Screen;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void DrawFacetsSequential(VertexBuffer vertexBuffer, FrameBuffer frameBuffer, IPainter painter,
            IDrawable drawable, RendererSettings rendererSettings, Stats stats)
        {
            for(var faId = 0; faId < drawable.Mesh.FacetCount; faId++)
            {
                //var facetData = zSortedFacets[faId];
                //if(facetData.zDepth < 0)
                //    continue;

                DrawFacet(vertexBuffer, frameBuffer, painter, drawable, rendererSettings, faId, stats);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void DrawFacet(VertexBuffer vertexBuffer, FrameBuffer frameBuffer, IPainter painter, IDrawable drawable,
            RendererSettings rendererSettings, int faId, Stats stats)
        {
            var pixels = Rasterizer.RasterizeFacet(vertexBuffer, frameBuffer, drawable, rendererSettings, faId, stats);
            if(pixels == null)
                return;
            var perPixelColors = CalculateShadingColors(painter, rendererSettings, drawable, vertexBuffer, pixels, faId);

            frameBuffer.PutPixels(perPixelColors);
            stats.DrawnTriangleCount++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void UpdateVertexBufferSequential(VertexBuffer vertexBuffer, FrameBuffer frameBuffer, IDrawable drawable,
            Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix, Stats stats)
        {
            stats.calcSw.Restart();

            // model => worldMatrix => world => viewMatrix => view => projectionMatrix => projection => toNdc => ndc => toScreen => screen

            vertexBuffer.Clear();

            // Transform and store vertices to View
            vertexBuffer.Drawable = drawable;

            UpdateVertexBuffersSequential(vertexBuffer, frameBuffer, viewMatrix, projectionMatrix);

            stats.calcSw.Stop();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void UpdateVertexBuffersSequential(VertexBuffer vertexBuffer, FrameBuffer frameBuffer,
            Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix)
        {
            for (var veId = 0; veId < vertexBuffer.Drawable.Mesh.Vertices.Count; veId++)
            {
                UpdateVertexBuffer(vertexBuffer, frameBuffer, viewMatrix, projectionMatrix, veId);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static List<(int x, int y, float z, ColorRGB color)> CalculateShadingColors(IPainter painter, RendererSettings rendererSettings,
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
