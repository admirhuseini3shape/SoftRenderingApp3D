using SoftRenderingApp3D.Buffer;
using SoftRenderingApp3D.Painter;
using System;
using System.Numerics;

namespace SoftRenderingApp3D.Renderer
{
    public class SimpleRenderer : IRenderer
    {
        public RenderContext RenderContext { get; set; }

        public IPainter Painter { get; set; }

        public int[] Render()
        {
            var stats = RenderContext.Stats;
            var surface = RenderContext.Surface;
            var camera = RenderContext.Camera;
            var projection = RenderContext.Projection;
            var world = RenderContext.World;
            var rendererSettings = RenderContext.RendererSettings;

            if(surface == null || camera == null || projection == null ||
               world == null || rendererSettings == null)
                return Array.Empty<int>();

            stats.Clear();

            stats.PaintTime();
            surface.Clear();

            stats.CalcTime();

            // model => worldMatrix => world => viewMatrix => view => projectionMatrix => projection => toNdc => ndc => toScreen => screen

            var viewMatrix = camera.ViewMatrix();
            var projectionMatrix = projection.ProjectionMatrix(surface.Width, surface.Height);

            // Allocate arrays to store transformed vertices
            using var worldBuffer = new WorldBuffer(world);
            RenderContext.WorldBuffer = worldBuffer;

            // This needs work, this is only for testing
            var textureIndex = rendererSettings.activeTexture % world.Textures.Count;
            var texture = world.Textures[textureIndex];

            var volumes = world.Drawables;
            var volumeCount = volumes.Count;
            for(var idxVolume = 0; idxVolume < volumeCount; idxVolume++)
            {
                var vbx = worldBuffer.VertexBuffer[idxVolume];
                var mesh = volumes[idxVolume].Mesh;

                //var worldMatrix = volume.WorldMatrix();
                //var modelViewMatrix = worldMatrix * viewMatrix;


                vbx.Drawable = volumes[idxVolume];
                vbx.TransformVertices(viewMatrix);
                //vbx.WorldMatrix = worldMatrix;

                stats.TotalTriangleCount += mesh.Facets.Count;

                var vertices = mesh.Vertices;
                var viewVertices = vbx.ViewVertices;

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
                    if(t.IsBehindFarPlane(vbx))
                    {
                        stats.BehindViewTriangleCount++;
                        continue;
                    }

                    // Discard if back facing 
                    if(rendererSettings.BackFaceCulling && t.IsFacingBack(vbx))
                    {
                        stats.FacingBackTriangleCount++;
                        continue;
                    }

                    // Project in frustum
                    t.TransformProjection(vbx, projectionMatrix);

                    // Discard if outside view frustum
                    if(t.IsOutsideFrustum(vbx))
                    {
                        stats.OutOfViewTriangleCount++;
                        continue;
                    }

                    stats.PaintTime();

                    if(!rendererSettings.ShowTextures)
                    {
                        Painter?.DrawTriangle(vbx, idxTriangle);
                    }
                    else
                    {
                        if(Painter != null && Painter.GetType() == typeof(GouraudPainter))
                        {
                            var painter = (GouraudPainter)Painter;
                            painter.DrawTriangleTextured(texture, vbx, idxTriangle,
                                rendererSettings.LiearTextureFiltering);
                        }
                    }

                    stats.DrawnTriangleCount++;

                    stats.CalcTime();
                }
            }

            return surface.Screen;
        }
    }
}
