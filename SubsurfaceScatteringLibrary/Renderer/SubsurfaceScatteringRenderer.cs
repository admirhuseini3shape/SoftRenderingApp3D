using g3;
using SoftRenderingApp3D;
using SoftRenderingApp3D.Buffer;
using SoftRenderingApp3D.DataStructures.Drawables;
using SoftRenderingApp3D.DataStructures.Materials;
using SoftRenderingApp3D.DataStructures.Meshes;
using SubsurfaceScatteringLibrary.Painter;
using SubsurfaceScatteringLibrary.Utils;
using System;
using System.Numerics;

namespace SubsurfaceScatteringLibrary.Renderer
{
    public class SubsurfaceScatteringRenderer : ISubsurfaceScatteringRenderer
    {
        public SubsurfaceScatteringRenderContext SubsurfaceScatteringRenderContext { get; set; }

        public ISubsurfaceScatteringPainter SubsurfaceScatteringPainter { get; set; }

        public int[] Render()
        {
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

            var volumes = world.Drawables;
            var volumeCount = volumes.Count;
            for(var idxVolume = 0; idxVolume < volumeCount; idxVolume++)
            {
                var vbx = worldBuffer.VertexBuffer[idxVolume];
                var mesh = volumes[idxVolume].Mesh;

                //var worldMatrix = volume.WorldMatrix();
                //var modelViewMatrix = worldMatrix * viewMatrix;
                var modelViewMatrix = viewMatrix;

                vbx.Drawable = volumes[idxVolume];
                //vbx.WorldMatrix = worldMatrix;
                vbx.WorldViewMatrix = modelViewMatrix;

                stats.TotalTriangleCount += mesh.Facets.Count;

                var vertices = mesh.Vertices;
                var viewVertices = vbx.ViewVertices;

                // Initialize the offset vertexBuffer
                var idxOffset = idxVolume + 1;
                var offsetVbx = worldBuffer.VertexBuffer[idxOffset];
                var offsetMesh = volumes[idxOffset].Mesh;

                //var offsetWorldMatrix = offset.WorldMatrix();
                //var offsetModelViewMatrix = offsetWorldMatrix * viewMatrix;
                var offsetModelViewMatrix = viewMatrix;

                offsetVbx.Drawable = volumes[idxOffset];
                //offsetVbx.WorldMatrix = offsetWorldMatrix;
                offsetVbx.WorldViewMatrix = offsetModelViewMatrix;

                var offsetViewVertices = offsetVbx.ViewVertices;


                var triangleCount = mesh.Facets.Count;

                // Transform and store vertices to View
                var vertexCount = vertices.Count;
                for(var idxVertex = 0; idxVertex < vertexCount; idxVertex++)
                {
                    viewVertices[idxVertex] = Vector3.Transform(vertices[idxVertex], viewMatrix);
                }

                // Transform and store offset vertices
                var offsetVertexCount = offsetMesh.Vertices.Count;
                for(var idxVertex = 0; idxVertex < offsetVertexCount; idxVertex++)
                {
                    offsetViewVertices[idxVertex] = Vector3.Transform(offsetMesh.Vertices[idxVertex], viewMatrix);
                }

                vbx.TransformWorld();
                vbx.TransformWorldView();
                offsetVbx.TransformWorld();
                offsetVbx.TransformWorldView();

                if(SubsurfaceScatteringRenderUtils.recalcSubsurfaceScattering)
                {
                    CalculateSubsurfaceScattering(vbx);
                    SubsurfaceScatteringRenderUtils.recalcSubsurfaceScattering = false;
                }

                // Render surface mesh
                for(var idxTriangle = 0; idxTriangle < triangleCount; idxTriangle++)
                {
                    // Get triangle
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

                    SubsurfaceScatteringPainter?.DrawTriangle(vbx, idxTriangle);

                    stats.DrawnTriangleCount++;

                    stats.CalcTime();
                }

                // Render subsurface mesh
                var offsetTriangleCount = offsetMesh.Facets.Count;

                for(var idxTriangle = 0; idxTriangle < offsetTriangleCount; idxTriangle++)
                {
                    // Get triangle
                    var t = offsetMesh.Facets[idxTriangle];

                    // Discard if behind far plane
                    if(t.IsBehindFarPlane(offsetVbx))
                    {
                        stats.BehindViewTriangleCount++;
                        continue;
                    }

                    // Discard if back facing 
                    if(rendererSettings.BackFaceCulling && t.IsFacingBack(offsetVbx))
                    {
                        stats.FacingBackTriangleCount++;
                        continue;
                    }

                    // Project in frustum
                    t.TransformProjection(offsetVbx, projectionMatrix);

                    // Discard if outside view frustum
                    if(t.IsOutsideFrustum(offsetVbx))
                    {
                        stats.OutOfViewTriangleCount++;
                        continue;
                    }

                    var subSurfacePainter = new SubsurfaceSubsurfaceScatteringPainter();
                    subSurfacePainter.RendererContext = SubsurfaceScatteringRenderContext;
                    subSurfacePainter.DrawTriangle(offsetVbx, idxTriangle);

                    stats.DrawnTriangleCount++;

                    stats.CalcTime();
                }


                // Caries ==================================================
                if(!SubsurfaceScatteringRenderUtils.Caries)
                {
                    if(SubsurfaceScatteringRenderUtils.GaussianBlur)
                    {
                        SubsurfaceScatteringRenderContext.Surface.ApplyGaussianBlurToSubsurface();
                    }
                    else
                    {
                        SubsurfaceScatteringRenderContext.Surface.ApplyClearSubsurface();
                    }

                    break;
                }

                var idxCaries = idxVolume + 2;
                var cariesVbx = worldBuffer.VertexBuffer[idxCaries];
                var caries = volumes[idxCaries];

                //var cariesWorldMatrix = caries.WorldMatrix();
                //var cariesModelViewMatrix = cariesWorldMatrix * viewMatrix;
                var cariesModelViewMatrix = viewMatrix;

                cariesVbx.Drawable = caries;
                //cariesVbx.WorldMatrix = cariesWorldMatrix;
                cariesVbx.WorldViewMatrix = cariesModelViewMatrix;
                var cariesMesh = caries.Mesh;
                var cariesVertices = cariesMesh.Vertices;
                var cariesViewVertices = cariesVbx.ViewVertices;


                var cariesTriangleCount = cariesMesh.Facets.Count;

                // Transform and store vertices to View
                var cariesVertexCount = cariesVertices.Count;
                for(var idxVertex = 0; idxVertex < cariesVertexCount; idxVertex++)
                {
                    cariesViewVertices[idxVertex] = Vector3.Transform(cariesVertices[idxVertex], viewMatrix);
                }

                cariesVbx.TransformWorld();
                cariesVbx.TransformWorldView();


                for(var idxTriangle = 0; idxTriangle < cariesTriangleCount; idxTriangle++)
                {
                    // Get triangle
                    var t = cariesMesh.Facets[idxTriangle];

                    // Discard if behind far plane
                    if(t.IsBehindFarPlane(cariesVbx))
                    {
                        stats.BehindViewTriangleCount++;
                        continue;
                    }

                    // Discard if back facing 
                    if(rendererSettings.BackFaceCulling && t.IsFacingBack(cariesVbx))
                    {
                        stats.FacingBackTriangleCount++;
                        continue;
                    }

                    // Project in frustum
                    t.TransformProjection(cariesVbx, projectionMatrix);

                    // Discard if outside view frustum
                    if(t.IsOutsideFrustum(cariesVbx))
                    {
                        stats.OutOfViewTriangleCount++;
                        continue;
                    }

                    var cariesPainter = new CariesSubsurfaceScatteringPainter();
                    cariesPainter.RendererContext = SubsurfaceScatteringRenderContext;
                    cariesPainter.DrawTriangle(cariesVbx, idxTriangle);

                    stats.DrawnTriangleCount++;

                    stats.CalcTime();
                }

                if(SubsurfaceScatteringRenderUtils.GaussianBlur)
                {
                    SubsurfaceScatteringRenderContext.Surface.ApplyGaussianBlurToSubsurface();
                }
                else
                {
                    SubsurfaceScatteringRenderContext.Surface.ApplyClearSubsurface();
                }

                // Only draw one subsurfaceScatteringMesh, will remove later
                break;
            }

            return surface.Screen;
        }

        public DMesh3 GetDMesh3FromVolume(SoftRenderingApp3D.DataStructures.Meshes.IMesh subsurfaceScatteringMesh)
        {
            return DMesh3Builder.Build(
                subsurfaceScatteringMesh.Vertices.ToFloats(),
                subsurfaceScatteringMesh.Facets.ToInts(),
                subsurfaceScatteringMesh.VertexNormals.ToFloats());
        }

        public void CalculateSubsurfaceScattering(VertexBuffer vbx)
        {
            var originalG3 = GetDMesh3FromVolume(vbx.Drawable.Mesh);

            var verticesCount = vbx.Drawable.Mesh.VertexCount;

            var lightPos = new Vector3(0, 10, 50);

            vbx.Drawable = new Drawable(vbx.Drawable.Mesh, new FacetColorMaterial(verticesCount, ColorRGB.Black));

            var spatial = new DMeshAABBTree3(originalG3);
            spatial.Build();
            for(var i = 0; i < verticesCount; i++)
            {
                CalculateVertexSubsurfaceScattering(vbx.Drawable.Mesh.Vertices[i], lightPos, spatial, originalG3,
                    vbx.VertexColors, i);
            }
        }

        public void CalculateVertexSubsurfaceScattering(Vector3 vertex, Vector3 lightPos, DMeshAABBTree3 spatial,
            DMesh3 originalG3, ColorRGB[] vertexColorsBuffer, int index)
        {
            // get direction of light to vertex
            var direction = vertex - lightPos;

            var ray = new Ray3d(SubsurfaceScatteringMiscUtils.ToVector3d(lightPos),
                SubsurfaceScatteringMiscUtils.ToVector3d(direction));
            var hit_tid = spatial.FindNearestHitTriangle(ray);
            // Check if ray misses
            if(hit_tid != DMesh3.InvalidID)
            {
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
            else
            {
                var color = SubsurfaceScatteringRenderUtils.surfaceColor;
                vertexColorsBuffer[index] = color;
            }
        }
    }
}
