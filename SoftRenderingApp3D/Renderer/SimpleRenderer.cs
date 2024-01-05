using SoftRenderingApp3D.Buffer;
using SoftRenderingApp3D.DataStructures.Drawables;
using SoftRenderingApp3D.DataStructures.Materials;
using SoftRenderingApp3D.Painter;
using SoftRenderingApp3D.Utils;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace SoftRenderingApp3D.Renderer
{
    public class SimpleRenderer : IRenderer
    {
        public int[] Render(AllVertexBuffers allVertexBuffers, FrameBuffer frameBuffer, IPainter painter,
            IList<IDrawable> drawables, Stats stats, Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix,
            RendererSettings rendererSettings)
        {
            if(drawables == null || painter == null || rendererSettings == null)
                return Array.Empty<int>();

            stats.Clear();

            stats.PaintTime();
            frameBuffer.Clear();

            stats.CalcTime();

            // model => worldMatrix => world => viewMatrix => view => projectionMatrix => projection => toNdc => ndc => toScreen => screen


            // Allocate arrays to store transformed vertices
            //using var allVertexBuffers = new AllVertexBuffers(world.Drawables);

            var drawablesCount = drawables.Count;
            for(var iDrawable = 0; iDrawable < drawablesCount; iDrawable++)
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

                    // Discard if behind far plane
                    if(facet.IsBehindFarPlane(vertexBuffer))
                    {
                        stats.BehindViewTriangleCount++;
                        continue;
                    }

                    // Discard if back facing 
                    if(rendererSettings.BackFaceCulling && facet.IsFacingBack(vertexBuffer))
                    {
                        stats.FacingBackTriangleCount++;
                        continue;
                    }

                    // Project in frustum
                    facet.TransformProjection(vertexBuffer, projectionMatrix);

                    // Discard if outside view frustum
                    if(facet.IsOutsideFrustum(vertexBuffer))
                    {
                        stats.OutOfViewTriangleCount++;
                        continue;
                    }

                    stats.PaintTime();

                    var textureMaterial = drawable.Material as ITextureMaterial;
                    var hasTexture = textureMaterial != null && textureMaterial.Texture != null;
                    if(!hasTexture || !rendererSettings.ShowTextures)
                        painter.DrawTriangle(vertexBuffer, frameBuffer, faId);
                    else if(painter is GouraudPainter gouraudPainter)
                    {
                        gouraudPainter.DrawTriangleTextured(textureMaterial.Texture,
                            vertexBuffer, frameBuffer, faId, rendererSettings.LinearTextureFiltering);
                    }

                    stats.DrawnTriangleCount++;

                    stats.CalcTime();
                }
            }

            return frameBuffer.Screen;
        }
    }
}
