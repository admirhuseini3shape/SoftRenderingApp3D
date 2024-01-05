using g3;
using SoftRenderingApp3D;
using SoftRenderingApp3D.Buffer;
using SoftRenderingApp3D.DataStructures.Drawables;
using SoftRenderingApp3D.DataStructures.Materials;
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
            var projection = SubsurfaceScatteringRenderContext.Projection;
            var drawables = SubsurfaceScatteringRenderContext.Drawables;
            var rendererSettings = SubsurfaceScatteringRenderContext.RendererSettings;
            var viewMatrix = SubsurfaceScatteringRenderContext.Camera.ViewMatrix();
            var projectionMatrix = projection.ProjectionMatrix(surface.Width, surface.Height);

            stats.Clear();

            stats.PaintTime();
            surface.Clear();

            stats.CalcTime();

            // model => worldMatrix => _subsurfaceScatteringWorld => viewMatrix => view => projectionMatrix => projection => toNdc => ndc => toScreen => screen

            // Allocate arrays to store transformed vertices
            using var allVertexBuffers = new AllVertexBuffers(drawables);
            SubsurfaceScatteringRenderContext.AllVertexBuffers = allVertexBuffers;

            var drawablesCount = drawables.Count;
            for(var idxVolume = 0; idxVolume < drawablesCount; idxVolume++)
            {
                var vertexBuffer = allVertexBuffers.VertexBuffer[idxVolume];
                var mesh = drawables[idxVolume].Mesh;

                //var worldMatrix = volume.WorldMatrix();
                //var modelViewMatrix = worldMatrix * viewMatrix;
                var modelViewMatrix = viewMatrix;

                vertexBuffer.Drawable = drawables[idxVolume];
                //vertexBuffer.WorldMatrix = worldMatrix;
                vertexBuffer.WorldViewMatrix = modelViewMatrix;

                stats.TotalTriangleCount += mesh.Facets.Count;

                var vertices = mesh.Vertices;
                var viewVertices = vertexBuffer.ViewVertices;

                // Initialize the offset vertexBuffer
                var idxOffset = idxVolume + 1;
                var offsetvertexBuffer = allVertexBuffers.VertexBuffer[idxOffset];
                var offsetMesh = drawables[idxOffset].Mesh;

                //var offsetWorldMatrix = offset.WorldMatrix();
                //var offsetModelViewMatrix = offsetWorldMatrix * viewMatrix;
                var offsetModelViewMatrix = viewMatrix;

                offsetvertexBuffer.Drawable = drawables[idxOffset];
                //offsetvertexBuffer.WorldMatrix = offsetWorldMatrix;
                offsetvertexBuffer.WorldViewMatrix = offsetModelViewMatrix;

                var offsetViewVertices = offsetvertexBuffer.ViewVertices;


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

                vertexBuffer.TransformWorld();
                vertexBuffer.TransformWorldView();
                offsetvertexBuffer.TransformWorld();
                offsetvertexBuffer.TransformWorldView();

                if(SubsurfaceScatteringRenderUtils.RecalcSubsurfaceScattering)
                {
                    CalculateSubsurfaceScattering(vertexBuffer);
                    SubsurfaceScatteringRenderUtils.RecalcSubsurfaceScattering = false;
                }

                // Render surface mesh
                for(var idxTriangle = 0; idxTriangle < triangleCount; idxTriangle++)
                {
                    // Get triangle
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

                    SubsurfaceScatteringPainter?.DrawTriangle(vertexBuffer, idxTriangle);

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
                    if(t.IsBehindFarPlane(offsetvertexBuffer))
                    {
                        stats.BehindViewTriangleCount++;
                        continue;
                    }

                    // Discard if back facing 
                    if(rendererSettings.BackFaceCulling && t.IsFacingBack(offsetvertexBuffer))
                    {
                        stats.FacingBackTriangleCount++;
                        continue;
                    }

                    // Project in frustum
                    t.TransformProjection(offsetvertexBuffer, projectionMatrix);

                    // Discard if outside view frustum
                    if(t.IsOutsideFrustum(offsetvertexBuffer))
                    {
                        stats.OutOfViewTriangleCount++;
                        continue;
                    }

                    var subSurfacePainter = new SubsurfaceSubsurfaceScatteringPainter();
                    subSurfacePainter.RendererContext = SubsurfaceScatteringRenderContext;
                    subSurfacePainter.DrawTriangle(offsetvertexBuffer, idxTriangle);

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
                var cariesvertexBuffer = allVertexBuffers.VertexBuffer[idxCaries];
                var caries = drawables[idxCaries];

                //var cariesWorldMatrix = caries.WorldMatrix();
                //var cariesModelViewMatrix = cariesWorldMatrix * viewMatrix;
                var cariesModelViewMatrix = viewMatrix;

                cariesvertexBuffer.Drawable = caries;
                //cariesvertexBuffer.WorldMatrix = cariesWorldMatrix;
                cariesvertexBuffer.WorldViewMatrix = cariesModelViewMatrix;
                var cariesMesh = caries.Mesh;
                var cariesVertices = cariesMesh.Vertices;
                var cariesViewVertices = cariesvertexBuffer.ViewVertices;


                var cariesTriangleCount = cariesMesh.Facets.Count;

                // Transform and store vertices to View
                var cariesVertexCount = cariesVertices.Count;
                for(var idxVertex = 0; idxVertex < cariesVertexCount; idxVertex++)
                {
                    cariesViewVertices[idxVertex] = Vector3.Transform(cariesVertices[idxVertex], viewMatrix);
                }

                cariesvertexBuffer.TransformWorld();
                cariesvertexBuffer.TransformWorldView();


                for(var idxTriangle = 0; idxTriangle < cariesTriangleCount; idxTriangle++)
                {
                    // Get triangle
                    var t = cariesMesh.Facets[idxTriangle];

                    // Discard if behind far plane
                    if(t.IsBehindFarPlane(cariesvertexBuffer))
                    {
                        stats.BehindViewTriangleCount++;
                        continue;
                    }

                    // Discard if back facing 
                    if(rendererSettings.BackFaceCulling && t.IsFacingBack(cariesvertexBuffer))
                    {
                        stats.FacingBackTriangleCount++;
                        continue;
                    }

                    // Project in frustum
                    t.TransformProjection(cariesvertexBuffer, projectionMatrix);

                    // Discard if outside view frustum
                    if(t.IsOutsideFrustum(cariesvertexBuffer))
                    {
                        stats.OutOfViewTriangleCount++;
                        continue;
                    }

                    var cariesPainter = new CariesSubsurfaceScatteringPainter();
                    cariesPainter.RendererContext = SubsurfaceScatteringRenderContext;
                    cariesPainter.DrawTriangle(cariesvertexBuffer, idxTriangle);

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

        public void CalculateSubsurfaceScattering(VertexBuffer vertexBuffer)
        {
            var originalG3 = GetDMesh3FromVolume(vertexBuffer.Drawable.Mesh);

            var verticesCount = vertexBuffer.Drawable.Mesh.VertexCount;

            var lightPos = new Vector3(0, 10, 50);

            vertexBuffer.Drawable = new Drawable(vertexBuffer.Drawable.Mesh, new FacetColorMaterial(verticesCount, ColorRGB.Black));

            var spatial = new DMeshAABBTree3(originalG3);
            spatial.Build();
            for(var i = 0; i < verticesCount; i++)
            {
                CalculateVertexSubsurfaceScattering(vertexBuffer.Drawable.Mesh.Vertices[i], lightPos, spatial, originalG3,
                    vertexBuffer.VertexColors, i);
            }
        }

        public void CalculateVertexSubsurfaceScattering(Vector3 vertex, Vector3 lightPos, DMeshAABBTree3 spatial,
            DMesh3 originalG3, ColorRGB[] vertexColorsBuffer, int index)
        {
            // get direction of light to vertex
            var direction = vertex - lightPos;

            var ray = new Ray3d(SubsurfaceScatteringMiscUtils.ToVector3d(lightPos),
                SubsurfaceScatteringMiscUtils.ToVector3d(direction));
            var hitTid = spatial.FindNearestHitTriangle(ray);
            // Check if ray misses
            if(hitTid != DMesh3.InvalidID)
            {
                var intr = MeshQueries.TriangleIntersection(originalG3, hitTid, ray);
                // Calculate distance traveled after passing through the surface
                var hitDist = SubsurfaceScatteringMiscUtils.ToVector3d(vertex)
                    .Distance(ray.PointAt(intr.RayParameter));
                // Calculate the decay of the light
                var decay = (float)Math.Exp(-hitDist * SubsurfaceScatteringRenderUtils.SubsurfaceDecay);
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
