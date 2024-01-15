using SoftRenderingApp3D.Buffer;
using SoftRenderingApp3D.DataStructures.Drawables;
using SoftRenderingApp3D.DataStructures.Materials;
using SoftRenderingApp3D.Painter;
using SoftRenderingApp3D.Rasterizers;
using SoftRenderingApp3D.Utils;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Numerics;
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

            stats.calcSw.Restart();

            // model => worldMatrix => world => viewMatrix => view => projectionMatrix => projection => toNdc => ndc => toScreen => screen

            vertexBuffer.Clear();

            vertexBuffer.Drawable = drawable;
            vertexBuffer.TransformVertices(viewMatrix);

            var triangleCount = drawable.Mesh.Facets.Count;
            stats.TotalTriangleCount += triangleCount;

            var vertices = drawable.Mesh.Vertices;
            var viewVertices = vertexBuffer.ViewVertices;

            // Transform and store vertices to View
            var vertexCount = vertices.Count;
            for(var veId = 0; veId < vertexCount; veId++)
                viewVertices[veId] = viewMatrix.Transform(vertices[veId]);

            var batches = Partitioner.Create(0, triangleCount);
            Parallel.ForEach(batches, range =>
            {
                for(var faId = range.Item1; faId < range.Item2; faId++)
                //for(var faId = 0; faId < triangleCount; faId++)
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
            });
            stats.calcSw.Stop();
            stats.paintSw.Restart();

            //var zSortedFacets = drawable.Mesh.Facets
            //.Select((fa, i) => new { FaId = i, zDepth = fa.CalculateZAverages(vertexBuffer.ProjectionVertices) })
            //.ToList();
            //zSortedFacets.Sort((x, y) => (int)(1000 * x.zDepth - 1000 * y.zDepth));


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
                    }

                    // Discard if back facing 
                    if(facet.IsFacingBack(vertexBuffer))
                    {
                        stats.FacingBackTriangleCount++;
                        if(rendererSettings.BackFaceCulling)
                        {
                            continue;
                        }
                    }

                    // Project in frustum
                    //facet.TransformProjection(vertexBuffer, projectionMatrix);

                    // Discard if outside view frustum
                    if(facet.IsOutsideFrustum(vertexBuffer))
                    {
                        stats.OutOfViewTriangleCount++;
                        continue;
                    }

                    var pixels = Rasterizer.GetPixels(vertexBuffer, frameBuffer, facet);
                    var perPixelColors = CalculateShadingColors(frameBuffer, painter,
                    rendererSettings, drawable, vertexBuffer, pixels, faId);

                    frameBuffer.PutPixels(perPixelColors);
                    stats.DrawnTriangleCount++;
                }
            });
            stats.paintSw.Stop();


            return frameBuffer.Screen;

        }

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

            stats.calcSw.Restart();

            // model => worldMatrix => world => viewMatrix => view => projectionMatrix => projection => toNdc => ndc => toScreen => screen

            vertexBuffer.Clear();

            vertexBuffer.Drawable = drawable;
            vertexBuffer.TransformVertices(viewMatrix);

            var triangleCount = drawable.Mesh.FacetCount;

            stats.TotalTriangleCount += triangleCount;

            var vertices = drawable.Mesh.Vertices;
            var viewVertices = vertexBuffer.ViewVertices;

            // Transform and store vertices to View
            var vertexCount = vertices.Count;
            for(var veId = 0; veId < vertexCount; veId++)
                viewVertices[veId] = viewMatrix.Transform(vertices[veId]);

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

            stats.calcSw.Stop();
            stats.paintSw.Restart();

            //var zSortedFacets = drawable.Mesh.Facets
            //.Select((fa, i) => new { FaId = i, zDepth = fa.CalculateZAverages(vertexBuffer.ProjectionVertices) })
            //.ToList();
            //zSortedFacets.Sort((x, y) => (int)(1000 * x.zDepth - 1000 * y.zDepth));

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
                }

                // Discard if back facing 
                if(facet.IsFacingBack(vertexBuffer))
                {
                    stats.FacingBackTriangleCount++;
                    if(rendererSettings.BackFaceCulling)
                    {
                        continue;
                    }
                }

                // Project in frustum
                //facet.TransformProjection(vertexBuffer, projectionMatrix);

                // Discard if outside view frustum
                if(facet.IsOutsideFrustum(vertexBuffer))
                {
                    stats.OutOfViewTriangleCount++;
                    continue;
                }

                var pixels = Rasterizer.GetPixels(vertexBuffer, frameBuffer, facet);
                var perPixelColors = CalculateShadingColors(frameBuffer, painter,
                rendererSettings, drawable, vertexBuffer, pixels, faId);

                frameBuffer.PutPixels(perPixelColors);
                stats.DrawnTriangleCount++;
            }
            stats.paintSw.Stop();


            return frameBuffer.Screen;
        }

        private static List<(int x, int y, float z, ColorRGB color)> CalculateShadingColors(FrameBuffer frameBuffer, IPainter painter, RendererSettings rendererSettings,
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
