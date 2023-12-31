using g3;
using SoftRenderingApp3D;
using SoftRenderingApp3D.Buffer;
using SoftRenderingApp3D.DataStructures.Meshes;
using SubsurfaceScatteringLibrary.Painter;
using SubsurfaceScatteringLibrary.Utils;
using System;
using System.Numerics;

namespace SubsurfaceScatteringLibrary.Renderer {
    public class SubsurfaceScatteringRenderer : ISubsurfaceScatteringRenderer {
        public SubsurfaceScatteringRenderContext SubsurfaceScatteringRenderContext { get; set; }

        public ISubsurfaceScatteringPainter SubsurfaceScatteringPainter { get; set; }

        public int[] Render() {
            var stats = SubsurfaceScatteringRenderContext.Stats;
            var surface = SubsurfaceScatteringRenderContext.Surface;
            var camera = SubsurfaceScatteringRenderContext.Camera;
            var projection = SubsurfaceScatteringRenderContext.Projection;
            var world = SubsurfaceScatteringRenderContext.World;
            var rendererSettings = SubsurfaceScatteringRenderContext.RendererSettings;

            stats.Clear();

            stats.PaintTime();
            surface.Clear();

            stats.CalcTime();

            // model => worldMatrix => _subsurfaceScatteringWorld => viewMatrix => view => projectionMatrix => projection => toNdc => ndc => toScreen => screen

            var viewMatrix = camera.ViewMatrix();
            var projectionMatrix = projection.ProjectionMatrix(surface.Width, surface.Height);

            // Allocate arrays to store transformed vertices
            using var worldBuffer = new WorldBuffer(world);
            SubsurfaceScatteringRenderContext.WorldBuffer = worldBuffer;

            var volumes = world.Meshes;
            var volumeCount = volumes.Count;
            for(var idxVolume = 0; idxVolume < volumeCount; idxVolume++) {
                var vbx = worldBuffer.VertexBuffer[idxVolume];
                var volume = volumes[idxVolume];

                //var worldMatrix = volume.WorldMatrix();
                //var modelViewMatrix = worldMatrix * viewMatrix;
                var modelViewMatrix = viewMatrix;

                vbx.Mesh = volume;
                //vbx.WorldMatrix = worldMatrix;
                vbx.WorldViewMatrix = modelViewMatrix;

                stats.TotalTriangleCount += volume.Triangles.Count;

                var vertices = volume.Vertices;
                var viewVertices = vbx.ViewVertices;

                // Initialize the offset vertexBuffer
                var offsetVbx = worldBuffer.VertexBuffer[idxVolume + 1];
                var offset = volumes[idxVolume + 1];

                //var offsetWorldMatrix = offset.WorldMatrix();
                //var offsetModelViewMatrix = offsetWorldMatrix * viewMatrix;
                var offsetModelViewMatrix = viewMatrix;

                offsetVbx.Mesh = offset;
                //offsetVbx.WorldMatrix = offsetWorldMatrix;
                offsetVbx.WorldViewMatrix = offsetModelViewMatrix;

                var offsetViewVertices = offsetVbx.ViewVertices;


                var triangleCount = volume.Triangles.Count;

                // Transform and store vertices to View
                var vertexCount = vertices.Count;
                for(var idxVertex = 0; idxVertex < vertexCount; idxVertex++) {
                    viewVertices[idxVertex] = Vector3.Transform(vertices[idxVertex], viewMatrix);
                }

                // Transform and store offset vertices
                var offsetVertexCount = offset.Vertices.Count;
                for(var idxVertex = 0; idxVertex < offsetVertexCount; idxVertex++) {
                    offsetViewVertices[idxVertex] = Vector3.Transform(offset.Vertices[idxVertex], viewMatrix);
                }

                vbx.TransformWorld();
                vbx.TransformWorldView();
                offsetVbx.TransformWorld();
                offsetVbx.TransformWorldView();
                var offsetCount = offset.Triangles.Count;

                if(SubsurfaceScatteringRenderUtils.recalcSubsurfaceScattering) {
                    CalculateSubsurfaceScattering(vbx);
                    SubsurfaceScatteringRenderUtils.recalcSubsurfaceScattering = false;
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
                    if(t.IsOutsideFrustum(vbx)) {
                        stats.OutOfViewTriangleCount++;
                        continue;
                    }

                    stats.PaintTime();

                    SubsurfaceScatteringPainter?.DrawTriangle(vbx, idxTriangle);

                    stats.DrawnTriangleCount++;

                    stats.CalcTime();
                }

                // Render subsurface mesh
                var offsetTriangleCount = offset.Triangles.Count;

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
                    if(t.IsOutsideFrustum(offsetVbx)) {
                        stats.OutOfViewTriangleCount++;
                        continue;
                    }

                    var color = offset.TriangleColors[idxTriangle];
                    var subSurfacePainter = new SubsurfaceSubsurfaceScatteringPainter();
                    subSurfacePainter.RendererContext = SubsurfaceScatteringRenderContext;
                    subSurfacePainter.DrawTriangle(offsetVbx, idxTriangle);

                    stats.DrawnTriangleCount++;

                    stats.CalcTime();
                }


                // Caries ==================================================
                if(!SubsurfaceScatteringRenderUtils.Caries) {
                    if(SubsurfaceScatteringRenderUtils.GaussianBlur) {
                        SubsurfaceScatteringRenderContext.Surface.ApplyGaussianBlurToSubsurface();
                    }
                    else {
                        SubsurfaceScatteringRenderContext.Surface.ApplyClearSubsurface();
                    }

                    break;
                }

                var cariesVbx = worldBuffer.VertexBuffer[idxVolume + 2];
                var caries = volumes[idxVolume + 2];

                //var cariesWorldMatrix = caries.WorldMatrix();
                //var cariesModelViewMatrix = cariesWorldMatrix * viewMatrix;
                var cariesModelViewMatrix = viewMatrix;

                cariesVbx.Mesh = caries;
                //cariesVbx.WorldMatrix = cariesWorldMatrix;
                cariesVbx.WorldViewMatrix = cariesModelViewMatrix;

                var cariesVertices = caries.Vertices;
                var cariesViewVertices = cariesVbx.ViewVertices;


                var cariesTriangleCount = caries.Triangles.Count;

                // Transform and store vertices to View
                var cariesVertexCount = cariesVertices.Count;
                for(var idxVertex = 0; idxVertex < cariesVertexCount; idxVertex++) {
                    cariesViewVertices[idxVertex] = Vector3.Transform(cariesVertices[idxVertex], viewMatrix);
                }

                cariesVbx.TransformWorld();
                cariesVbx.TransformWorldView();


                for(var idxTriangle = 0; idxTriangle < cariesTriangleCount; idxTriangle++) {
                    // Get triangle
                    var t = caries.Triangles[idxTriangle];

                    // Discard if behind far plane
                    if(t.IsBehindFarPlane(cariesVbx)) {
                        stats.BehindViewTriangleCount++;
                        continue;
                    }

                    // Discard if back facing 
                    if(rendererSettings.BackFaceCulling && t.IsFacingBack(cariesVbx)) {
                        stats.FacingBackTriangleCount++;
                        continue;
                    }

                    // Project in frustum
                    t.TransformProjection(cariesVbx, projectionMatrix);

                    // Discard if outside view frustum
                    if(t.IsOutsideFrustum(cariesVbx)) {
                        stats.OutOfViewTriangleCount++;
                        continue;
                    }

                    var cariesPainter = new CariesSubsurfaceScatteringPainter();
                    cariesPainter.RendererContext = SubsurfaceScatteringRenderContext;
                    cariesPainter.DrawTriangle(cariesVbx, idxTriangle);

                    stats.DrawnTriangleCount++;

                    stats.CalcTime();
                }

                if(SubsurfaceScatteringRenderUtils.GaussianBlur) {
                    SubsurfaceScatteringRenderContext.Surface.ApplyGaussianBlurToSubsurface();
                }
                else {
                    SubsurfaceScatteringRenderContext.Surface.ApplyClearSubsurface();
                }

                // Only draw one subsurfaceScatteringMesh, will remove later
                break;
            }

            return surface.Screen;
        }

        public DMesh3 GetDMesh3FromVolume(Mesh subsurfaceScatteringMesh) {
            return DMesh3Builder.Build(
                subsurfaceScatteringMesh.Vertices.ToFloatArray(),
                subsurfaceScatteringMesh.Triangles.ToIntArray(),
                subsurfaceScatteringMesh.VertexNormals.ToFloatArray());
        }

        public void CalculateSubsurfaceScattering(VertexBuffer vbx) {
            var originalG3 = GetDMesh3FromVolume(vbx.Mesh as Mesh);

            var verticesCount = vbx.Mesh.Vertices.Count;

            var lightPos = new Vector3(0, 10, 50);

            (vbx.Mesh as Mesh)?.InitializeTrianglesColor(ColorRGB.Black);

            var spatial = new DMeshAABBTree3(originalG3);
            spatial.Build();
            for(var i = 0; i < verticesCount; i++) {
                CalculateVertexSubsurfaceScattering(vbx.Mesh.Vertices[i], lightPos, spatial, originalG3,
                    vbx.VertexColors, i);
            }
        }

        public void CalculateVertexSubsurfaceScattering(Vector3 vertex, Vector3 lightPos, DMeshAABBTree3 spatial,
            DMesh3 originalG3, ColorRGB[] vertexColorsBuffer, int index) {
            // get direction of light to vertex
            var direction = vertex - lightPos;

            var ray = new Ray3d(SubsurfaceScatteringMiscUtils.ToVector3d(lightPos),
                SubsurfaceScatteringMiscUtils.ToVector3d(direction));
            var hit_tid = spatial.FindNearestHitTriangle(ray);
            // Check if ray misses
            if(hit_tid != DMesh3.InvalidID) {
                var intr = MeshQueries.TriangleIntersection(originalG3, hit_tid, ray);
                // Calculate distance traveled after passing through the surface
                var hit_dist = SubsurfaceScatteringMiscUtils.ToVector3d(vertex)
                    .Distance(ray.PointAt(intr.RayParameter));
                // Calculate the decay of the light
                var decay = (float)Math.Exp(-hit_dist * SubsurfaceScatteringRenderUtils.subsurfaceDecay);
                // Color of the vertex
                //var color = decay * SubsurfaceScatteringRenderUtils.surfaceColor;
                var color = decay * SubsurfaceScatteringRenderUtils.surfaceColor;
                vertexColorsBuffer[index] = color;
            }
            else {
                var color = SubsurfaceScatteringRenderUtils.surfaceColor;
                vertexColorsBuffer[index] = color;
            }
        }
    }
}