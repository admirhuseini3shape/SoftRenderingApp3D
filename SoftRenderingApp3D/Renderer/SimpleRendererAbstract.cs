using SoftRenderingApp3D.Buffer;
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
    public abstract class SimpleRendererAbstract : IRenderer
    {
        public VertexBuffer VertexBuffer { get; }
        public FrameBuffer FrameBuffer { get; }
        protected readonly Stats Stats;
        protected readonly Rasterizer Rasterizer;

        protected SimpleRendererAbstract(VertexBuffer vertexBuffer, FrameBuffer frameBuffer)
        {
            VertexBuffer = vertexBuffer;
            FrameBuffer = frameBuffer;
            Rasterizer = new Rasterizer(vertexBuffer, frameBuffer);
            var count = vertexBuffer?.Drawable?.Mesh?.FacetCount ?? 0;
            Stats = StatsSingleton.Instance;
        }

        public int[] Render(IPainterProvider painterProvider, Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix, RendererSettings rendererSettings)
        {
            FrameBuffer.Clear();
            var drawable = VertexBuffer.Drawable;
            if(drawable == null || painterProvider == null || rendererSettings == null || drawable.Mesh.FacetCount == 0)
            {
                return FrameBuffer.Screen;
            }

            Stats.Clear();

            UpdateVertexBuffer(viewMatrix, projectionMatrix);
            Stats.paintSw.Restart();

            RasterizeFacets(drawable.Mesh.Facets, rendererSettings);

            var painter = painterProvider.GetPainter(drawable.Material, VertexBuffer, FrameBuffer, rendererSettings);
            DrawPixels(painter, rendererSettings);
            Stats.paintSw.Stop();

            return FrameBuffer.Screen;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract void RasterizeFacets(IReadOnlyList<Facet> facets, RendererSettings rendererSettings);

        private void DrawPixels(IPainter painter, RendererSettings rendererSettings)
        {
            painter.ClearBuffers();
            var pixelCount = FrameBuffer.Screen.Length;
            var pixelsToDraw = new List<int>(pixelCount);
            for(int i = 0; i < pixelCount; i++)
            {
                if(FrameBuffer.FacetIdsForPixels[i] == FrameBuffer.NoFacet)
                    continue;
                pixelsToDraw.Add(i);
            }
            if(pixelsToDraw.Count == 0)
                return;

            Parallel.ForEach(Partitioner.Create(0, pixelsToDraw.Count),
                new ParallelOptions { TaskScheduler = TaskScheduler.Current },
                range =>
                {
                    for(var i = range.Item1; i < range.Item2; i++)
                    {
                        var iPixel = pixelsToDraw[i];
                        var faId = FrameBuffer.FacetIdsForPixels[iPixel];
                        if(faId == FrameBuffer.NoFacet)
                            continue;

                        painter.UpdateBuffers(faId);

                        //var index = x + y * Width;
                        var x = iPixel % FrameBuffer.Width;
                        var y = iPixel / FrameBuffer.Width;

                        var color = painter.DrawPixel(x, y, rendererSettings);

                        FrameBuffer.PutPixel(x, y, color);
                    }
                });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UpdateVertexBuffer(Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix)
        {
            Stats.calcSw.Restart();
            VertexBuffer.Clear();

            // model => worldMatrix => world => viewMatrix => view => projectionMatrix => projection => toNdc => ndc => toScreen => screen

            // Transform and store vertices to View
            TransformVertexBuffers(viewMatrix, projectionMatrix);
            var facetsCount = VertexBuffer.Drawable.Mesh.FacetCount;
            Stats.TotalTriangleCount += facetsCount;

            Stats.calcSw.Stop();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract void TransformVertexBuffers(Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TransformVertex(Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix, int veId)
        {
            VertexBuffer.WorldVertices[veId] = viewMatrix.Transform(VertexBuffer.Drawable.Mesh.Vertices[veId]);
            VertexBuffer.WorldVertexNormals[veId] =
                viewMatrix.TransformWithoutTranslation(VertexBuffer.Drawable.Mesh.VertexNormals[veId]);
            VertexBuffer.ViewVertices[veId] = viewMatrix.Transform(VertexBuffer.Drawable.Mesh.Vertices[veId]);
            VertexBuffer.ProjectionVertices[veId] = Vector4.Transform(VertexBuffer.ViewVertices[veId], projectionMatrix);
            VertexBuffer.ScreenPointVertices[veId] = FrameBuffer.ToScreen3(VertexBuffer.ProjectionVertices[veId]);
        }
    }
}
