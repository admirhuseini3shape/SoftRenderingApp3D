using System;
using g3;
using System.Numerics;

namespace SoftRenderingApp3D {

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
                vbx.WorldViewMatrix = modelViewMatrix;

                stats.TotalTriangleCount += volume.Triangles.Length;

                var vertices = volume.Vertices;
                var viewVertices = vbx.ViewVertices;

                // Initialize the offset vertexBuffer
                var offsetVbx = worldBuffer.VertexBuffer[idxVolume + 1];
                var offset = volumes[idxVolume + 1];

                var offsetWorldMatrix = offset.WorldMatrix();
                var offsetModelViewMatrix = offsetWorldMatrix * viewMatrix;

                offsetVbx.Volume = offset;
                offsetVbx.WorldMatrix = offsetWorldMatrix;
                offsetVbx.WorldViewMatrix = offsetModelViewMatrix;

                var offsetViewVertices = offsetVbx.ViewVertices;


                var triangleCount = volume.Triangles.Length;

                // Transform and store vertices to View
                var vertexCount = vertices.Length;
                for(var idxVertex = 0; idxVertex < vertexCount; idxVertex++) {
                    viewVertices[idxVertex] = Vector3.Transform(vertices[idxVertex].position, viewMatrix);
                }

                // Transform and store offset vertices
                var offsetVertexCount = offset.Vertices.Length;
                for(var idxVertex = 0; idxVertex < offsetVertexCount; idxVertex++) {
                    offsetViewVertices[idxVertex] = Vector3.Transform(offset.Vertices[idxVertex].position, viewMatrix);
                }

                vbx.TransformWorld();
                vbx.TransformWorldView();
                offsetVbx.TransformWorld();
                offsetVbx.TransformWorldView();
                var offsetCount = offset.Triangles.Length;

                if(RenderUtils.recalcSubsurfaceScattering) {
                    calculateSubsurfaceScattering(vbx);
                    RenderUtils.recalcSubsurfaceScattering = false;
                }

                // Render surface mesh
                for(var idxTriangle = 0; idxTriangle < triangleCount; idxTriangle++) {
                    // Get triangle
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

                    Painter?.DrawTriangle(vbx, idxTriangle);

                    stats.DrawnTriangleCount++;

                    stats.CalcTime();
                }

                // Render subsurface mesh
                var offsetTriangleCount = offset.Triangles.Length;

                for(var idxTriangle = 0; idxTriangle < offsetTriangleCount; idxTriangle++) {
                    // Get triangle
                    var t = offset.Triangles[idxTriangle];

                    // Discard if behind far plane
                    if(t.IsBehindFarPlane(offsetVbx)) {
                        stats.BehindViewTriangleCount++;
                        continue;
                    }

                    // Discard if back facing 
                    if(rendererSettings.BackFaceCulling && t.IsFacingBack(offsetVbx)) {
                        stats.FacingBackTriangleCount++;
                        continue;
                    }

                    // Project in frustum
                    t.TransformProjection(offsetVbx, projectionMatrix);

                    // Discard if outside view frustum
                    if(t.isOutsideFrustum(offsetVbx)) {
                        stats.OutOfViewTriangleCount++;
                        continue;
                    }

                    var color = offset.TriangleColors[idxTriangle];
                    var subSurfacePainter = new SubsurfacePainter();
                    subSurfacePainter.RendererContext = renderContext;
                    subSurfacePainter.DrawTriangle(offsetVbx, idxTriangle);

                    stats.DrawnTriangleCount++;

                    stats.CalcTime();
                }

                if(RenderUtils.GaussianBlur)
                    renderContext.Surface.ApplyGaussianBlurToSubsurface();
                else
                    renderContext.Surface.ApplyClearSubsurface();
                // Only draw one volume, will remove later
                break;
            }
            return surface.Screen;
        }

        

        public DMesh3 GetDMesh3FromVolume(Volume volume) {
            return DMesh3Builder.Build(volume.Vertices.ToFloatArray(), volume.Triangles.ToIntArray(), volume.NormVertices.ToFloatArray());
        }

        public void calculateSubsurfaceScattering(VertexBuffer vbx) {
            DMesh3 originalG3 = GetDMesh3FromVolume(vbx.Volume as Volume);

            var verticesCount = vbx.Volume.Vertices.Length;

            var lightPos = new Vector3(0, 10, 50);

            (vbx.Volume as Volume).InitializeTrianglesColor(ColorRGB.Black);

            var spatial = new DMeshAABBTree3(originalG3);
            spatial.Build();
            for(int i = 0; i < verticesCount; i++) {
                calculateVertexSubsurfaceScattering(vbx.Volume.Vertices[i], lightPos, spatial, originalG3, (vbx.Volume as Volume), i);
            }
        }

        public void calculateVertexSubsurfaceScattering(ColoredVertex vertex, Vector3 lightPos, DMeshAABBTree3 spatial, DMesh3 originalG3, Volume volume, int index) {
            // get direction of light to vertex
            Vector3 direction = vertex.position - lightPos;

            Ray3d ray = new Ray3d(MiscUtils.Vector3ToVector3d(lightPos), MiscUtils.Vector3ToVector3d(direction));
            int hit_tid = spatial.FindNearestHitTriangle(ray);
            // Check if ray misses
            if(hit_tid != DMesh3.InvalidID) {
                IntrRay3Triangle3 intr = MeshQueries.TriangleIntersection(originalG3, hit_tid, ray);
                // Calculate distance traveled after passing through the surface
                double hit_dist = MiscUtils.Vector3ToVector3d(vertex.position).Distance(ray.PointAt(intr.RayParameter));
                // Calculate the decay of the light
                float decay = (float)Math.Exp(-hit_dist * RenderUtils.subsurfaceDecay);
                // Color of the vertex
                var color = decay * RenderUtils.surfaceColor;
                var alphaColor = new ColorRGB(color.R, color.G, color.B, (byte)(RenderUtils.subsurfaceScatteringWeight * 255));
                volume.Vertices[index].color = alphaColor;
            }
            else {
                var color = RenderUtils.surfaceColor;
                var alphaColor = new ColorRGB(color.R, color.G, color.B, (byte)(RenderUtils.subsurfaceScatteringWeight * 255));
                volume.Vertices[index].color = alphaColor;
            }
        }
    }
}