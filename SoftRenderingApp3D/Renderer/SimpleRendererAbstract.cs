using SoftRenderingApp3D.Buffer;
using SoftRenderingApp3D.DataStructures;
using SoftRenderingApp3D.DataStructures.Drawables;
using SoftRenderingApp3D.DataStructures.Materials;
using SoftRenderingApp3D.Painter;
using SoftRenderingApp3D.Rasterizers;
using SoftRenderingApp3D.Utils;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace SoftRenderingApp3D.Renderer
{
    public abstract class SimpleRendererAbstract : IRenderer
    {
        public VertexBuffer VertexBuffer { get; }
        public FrameBuffer FrameBuffer { get; }
        protected readonly Stats stats;

        protected SimpleRendererAbstract(VertexBuffer vertexBuffer, FrameBuffer frameBuffer)
        {
            VertexBuffer = vertexBuffer;
            FrameBuffer = frameBuffer;
            stats = StatsSingleton.Instance;
        }

        public int[] Render(IPainter painter, Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix, RendererSettings rendererSettings)
        {
            FrameBuffer.Clear();
            var drawable = VertexBuffer.Drawable;
            if(drawable == null || painter == null || rendererSettings == null || drawable.Mesh.FacetCount == 0)
            {
                return FrameBuffer.Screen;
            }

            stats.Clear();

            UpdateVertexBuffer(viewMatrix, projectionMatrix);
            stats.paintSw.Restart();

            //var zSortedFacets = drawable.Mesh.Facets
            //.Select((fa, i) => new { FaId = i, zDepth = fa.CalculateZAverages(vertexBuffer.ProjectionVertices) })
            //.ToList();
            //zSortedFacets.Sort((x, y) => (int)(1000 * x.zDepth - 1000 * y.zDepth));


            DrawFacets(painter, drawable, rendererSettings);
            stats.paintSw.Stop();

            return FrameBuffer.Screen;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract void DrawFacets(IPainter painter, IDrawable drawable, RendererSettings rendererSettings);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UpdateVertexBuffer(Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix)
        {
            stats.calcSw.Restart();
            VertexBuffer.Clear();

            // model => worldMatrix => world => viewMatrix => view => projectionMatrix => projection => toNdc => ndc => toScreen => screen

            // Transform and store vertices to View
            TransformVertexBuffers(viewMatrix, projectionMatrix);
            var facetsCount = VertexBuffer.Drawable.Mesh.FacetCount;
            stats.TotalTriangleCount += facetsCount;

            stats.calcSw.Stop();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract void TransformVertexBuffers(Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TransformVertex(Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix, int veId)
        {
            VertexBuffer.WorldVertices[veId] = viewMatrix.Transform(VertexBuffer.Drawable.Mesh.Vertices[veId]);
            VertexBuffer.WorldVertexNormals[veId] =
                viewMatrix.TransformWithoutTranslation(VertexBuffer.Drawable.Mesh.VertexNormals[veId]);
            VertexBuffer.ViewVertices[veId] = viewMatrix.Transform(VertexBuffer.Drawable.Mesh.Vertices[veId]);
            VertexBuffer.ProjectionVertices[veId] = Vector4.Transform(VertexBuffer.ViewVertices[veId], projectionMatrix);
            VertexBuffer.ScreenPointVertices[veId] = FrameBuffer.ToScreen3(VertexBuffer.ProjectionVertices[veId]);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected List<FacetPixelData> CalculateShadingColors(IDrawable drawable, IPainter painter, 
            List<Vector3> pixels, RendererSettings rendererSettings, int faId)
        {
            var perPixelColors = new List<FacetPixelData>();
            var textureMaterial = drawable.Material as ITextureMaterial;
            var hasTexture = textureMaterial != null && textureMaterial.Texture != null;
            if(!hasTexture || !rendererSettings.ShowTextures)
                perPixelColors = painter.DrawTriangle(VertexBuffer, pixels, faId);
            else if(painter is GouraudPainter gouraudPainter)
            {
                perPixelColors = gouraudPainter.DrawTriangleTextured(textureMaterial.Texture,
                    VertexBuffer, pixels, faId, rendererSettings.LinearTextureFiltering);
            }

            return perPixelColors;
        }
    }
}
