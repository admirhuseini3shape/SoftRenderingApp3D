using SoftRenderingApp3D.Buffer;
using SoftRenderingApp3D.DataStructures.Drawables;
using SoftRenderingApp3D.DataStructures.Materials;
using SoftRenderingApp3D.Painter;
using SoftRenderingApp3D.Rasterizers;
using SoftRenderingApp3D.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace SoftRenderingApp3D.Renderer
{
    public class Comparers
    {
        public WidthComparer Width { get; }
        public HeightComparer Length { get; }
        public DepthComparer Depth { get; }

        public Comparers()
        {
            Width = new WidthComparer();
            Length = new HeightComparer();
            Depth = new DepthComparer();
        }
    }
    public class WidthComparer : IComparer<(int FaId, Vector3 Midpoint)>
    {
        public int Compare((int FaId, Vector3 Midpoint) x, (int FaId, Vector3 Midpoint) y)
        {
            return (int)(x.Midpoint.X - y.Midpoint.X);
        }
    }

    public class HeightComparer : IComparer<(int FaId, Vector3 Midpoint)>
    {
        public int Compare((int FaId, Vector3 Midpoint) x, (int FaId, Vector3 Midpoint) y)
        {
            return (int)(x.Midpoint.Y - y.Midpoint.Y);
        }
    }
    public class DepthComparer : IComparer<(int FaId, Vector3 Midpoint)>
    {
        public int Compare((int FaId, Vector3 Midpoint) x, (int FaId, Vector3 Midpoint) y)
        {
            return (int)(x.Midpoint.Z - y.Midpoint.Z);
        }
    }

    public class SimpleRenderer : IRenderer
    {
        private (int iDrawable, int meshFaId)[] meshFacetIds;
        private (int FaId, Vector4 Midpoint)[] allFacetMidPoints;

        private void InitializeFacets(IReadOnlyList<Drawable> drawables, VertexBuffer vertexBuffer)
        {
            var allFacetsCount = drawables.Sum(x => x.Mesh.FacetCount);
            allFacetMidPoints = new (int FaId, Vector4 Midpoint)[allFacetsCount];
            meshFacetIds = new (int iDrawable, int meshFaId)[allFacetsCount];

            var allFacetIds = 0;
            for(var i = 0; i < drawables.Count; i++)
            {
                var mesh = drawables[i].Mesh;
                for(var meshFaId = 0; meshFaId < mesh.FacetCount; meshFaId++)
                {
                    allFacetMidPoints[allFacetIds] = (allFacetIds,
                        mesh.Facets[meshFaId].CalculateCentroid(vertexBuffer.ProjectionVertices));
                    meshFacetIds[allFacetIds] = (i, meshFaId);
                    allFacetIds++;
                }
            }
        }

        public int[] Render(AllVertexBuffers allVertexBuffers, FrameBuffer frameBuffer, IPainter painter,
            IList<IDrawable> drawables, Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix,
            RendererSettings rendererSettings)
        {
            if(drawables == null || painter == null || rendererSettings == null)
                return Array.Empty<int>();

            const int minFacetsForParallelization = 10000;
            var allTrianglesCount = drawables.Sum(x => x.Mesh.FacetCount);
            if(allTrianglesCount < minFacetsForParallelization)
                return RenderSequential(allVertexBuffers, frameBuffer, painter,
                    drawables, viewMatrix, projectionMatrix, rendererSettings);
            
            var stats = StatsSingleton.Instance;
            stats.Clear();

            frameBuffer.Clear();

            stats.calcSw.Restart();

            // model => worldMatrix => world => viewMatrix => view => projectionMatrix => projection => toNdc => ndc => toScreen => screen

            var drawablesPartitioner = new List<(int from, int to)>(drawables.Count);

            var drawablesCount = drawables.Count;
            //for(var iDrawable = 0; iDrawable < drawablesCount; iDrawable++)
            Parallel.For(0, drawablesCount, iDrawable =>
            {
                var vertexBuffer = allVertexBuffers.VertexBuffer[iDrawable];
                vertexBuffer.Clear();

                var drawable = drawables[iDrawable];
                vertexBuffer.Drawable = drawables[iDrawable];
                vertexBuffer.TransformVertices(viewMatrix);

                var triangleCount = drawable.Mesh.Facets.Count;
                stats.TotalTriangleCount += triangleCount;

                var vertices = drawable.Mesh.Vertices;
                var viewVertices = vertexBuffer.ViewVertices;

                // Transform and store vertices to View
                var vertexCount = vertices.Count;
                for(var veId = 0; veId < vertexCount; veId++)
                    viewVertices[veId] = viewMatrix.Transform(vertices[veId]);

                const int trianglesPerBatch = 1000;
                var batches = Partitioner.Create(0, triangleCount, trianglesPerBatch);
                //Parallel.ForEach(batches, range =>
                //{
                //for(var faId = range.Item1; faId < range.Item2; faId++)
                for(var faId = 0; faId < triangleCount; faId++)
                {
                    var facet = drawable.Mesh.Facets[faId];
                    facet.TransformProjection(vertexBuffer, projectionMatrix);

                    vertexBuffer.ScreenPointVertices[facet.I0] =
                        frameBuffer.ToScreen3(vertexBuffer.ProjectionVertices[facet.I0]);
                    vertexBuffer.ScreenPointVertices[facet.I1] =
                        frameBuffer.ToScreen3(vertexBuffer.ProjectionVertices[facet.I1]);
                    vertexBuffer.ScreenPointVertices[facet.I2] =
                        frameBuffer.ToScreen3(vertexBuffer.ProjectionVertices[facet.I2]);
                }
                // });
            });
            stats.calcSw.Stop();
            stats.paintSw.Restart();
            //for(var iDrawable = 0; iDrawable < drawablesCount; iDrawable++)
            Parallel.For(0, drawablesCount, iDrawable =>
                {
                    var drawable = drawables[iDrawable];
                    var vertexBuffer = allVertexBuffers.VertexBuffer[iDrawable];
                    //var zSortedFacets = drawable.Mesh.Facets
                    //.Select((fa, i) => new { FaId = i, zDepth = fa.CalculateZAverages(vertexBuffer.ProjectionVertices) })
                    //.ToList();
                    //zSortedFacets.Sort((x, y) => (int)(1000 * x.zDepth - 1000 * y.zDepth));

                    var triangleCount = drawable.Mesh.Facets.Count;

                    const int trianglesPerBatch = 15000;
                    var batches = Partitioner.Create(0, triangleCount, trianglesPerBatch);

                    Parallel.ForEach(batches, range =>
                    {
                        for(var faId = range.Item1; faId < range.Item2; faId++)
                        //for(var faId = 0; faId < triangleCount; faId++)
                        {
                            //var facetData = zSortedFacets[faId];
                            //if(facetData.zDepth < 0)
                            //    continue;

                            var facet = drawable.Mesh.Facets[faId];

                            // Discard if behind far plane
                            if(facet.IsBehindFarPlane(vertexBuffer))
                            {
                                stats.BehindViewTriangleCount++;
                                continue;
                                //return;
                            }

                            // Discard if back facing 
                            if(facet.IsFacingBack(vertexBuffer))
                            {
                                stats.FacingBackTriangleCount++;
                                if(rendererSettings.BackFaceCulling)
                                {
                                    continue;
                                    //return;
                                }
                            }

                            // Project in frustum
                            //facet.TransformProjection(vertexBuffer, projectionMatrix);

                            // Discard if outside view frustum
                            if(facet.IsOutsideFrustum(vertexBuffer))
                            {
                                stats.OutOfViewTriangleCount++;
                                continue;
                                //return;
                            }



                            var pixels = Rasterizer.GetPixels(vertexBuffer, frameBuffer, facet);
                            var perPixelColors = GetColors(frameBuffer, painter,
                            rendererSettings, drawable, vertexBuffer, pixels, faId);

                            frameBuffer.PutPixels(perPixelColors);
                            stats.DrawnTriangleCount++;


                        }
                    });
                });
            stats.paintSw.Stop();


            return frameBuffer.Screen;

        }

        public int[] RenderSequential(AllVertexBuffers allVertexBuffers, FrameBuffer frameBuffer, IPainter painter,
            IList<IDrawable> drawables, Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix,
            RendererSettings rendererSettings)
        {
            if(drawables == null || painter == null || rendererSettings == null)
                return Array.Empty<int>();

            var stats = StatsSingleton.Instance;
            stats.Clear();


            frameBuffer.Clear();

            stats.calcSw.Restart();

            // model => worldMatrix => world => viewMatrix => view => projectionMatrix => projection => toNdc => ndc => toScreen => screen


            var drawablesCount = drawables.Count;
            for(var iDrawable = 0; iDrawable < drawablesCount; iDrawable++)
            {
                var vertexBuffer = allVertexBuffers.VertexBuffer[iDrawable];
                vertexBuffer.Clear();

                var drawable = drawables[iDrawable];
                vertexBuffer.Drawable = drawables[iDrawable];
                vertexBuffer.TransformVertices(viewMatrix);

                var triangleCount = drawable.Mesh.Facets.Count;
                stats.TotalTriangleCount += triangleCount;

                var vertices = drawable.Mesh.Vertices;
                var viewVertices = vertexBuffer.ViewVertices;

                // Transform and store vertices to View
                var vertexCount = vertices.Count;
                for(var veId = 0; veId < vertexCount; veId++)
                    viewVertices[veId] = viewMatrix.Transform(vertices[veId]);

                const int trianglesPerBatch = 1000;
                var batches = Partitioner.Create(0, triangleCount, trianglesPerBatch);
                for(var faId = 0; faId < triangleCount; faId++)
                {
                    var facet = drawable.Mesh.Facets[faId];
                    facet.TransformProjection(vertexBuffer, projectionMatrix);

                    vertexBuffer.ScreenPointVertices[facet.I0] =
                        frameBuffer.ToScreen3(vertexBuffer.ProjectionVertices[facet.I0]);
                    vertexBuffer.ScreenPointVertices[facet.I1] =
                        frameBuffer.ToScreen3(vertexBuffer.ProjectionVertices[facet.I1]);
                    vertexBuffer.ScreenPointVertices[facet.I2] =
                        frameBuffer.ToScreen3(vertexBuffer.ProjectionVertices[facet.I2]);
                }
            }
            stats.calcSw.Stop();
            stats.paintSw.Restart();
            for(var iDrawable = 0; iDrawable < drawablesCount; iDrawable++)
            {
                var drawable = drawables[iDrawable];
                var vertexBuffer = allVertexBuffers.VertexBuffer[iDrawable];
                //var zSortedFacets = drawable.Mesh.Facets
                //.Select((fa, i) => new { FaId = i, zDepth = fa.CalculateZAverages(vertexBuffer.ProjectionVertices) })
                //.ToList();
                //zSortedFacets.Sort((x, y) => (int)(1000 * x.zDepth - 1000 * y.zDepth));

                var triangleCount = drawable.Mesh.Facets.Count;

                const int trianglesPerBatch = 15000;
                var batches = Partitioner.Create(0, triangleCount, trianglesPerBatch);

                for(var faId = 0; faId < triangleCount; faId++)
                {
                    //var facetData = zSortedFacets[faId];
                    //if(facetData.zDepth < 0)
                    //    continue;

                    var facet = drawable.Mesh.Facets[faId];

                    // Discard if behind far plane
                    if(facet.IsBehindFarPlane(vertexBuffer))
                    {
                        stats.BehindViewTriangleCount++;
                        continue;
                        //return;
                    }

                    // Discard if back facing 
                    if(facet.IsFacingBack(vertexBuffer))
                    {
                        stats.FacingBackTriangleCount++;
                        if(rendererSettings.BackFaceCulling)
                        {
                            continue;
                            //return;
                        }
                    }

                    // Project in frustum
                    //facet.TransformProjection(vertexBuffer, projectionMatrix);

                    // Discard if outside view frustum
                    if(facet.IsOutsideFrustum(vertexBuffer))
                    {
                        stats.OutOfViewTriangleCount++;
                        continue;
                        //return;
                    }

                    var pixels = Rasterizer.GetPixels(vertexBuffer, frameBuffer, facet);
                    var perPixelColors = GetColors(frameBuffer, painter,
                    rendererSettings, drawable, vertexBuffer, pixels, faId);

                    frameBuffer.PutPixels(perPixelColors);
                    stats.DrawnTriangleCount++;
                }
            }
            stats.paintSw.Stop();


            return frameBuffer.Screen;
        }

        private static List<(int x, int y, float z, ColorRGB color)> GetColors(FrameBuffer frameBuffer, IPainter painter, RendererSettings rendererSettings,
            IDrawable drawable, VertexBuffer vertexBuffer, List<Vector3> pixels, int faId)
        {
            var perPixelColors = new List<(int x, int y, float z, ColorRGB color)>();
            var textureMaterial = drawable.Material as ITextureMaterial;
            var hasTexture = textureMaterial != null && textureMaterial.Texture != null;
            if(!hasTexture || !rendererSettings.ShowTextures)
                perPixelColors = painter.DrawTriangle(vertexBuffer, frameBuffer, pixels, faId);
            else if(painter is GouraudPainter gouraudPainter)
            {
                perPixelColors = gouraudPainter.DrawTriangleTextured(textureMaterial.Texture,
                    vertexBuffer, frameBuffer, pixels, faId, rendererSettings.LinearTextureFiltering);
            }

            return perPixelColors;
        }
    }
}
