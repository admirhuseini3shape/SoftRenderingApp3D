using SoftRenderingApp3D.Buffer;
using SoftRenderingApp3D.Painter;
using System;
using System.Numerics;

namespace SoftRenderingApp3D.Renderer
{
    public class SimpleRenderer : IRenderer
    {
        public SimpleRenderer(FrameBuffer frameBuffer)
        {
            FrameBuffer = frameBuffer;
        }

        public FrameBuffer FrameBuffer { get; }

        public int[] Render(RenderContext renderContext, IPainter painter)
        {
            var stats = renderContext.Stats;
            var frameBuffer = FrameBuffer;
            var viewMatrix = renderContext.Camera.ViewMatrix();
            var projectionMatrix = renderContext.Projection.ProjectionMatrix(frameBuffer.Width, frameBuffer.Height);
            var world = renderContext.World;
            var rendererSettings = renderContext.RendererSettings;

            if(world == null || rendererSettings == null)
                return Array.Empty<int>();

            stats.Clear();

            stats.PaintTime();
            frameBuffer.Clear();

            stats.CalcTime();

            // model => worldMatrix => world => viewMatrix => view => projectionMatrix => projection => toNdc => ndc => toScreen => screen


            // Allocate arrays to store transformed vertices
            using var allVertexBuffers = new AllVertexBuffers(world.Drawables);
            renderContext.AllVertexBuffers = allVertexBuffers;

            // This needs work, this is only for testing
            var textureIndex = rendererSettings.ActiveTexture % world.Textures.Count;
            var texture = world.Textures[textureIndex];

            var volumes = world.Drawables;
            var volumeCount = volumes.Count;
            for(var idxVolume = 0; idxVolume < volumeCount; idxVolume++)
            {
                var vertexBuffer = allVertexBuffers.VertexBuffer[idxVolume];
                var mesh = volumes[idxVolume].Mesh;

                //var worldMatrix = volume.WorldMatrix();
                //var modelViewMatrix = worldMatrix * viewMatrix;


                vertexBuffer.Drawable = volumes[idxVolume];
                vertexBuffer.TransformVertices(viewMatrix);
                //vertexBuffer.WorldMatrix = worldMatrix;

                stats.TotalTriangleCount += mesh.Facets.Count;

                var vertices = mesh.Vertices;
                var viewVertices = vertexBuffer.ViewVertices;

                // Transform and store vertices to View
                var vertexCount = vertices.Count;
                for(var idxVertex = 0; idxVertex < vertexCount; idxVertex++)
                {
                    viewVertices[idxVertex] = Vector3.Transform(vertices[idxVertex], viewMatrix);
                }

                var triangleCount = mesh.Facets.Count;
                for(var idxTriangle = 0; idxTriangle < triangleCount; idxTriangle++)
                {
                    var t = mesh.Facets[idxTriangle];

                    // Discard if behind far plane
                    if(t.IsBehindFarPlane(vertexBuffer))
                    {
                        stats.BehindViewTriangleCount++;
                        continue;
                    }

                    // Discard if back facing 
                    if(rendererSettings.BackFaceCulling && t.IsFacingBack(vertexBuffer))
                    {
                        stats.FacingBackTriangleCount++;
                        continue;
                    }

                    // Project in frustum
                    t.TransformProjection(vertexBuffer, projectionMatrix);

                    // Discard if outside view frustum
                    if(t.IsOutsideFrustum(vertexBuffer))
                    {
                        stats.OutOfViewTriangleCount++;
                        continue;
                    }

                    stats.PaintTime();

                    if(!rendererSettings.ShowTextures)
                    {
                        painter?.DrawTriangle(vertexBuffer, frameBuffer, idxTriangle);
                    }
                    else
                    {
                        if(painter != null && painter.GetType() == typeof(GouraudPainter))
                        {
                            var gouraudPainter = (GouraudPainter)painter;
                            gouraudPainter.DrawTriangleTextured(texture, vertexBuffer,frameBuffer, idxTriangle,
                                rendererSettings.LinearTextureFiltering);
                        }
                    }

                    stats.DrawnTriangleCount++;

                    stats.CalcTime();
                }
            }

            return frameBuffer.Screen;
        }
    }
}
