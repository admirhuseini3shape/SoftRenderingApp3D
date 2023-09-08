using System;
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


            var models = world.Models;
            var modelCount = models.Count;
            for(var idxModel = 0; idxModel < modelCount; idxModel++) {

                var vbx = worldBuffer.VertexBuffer[idxModel];
                var model = models[idxModel];

                var worldMatrix = model.Volume.WorldMatrix();
                var modelViewMatrix = worldMatrix * viewMatrix;


                vbx.Volume = model.Volume;
                vbx.WorldMatrix = worldMatrix;

                stats.TotalTriangleCount += model.Volume.Triangles.Length;

                var vertices = model.Volume.Vertices;
                var viewVertices = vbx.ViewVertices;

                // Transform and store vertices to View
                var vertexCount = vertices.Length;
                for(var idxVertex = 0; idxVertex < vertexCount; idxVertex++) {
                    viewVertices[idxVertex] = Vector3.Transform(vertices[idxVertex], viewMatrix);
                }

                var triangleCount = model.Volume.Triangles.Length;
                for(var idxTriangle = 0; idxTriangle < triangleCount; idxTriangle++) {
                    var triangle = model.Volume.Triangles[idxTriangle];

                    // Discard if behind far plane
                    if(triangle.IsBehindFarPlane(vbx)) {
                        stats.BehindViewTriangleCount++;
                        continue;
                    }

                    // Discard if back facing 
                    if(rendererSettings.BackFaceCulling && triangle.IsFacingBack(vbx)) {
                        stats.FacingBackTriangleCount++;
                        continue;
                    }

                    // Project in frustum
                    triangle.TransformProjection(vbx, projectionMatrix);

                    // Discard if outside view frustum
                    if(triangle.isOutsideFrustum(vbx)) {
                        stats.OutOfViewTriangleCount++;
                        continue;
                    }

                    stats.PaintTime();

                    if(rendererSettings.ShowTriangles)
                        wireFramePainter.DrawTriangle(ColorRGB.Magenta, vbx, idxTriangle);

                    if(rendererSettings.ShowTriangleNormals) {
                        var worldCentroid = triangle.CalculateCentroid(vbx.WorldVertices);

                        var startPoint = Vector4.Transform(worldCentroid, world2Projection);
                        var endPoint = Vector4.Transform(worldCentroid + triangle.CalculateNormal(vbx.WorldVertices), world2Projection);

                        wireFramePainter.DrawLine(surface, ColorRGB.Red, startPoint, endPoint);
                    }

                    // Check if model is textured or not
                    if (model.GetType() == typeof(BasicModel)) {
                        var color = (model as BasicModel).Colors[idxTriangle];

                        Painter?.DrawTriangle(color, vbx, idxTriangle);
                    }
                    else if (model.GetType() == typeof(TexturedModel)) {
                        // Cast to GouraudPainter, this needs fixing because currently only the GouraudPainter has implemented the function for drawing textures
                        GouraudPainter painter = (GouraudPainter)Painter;
                        painter.DrawTriangleTextured((model as TexturedModel).Texture, vbx, idxTriangle, rendererSettings.LiearTextureFiltering);
                    }
                    else {
                        throw new Exception($"Invalid object type for model, type is: {model.GetType()}, expected: {typeof(TexturedModel)}, {typeof(BasicModel)}");
                    }
                    


                    stats.DrawnTriangleCount++;

                    stats.CalcTime();
                }

                // Only draw one volume, will remove later
                break;
            }

            if(rendererSettings.ShowXZGrid)
                RenderUtils.drawGrid(surface, wireFramePainter, world2Projection, -10, 10);

            if(rendererSettings.ShowAxes)
                RenderUtils.drawAxes(surface, wireFramePainter, world2Projection);

            return surface.Screen;
        }
    }
}