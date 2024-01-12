using SoftRenderingApp3D.Buffer;
using SoftRenderingApp3D.DataStructures.Drawables;
using SoftRenderingApp3D.DataStructures.Materials;
using SoftRenderingApp3D.Painter;
using SoftRenderingApp3D.Rasterizers;
using SoftRenderingApp3D.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace SoftRenderingApp3D.Renderer
{
    public class SimpleRenderer : IRenderer
    {
        public int[] Render(AllVertexBuffers allVertexBuffers, FrameBuffer frameBuffer, IPainter painter,
            IList<IDrawable> drawables, Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix,
            RendererSettings rendererSettings)
        {
            if(drawables == null || painter == null || rendererSettings == null)
                return Array.Empty<int>();

            var stats = StatsSingleton.Instance;
            stats.Clear();

            stats.PaintTime();
            frameBuffer.Clear();

            stats.CalcTime();

            // model => worldMatrix => world => viewMatrix => view => projectionMatrix => projection => toNdc => ndc => toScreen => screen

            var drawablesCount = drawables.Count;
            for(var iDrawable = 0; iDrawable < drawablesCount; iDrawable++)
            //Parallel.For(0, drawablesCount, iDrawable =>
            {
                var vertexBuffer = allVertexBuffers.VertexBuffer[iDrawable];
                vertexBuffer.Clear();

                var drawable = drawables[iDrawable];
                vertexBuffer.Drawable = drawables[iDrawable];
                vertexBuffer.TransformVertices(viewMatrix);

                stats.TotalTriangleCount += drawable.Mesh.Facets.Count;

                var vertices = drawable.Mesh.Vertices;
                var viewVertices = vertexBuffer.ViewVertices;

                // Transform and store vertices to View
                var vertexCount = vertices.Count;
                for(var veId = 0; veId < vertexCount; veId++)
                    viewVertices[veId] = viewMatrix.Transform(vertices[veId]);

                var triangleCount = drawable.Mesh.Facets.Count;
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
                var zSortedFacets = drawable.Mesh.Facets
                    .Select((fa, i) => new { FaId = i, zDepth = fa.CalculateZAverages(vertexBuffer.ProjectionVertices) })
                    .ToList();
                zSortedFacets.Sort((x, y) => (int)(1000 * x.zDepth - 1000 * y.zDepth));
                for(var faId = 0; faId < triangleCount; faId++)
                //Parallel.For(0, triangleCount, faId =>
                {
                    var facetData = zSortedFacets[faId];
                    //if(facetData.zDepth < 0)
                    //    continue;

                    var facet = drawable.Mesh.Facets[facetData.FaId];

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

                    stats.PaintTime();

                    var pixels = Rasterizer.GetPixels(vertexBuffer, frameBuffer, facet);
                    var perPixelColors = GetColors(frameBuffer, painter,
                        rendererSettings, drawable, vertexBuffer, pixels, facetData.FaId);

                    frameBuffer.PutPixels(perPixelColors);
                    stats.DrawnTriangleCount++;

                    stats.CalcTime();
                }//);
            }//);

            // frameBuffer.Dispose();
            
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
