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
                wireFramePainter.RendererContext = value;
            }
        }

        public IPainter Painter { get; set; }

        private WireFramePainter wireFramePainter;

        public SimpleRenderer() {
            wireFramePainter = new WireFramePainter();
        }

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
                vbx.WorldViewMatrix = modelViewMatrix;

                (vbx.Volume as Volume).SetVolumeColor(ColorRGB.Gray); 

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

                var offsetVertices = offset.Vertices;
                var offsetViewVertices = offsetVbx.ViewVertices;

                (offsetVbx.Volume as Volume).SetVolumeColor(ColorRGB.White);


                DMesh3 originalG3 = GetDMesh3FromVolume(volume as Volume);
                DMesh3 offsetG3 = GetDMesh3FromVolume(offset as Volume);

                var triangleCount = volume.Triangles.Length;

                // Transform and store vertices to View
                var vertexCount = vertices.Length;
                for(var idxVertex = 0; idxVertex < vertexCount; idxVertex++) {
                    viewVertices[idxVertex] = Vector3.Transform(vertices[idxVertex], viewMatrix);
                }

                // Transform and store offset vertices
                var offsetVertexCount = offset.Vertices.Length;
                for(var idxVertex = 0; idxVertex < offsetVertexCount; idxVertex++) {
                    offsetViewVertices[idxVertex] = Vector3.Transform(offset.Vertices[idxVertex], viewMatrix);
                }

                vbx.TransformWorld();
                vbx.TransformWorldView();
                offsetVbx.TransformWorld();
                offsetVbx.TransformWorldView();

                if(rendererSettings.ShowTextures) {
                    DMeshAABBTree3 spatial = new DMeshAABBTree3(offsetG3);
                    spatial.Build();
                    for(int i = 0; i < triangleCount; i++) {
                        var t = volume.Triangles[i];
                        // Calculate subsurface lighting

                        // Calculate distance to offset
                        // get light position
                        Vector3 lightPos = world.LightSources[0].Position;
                        Vector3 trianglePos = t.GetCenterCoordinates(vbx);
                        Vector3 direction = (trianglePos - lightPos);

                        Ray3d ray = new Ray3d(MiscUtils.Vector3ToVector3d(lightPos), MiscUtils.Vector3ToVector3d(direction));
                        int hit_tid = spatial.FindNearestHitTriangle(ray);

                        // Check if ray misses
                        if(hit_tid != DMesh3.InvalidID) {
                            IntrRay3Triangle3 intr = MeshQueries.TriangleIntersection(offsetG3, hit_tid, ray);
                            double hit_dist = MiscUtils.Vector3ToVector3d(lightPos).Distance(ray.PointAt(intr.RayParameter));
                            offsetVbx.Volume.TriangleColors[hit_tid] += (float)Math.Exp(-hit_dist * 0.1f) * ColorRGB.White;
                        }

                    }
                }

                var offsetCount = offset.Triangles.Length;

                if(rendererSettings.ShowTextures) {
                    var spatial = new DMeshAABBTree3(originalG3);
                    spatial.Build();
                    for(int i = 0; i < offsetCount; i++) {
                        var t = offset.Triangles[i];
                        // Now calculate amount of light reaching camera

                        // Calculate distance to offset
                        // get light position
                        Vector3 cameraPos = (renderContext.Camera as ArcBallCam).Position;
                        Vector3 trianglePos = t.GetCenterCoordinates(offsetVbx);
                        Vector3 direction = trianglePos - cameraPos;

                        if(Vector3.Dot(t.CalculateNormal(offset.Vertices), direction) < 0)
                            continue;

                        Ray3d ray = new Ray3d(MiscUtils.Vector3ToVector3d(trianglePos), MiscUtils.Vector3ToVector3d(direction));
                        int hit_tid = spatial.FindNearestHitTriangle(ray);

                        // Check if ray misses
                        if(hit_tid != DMesh3.InvalidID) {
                            IntrRay3Triangle3 intr = MeshQueries.TriangleIntersection(originalG3, hit_tid, ray);
                            double hit_dist = MiscUtils.Vector3ToVector3d(cameraPos).Distance(ray.PointAt(intr.RayParameter));
                            vbx.Volume.TriangleColors[hit_tid] += (float)Math.Exp(-hit_dist * 0.01f) * offsetVbx.Volume.TriangleColors[i];
                        }
                    }
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

                    var color = volume.TriangleColors[idxTriangle];

                    if(rendererSettings.ShowTriangles)
                        wireFramePainter.DrawTriangle(ColorRGB.Magenta, vbx, idxTriangle);

                    if(!rendererSettings.ShowTextures || true) {
                        Painter?.DrawTriangle(color, vbx, idxTriangle);
                    }
                    else {
                        if(Painter.GetType() == typeof(GouraudPainter)) {
                            // Cast to GouraudPainter, this needs fixing because currently only the GouraudPainter has implemented the function for drawing textures
                            GouraudPainter painter = (GouraudPainter)Painter;
                            painter.DrawTriangleTextured(texture, vbx, idxTriangle, rendererSettings.LiearTextureFiltering);
                        }
                    }

                    stats.DrawnTriangleCount++;

                    stats.CalcTime();
                }

                // Render subsurface mesh
                /* if(rendererSettings.ShowTextures) {

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
                        subSurfacePainter.DrawTriangle(color, offsetVbx, idxTriangle);



                        stats.DrawnTriangleCount++;

                        // Now calculate colors for offset


                        stats.CalcTime();
                    }
                }*/

                surface.CombineScreens();

                // Only draw one volume, will remove later
                break;
            }
            if(rendererSettings.ShowXZGrid)
                RenderUtils.drawGrid(surface, wireFramePainter, world2Projection, -10, 10);

            if(rendererSettings.ShowAxes)
                RenderUtils.drawAxes(surface, wireFramePainter, world2Projection);

            return surface.Screen;
        }

        

        public DMesh3 GetDMesh3FromVolume(Volume volume) {
            return DMesh3Builder.Build(volume.Vertices.ToFloatArray(), volume.Triangles.ToIntArray(), volume.NormVertices.ToFloatArray());
        }
    }
}