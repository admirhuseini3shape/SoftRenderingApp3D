using SoftRenderingApp3D.Buffer;
using SoftRenderingApp3D.DataStructures.Materials;
using SoftRenderingApp3D.DataStructures.Textures;
using SoftRenderingApp3D.Utils;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace SoftRenderingApp3D.Painter
{
    public class GouraudPainter : IPainter
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawTriangle(VertexBuffer vertexBuffer, FrameBuffer frameBuffer, int faId)
        {
            var facet = vertexBuffer.Drawable.Mesh.Facets[faId];

            vertexBuffer.ScreenPointVertices[facet.I0] = frameBuffer.ToScreen3(vertexBuffer.ProjectionVertices[facet.I0]);
            vertexBuffer.ScreenPointVertices[facet.I1] = frameBuffer.ToScreen3(vertexBuffer.ProjectionVertices[facet.I1]);
            vertexBuffer.ScreenPointVertices[facet.I2] = frameBuffer.ToScreen3(vertexBuffer.ProjectionVertices[facet.I2]);

            var hasVertexColors = vertexBuffer.Drawable.Material is IVertexColorMaterial;
            if(!hasVertexColors)
            {
                var triangleColor = Constants.StandardColor;
                if(vertexBuffer.Drawable.Material is IFacetColorMaterial facetColorMaterial)
                    triangleColor = facetColorMaterial.FacetColors[faId];

                vertexBuffer.VertexColors[facet.I0] = triangleColor;
                vertexBuffer.VertexColors[facet.I1] = triangleColor;
                vertexBuffer.VertexColors[facet.I2] = triangleColor;
            }

            var (i0, i1, i2) = PainterUtils.SortIndices(vertexBuffer.ScreenPointVertices, facet.I0, facet.I1, facet.I2);

            var result = ScanLine.ScanLineTriangle(vertexBuffer, frameBuffer.Height, frameBuffer.Width, i0, i1, i2);
            var barycentricPoints = Barycentric2d.ConvertToBarycentricPoints(result,
                vertexBuffer.ScreenPointVertices[i0],
                vertexBuffer.ScreenPointVertices[i1],
                vertexBuffer.ScreenPointVertices[i2]);

            var color0 = vertexBuffer.VertexColors[i0];
            var color1 = vertexBuffer.VertexColors[i1];
            var color2 = vertexBuffer.VertexColors[i2];

            var perPixelColors = new List<(int x, int y, int z, ColorRGB color)>(result.Count);
            for(var i = 0; i < result.Count; i++)
            {
                var alpha = barycentricPoints[i].X;
                var beta = barycentricPoints[i].Y;
                var gamma = barycentricPoints[i].Z;
                var lightContribution = barycentricPoints[i].W;

                //var maxR = Math.Max(color0.R, Math.Max(color1.R, color2.R));
                //var maxG = Math.Max(color0.G, Math.Max(color1.G, color2.G));
                //var maxB = Math.Max(color0.B, Math.Max(color1.B, color2.B));
                // interpolate
                var R = (byte)(alpha * color0.R + beta * color1.R + gamma * color2.R);//.Clamp(0, maxR);
                var G = (byte)(alpha * color0.G + beta * color1.G + gamma * color2.G);//.Clamp(0, maxG);
                var B = (byte)(alpha * color0.B + beta * color1.B + gamma * color2.B);//.Clamp(0, maxB);
                var color = new ColorRGB(R, G, B, 255);

                perPixelColors.Add(((int)result[i].X, (int)result[i].Y, (int)result[i].Z, lightContribution * color));
            }

            lock(frameBuffer)
            {
                for(var i = 0; i < result.Count; i++)
                {
                    var pixel = perPixelColors[i];
                    frameBuffer.PutPixel(pixel.x, pixel.y, pixel.z, pixel.color);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawTriangleTextured(Texture texture, VertexBuffer vertexBuffer, FrameBuffer frameBuffer, int faId, bool linearFiltering)
        {
            var facet = vertexBuffer.Drawable.Mesh.Facets[faId];

            vertexBuffer.ScreenPointVertices[facet.I0] = frameBuffer.ToScreen3(vertexBuffer.ProjectionVertices[facet.I0]);
            vertexBuffer.ScreenPointVertices[facet.I1] = frameBuffer.ToScreen3(vertexBuffer.ProjectionVertices[facet.I1]);
            vertexBuffer.ScreenPointVertices[facet.I2] = frameBuffer.ToScreen3(vertexBuffer.ProjectionVertices[facet.I2]);

            var (i0,i1,i2) = PainterUtils.SortIndices(vertexBuffer.ScreenPointVertices, facet.I0, facet.I1, facet.I2);

            var result = ScanLine.ScanLineTriangle(vertexBuffer, frameBuffer.Height, frameBuffer.Width, i0, i1, i2);
            var barycentricPoints = Barycentric2d.ConvertToBarycentricPoints(result,
                vertexBuffer.ScreenPointVertices[i0],
                vertexBuffer.ScreenPointVertices[i1],
                vertexBuffer.ScreenPointVertices[i2]);

            // Get the texture coordinates of each point of the triangle
            var uv0 = vertexBuffer.Drawable.Mesh.TexCoordinates[i0];
            var uv1 = vertexBuffer.Drawable.Mesh.TexCoordinates[i1];
            var uv2 = vertexBuffer.Drawable.Mesh.TexCoordinates[i2];

            var perPixelColors = new List<(int x, int y, int z, ColorRGB color)>(result.Count);
            for(var i = 0; i < result.Count; i++)
            {
                var alpha = barycentricPoints[i].X;
                var beta = barycentricPoints[i].Y;
                var gamma = barycentricPoints[i].Z;
                var lightContribution = barycentricPoints[i].W;

                var texX = uv0.X * alpha + uv1.X * beta + uv2.X * gamma;
                var texY = uv0.Y * alpha + uv1.Y * beta + uv2.Y * gamma;

                var color = linearFiltering ?
                    texture.GetPixelColorLinearFiltering(texX, texY) :
                    texture.GetPixelColorNearestFiltering(texX, texY);
                perPixelColors.Add(((int)result[i].X, (int)result[i].Y, (int)result[i].Z, lightContribution * color));
            }

            lock(frameBuffer)
            {
                for(var i = 0; i < result.Count; i++)
                {
                    var pixel = perPixelColors[i];
                    frameBuffer.PutPixel(pixel.x, pixel.y, pixel.z, pixel.color);
                }
            }
        }
    }
}
