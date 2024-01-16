using SoftRenderingApp3D.Buffer;
using SoftRenderingApp3D.DataStructures.Drawables;
using SoftRenderingApp3D.Painter;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace SoftRenderingApp3D.Renderer
{
    public class SimpleRenderer : IRenderer
    {
        public VertexBuffer VertexBuffer { get; }
        public FrameBuffer FrameBuffer { get; }

        private readonly SimpleRendererAbstract sequentialRenderer;
        private readonly SimpleRendererAbstract parallelRenderer;

        public SimpleRenderer(VertexBuffer vertexBuffer, FrameBuffer frameBuffer)
        {
            VertexBuffer = vertexBuffer;
            FrameBuffer = frameBuffer;
            sequentialRenderer = new SimpleRendererSequential(vertexBuffer, frameBuffer);
            parallelRenderer = new SimpleRendererParallel(vertexBuffer, frameBuffer);
        }

        public int[] Render(IPainter painter, Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix, RendererSettings rendererSettings)
        {
            var drawable = VertexBuffer.Drawable;
            if(drawable == null || painter == null || rendererSettings == null || drawable.Mesh.FacetCount == 0)
            {
                FrameBuffer.Clear();
                return FrameBuffer.Screen;
            }


            return GetRenderer(drawable.Mesh.FacetCount).Render(painter, viewMatrix, projectionMatrix, rendererSettings);
        }

        private SimpleRendererAbstract GetRenderer(int facetCount)
        {
            const int minFacetsForParallelization = 10000;
            return facetCount < minFacetsForParallelization ? sequentialRenderer : parallelRenderer;
        }
    }

    public class SimpleRendererParallel : SimpleRendererAbstract
    {
        public SimpleRendererParallel(VertexBuffer vertexBuffer, FrameBuffer frameBuffer)
            : base(vertexBuffer, frameBuffer) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void DrawFacets(IPainter painter, IDrawable drawable, RendererSettings rendererSettings)
        {
            var initialPixelCount = FrameBuffer.Screen.Length / 64;
            Parallel.ForEach(Partitioner.Create(0, drawable.Mesh.FacetCount),
                new ParallelOptions { TaskScheduler = TaskScheduler.Current },
                () => new List<(int x, int y, float z, ColorRGB color)>(initialPixelCount),
                (range, state, local) =>
            {
                for(var faId = range.Item1; faId < range.Item2; faId++)
                {
                    //var facetData = zSortedFacets[faId];
                    //if(facetData.zDepth < 0)
                    //    continue;

                    var perPixelColors = DrawFacet(painter, drawable, rendererSettings, faId);
                    if(perPixelColors == null)
                        continue;
                    local.AddRange(perPixelColors);
                    stats.DrawnTriangleCount++;
                }

                return local;
            },
                local =>
                {
                    lock(FrameBuffer)
                        FrameBuffer.PutPixels(local);
                });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void TransformVertexBuffers(Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix)
        {
            Parallel.ForEach(Partitioner.Create(0, VertexBuffer.Drawable.Mesh.VertexCount),
                new ParallelOptions { TaskScheduler = TaskScheduler.Current },
                (range) =>
            {
                for(var veId = range.Item1; veId < range.Item2; veId++)
                {
                    TransformVertex(viewMatrix, projectionMatrix, veId);
                }
            });
        }
    }

    public class SimpleRendererSequential : SimpleRendererAbstract
    {
        public SimpleRendererSequential(VertexBuffer vertexBuffer, FrameBuffer frameBuffer)
            : base(vertexBuffer, frameBuffer) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void DrawFacets(IPainter painter, IDrawable drawable, RendererSettings rendererSettings)
        {
            for(var faId = 0; faId < drawable.Mesh.FacetCount; faId++)
            {
                //var facetData = zSortedFacets[faId];
                //if(facetData.zDepth < 0)
                //    continue;

                var perPixelColors = DrawFacet(painter, drawable, rendererSettings, faId);
                if(perPixelColors == null)
                    continue;

                FrameBuffer.PutPixels(perPixelColors);
                stats.DrawnTriangleCount++;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void TransformVertexBuffers(Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix)
        {
            for(var veId = 0; veId < VertexBuffer.Drawable.Mesh.Vertices.Count; veId++)
            {
                TransformVertex(viewMatrix, projectionMatrix, veId);
            }
        }
    }
}
