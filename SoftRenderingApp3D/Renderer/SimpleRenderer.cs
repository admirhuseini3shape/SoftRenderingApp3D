using SoftRenderingApp3D.Buffer;
using SoftRenderingApp3D.Painter;
using System.Numerics;

namespace SoftRenderingApp3D.Renderer {

    public class SimpleRenderer : IRenderer {
        private RenderContext renderContext;

        public RenderContext RenderContext {
            get => renderContext;
            set {
                renderContext = value;
            }
        }

        public IPainter Painter { get; set; }

        public int[] Render() {
            var stats = RenderContext.Stats;
            var surface = RenderContext.Surface;
            var camera = RenderContext.Camera;
            var projection = RenderContext.Projection;
            var world = RenderContext.World;
            var rendererSettings = RenderContext.RendererSettings;

            stats.Clear();

            stats.PaintTime();
            surface.Clear();

            stats.CalcTime();

            // model => worldMatrix => world => viewMatrix => view => projectionMatrix => projection => toNdc => ndc => toScreen => screen

            var viewMatrix = camera.ViewMatrix();
            var projectionMatrix = projection.ProjectionMatrix(surface.Width, surface.Height);
            var world2Projection = viewMatrix * projectionMatrix;

            // Allocate arrays to store transformed vertices
            using var worldBuffer = new WorldBuffer(world);
            renderContext.WorldBuffer = worldBuffer;

            // This needs work, this is only for testing
            var textureIndex = rendererSettings.activeTexture % world.Textures.Count;
            var texture = world.Textures[textureIndex];

            var volumes = world.Volumes;
            var volumeCount = volumes.Count;
            for(var idxVolume = 0; idxVolume < volumeCount; idxVolume++) {

                var vbx = worldBuffer.VertexBuffer[idxVolume];
                var volume = volumes[idxVolume];

                var worldMatrix = volume.WorldMatrix();
                var modelViewMatrix = worldMatrix * viewMatrix;


                vbx.Volume = volume;
                vbx.WorldMatrix = worldMatrix;

                stats.TotalTriangleCount += volume.Triangles.Length;

                var vertices = volume.Vertices;
                var viewVertices = vbx.ViewVertices;

                // Transform and store vertices to View
                var vertexCount = vertices.Length;
                for(var idxVertex = 0; idxVertex < vertexCount; idxVertex++) {
                    viewVertices[idxVertex] = Vector3.Transform(vertices[idxVertex].position, viewMatrix);
                }

                var triangleCount = volume.Triangles.Length;
                for(var idxTriangle = 0; idxTriangle < triangleCount; idxTriangle++) {
                    var t = volume.Triangles[idxTriangle];

                    // Discard if behind far plane
                    if(t.IsBehindFarPlane(vbx)) {
                        stats.BehindViewTriangleCount++;
                        continue;
                    }

                    // Discard if back facing 
                    if(rendererSettings.BackFaceCulling && t.IsFacingBack(vbx)) {
                        stats.FacingBackTriangleCount++;
                        continue;
                    }

                    // Project in frustum
                    t.TransformProjection(vbx, projectionMatrix);

                    // Discard if outside view frustum
                    if(t.isOutsideFrustum(vbx)) {
                        stats.OutOfViewTriangleCount++;
                        continue;
                    }

                    stats.PaintTime();

                    if(!rendererSettings.ShowTextures) {
                        Painter?.DrawTriangle(vbx, idxTriangle);
                    }
                    else {
                        if(Painter.GetType() == typeof(GouraudPainter)) {
                            GouraudPainter painter = (GouraudPainter)Painter;
                            painter.DrawTriangleTextured(texture, vbx, idxTriangle, rendererSettings.LiearTextureFiltering);
                        }
                    }

                    stats.DrawnTriangleCount++;

                    stats.CalcTime();
                }

                // Only draw one volume, will remove later
                break;
            }

            return surface.Screen;
        }
    }
}