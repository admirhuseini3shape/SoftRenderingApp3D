using SoftRenderingApp3D.Buffer;
using SoftRenderingApp3D.DataStructures.Materials;
using SoftRenderingApp3D.DataStructures.Textures;
using SoftRenderingApp3D.Utils;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace SoftRenderingApp3D.Painter
{
    public class GouraudPainter : IPainter
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawTriangle(VertexBuffer vertexBuffer, FrameBuffer frameBuffer, float[,] zBuffer, int faId)
        {
            var facet = vertexBuffer.Drawable.Mesh.Facets[faId];

            var pixels = GetPixels(vertexBuffer, frameBuffer, facet);

            var barycentricPoints = Barycentric2d.ConvertToBarycentricPoints(pixels,
                vertexBuffer.ScreenPointVertices[facet.I0],
                vertexBuffer.ScreenPointVertices[facet.I1],
                vertexBuffer.ScreenPointVertices[facet.I2]);

            if(barycentricPoints == null)
                return;
            
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

            var color0 = vertexBuffer.VertexColors[facet.I0];
            var color1 = vertexBuffer.VertexColors[facet.I1];
            var color2 = vertexBuffer.VertexColors[facet.I2];

            // This has to move elsewhere
            var lightPos = new Vector3(0, 10, 10);

            // computing the cos of the angle between the light vector and the normal vector
            // it will return a value between 0 and 1 that will be used as the intensity of the color
            var nl0 = MathUtils.ComputeNDotL(
                vertexBuffer.WorldVertices[facet.I0],
                vertexBuffer.WorldVertexNormals[facet.I0],
                lightPos);
            var nl1 = MathUtils.ComputeNDotL(
                vertexBuffer.WorldVertices[facet.I1],
                vertexBuffer.WorldVertexNormals[facet.I1],
                lightPos);
            var nl2 = MathUtils.ComputeNDotL(
                vertexBuffer.WorldVertices[facet.I2],
                vertexBuffer.WorldVertexNormals[facet.I2],
                lightPos);

            var perPixelColors = new List<(int x, int y, int z, ColorRGB color)>(pixels.Count);
            for(var i = 0; i < pixels.Count; i++)
            {
                var alpha = barycentricPoints[i].X;
                var beta = barycentricPoints[i].Y;
                var gamma = barycentricPoints[i].Z;
                var lightContribution = (alpha * nl0 + beta * nl1 + gamma * nl2).Clamp();

                //var maxR = Math.Max(color0.R, Math.Max(color1.R, color2.R));
                //var maxG = Math.Max(color0.G, Math.Max(color1.G, color2.G));
                //var maxB = Math.Max(color0.B, Math.Max(color1.B, color2.B));
                // interpolate
                var r = (byte)(alpha * color0.R + beta * color1.R + gamma * color2.R);//.Clamp(0, maxR);
                var g = (byte)(alpha * color0.G + beta * color1.G + gamma * color2.G);//.Clamp(0, maxG);
                var b = (byte)(alpha * color0.B + beta * color1.B + gamma * color2.B);//.Clamp(0, maxB);
                var color = new ColorRGB(r, g, b, 255);

                perPixelColors.Add(((int)pixels[i].X, (int)pixels[i].Y, (int)pixels[i].Z, lightContribution * color));
            }

            WriteToBuffer(frameBuffer, pixels, perPixelColors);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawTriangleTextured(Texture texture, VertexBuffer vertexBuffer, FrameBuffer frameBuffer, int faId, bool linearFiltering)
        {
            var facet = vertexBuffer.Drawable.Mesh.Facets[faId];

            var pixels = GetPixels(vertexBuffer, frameBuffer, facet);

            var barycentricPoints = Barycentric2d.ConvertToBarycentricPoints(pixels,
                vertexBuffer.ScreenPointVertices[facet.I0],
                vertexBuffer.ScreenPointVertices[facet.I1],
                vertexBuffer.ScreenPointVertices[facet.I2]);

            if(barycentricPoints == null)
                return;

            // Get the texture coordinates of each point of the triangle
            var uv0 = vertexBuffer.Drawable.Mesh.TexCoordinates[facet.I0];
            var uv1 = vertexBuffer.Drawable.Mesh.TexCoordinates[facet.I1];
            var uv2 = vertexBuffer.Drawable.Mesh.TexCoordinates[facet.I2];

            // This has to move elsewhere
            var lightPos = new Vector3(0, 10, 10);

            // computing the cos of the angle between the light vector and the normal vector
            // it will return a value between 0 and 1 that will be used as the intensity of the color
            var nl0 = MathUtils.ComputeNDotL(
                vertexBuffer.WorldVertices[facet.I0],
                vertexBuffer.WorldVertexNormals[facet.I0],
                lightPos);
            var nl1 = MathUtils.ComputeNDotL(
                vertexBuffer.WorldVertices[facet.I1],
                vertexBuffer.WorldVertexNormals[facet.I1],
                lightPos);
            var nl2 = MathUtils.ComputeNDotL(
                vertexBuffer.WorldVertices[facet.I2],
                vertexBuffer.WorldVertexNormals[facet.I2],
                lightPos);
            var perPixelColors = new List<(int x, int y, int z, ColorRGB color)>(pixels.Count);
            for(var i = 0; i < pixels.Count; i++)
            {
                var alpha = barycentricPoints[i].X;
                var beta = barycentricPoints[i].Y;
                var gamma = barycentricPoints[i].Z;
                var lightContribution = alpha * nl0 + beta * nl1 + gamma * nl2;

                var texX = uv0.X * alpha + uv1.X * beta + uv2.X * gamma;
                var texY = uv0.Y * alpha + uv1.Y * beta + uv2.Y * gamma;

                var color = linearFiltering ?
                    texture.GetPixelColorLinearFiltering(texX, texY) :
                    texture.GetPixelColorNearestFiltering(texX, texY);
                perPixelColors.Add(((int)pixels[i].X, (int)pixels[i].Y, (int)pixels[i].Z, lightContribution * color));
            }

            WriteToBuffer(frameBuffer, pixels, perPixelColors);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static List<Vector3> GetPixels(VertexBuffer vertexBuffer, FrameBuffer frameBuffer, Facet facet)
        {
            var result = new List<Vector3>();
            vertexBuffer.ScreenPointVertices[facet.I0] = frameBuffer.ToScreen3(vertexBuffer.ProjectionVertices[facet.I0]);
            vertexBuffer.ScreenPointVertices[facet.I1] = frameBuffer.ToScreen3(vertexBuffer.ProjectionVertices[facet.I1]);
            vertexBuffer.ScreenPointVertices[facet.I2] = frameBuffer.ToScreen3(vertexBuffer.ProjectionVertices[facet.I2]);

            var (i0, i1, i2) = PainterUtils.SortIndices(vertexBuffer.ScreenPointVertices, facet.I0, facet.I1, facet.I2);
            if(i0 == i1 || i1 == i2 || i2 == i0)
                return result;

            return ScanLine.ScanLineTriangle(vertexBuffer, frameBuffer.Height, frameBuffer.Width, i0, i1, i2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void WriteToBuffer(FrameBuffer frameBuffer, IReadOnlyCollection<Vector3> pixels,
            IReadOnlyList<(int x, int y, int z, ColorRGB color)> perPixelColors)
        {
            lock(frameBuffer)
            {
                for(var i = 0; i < pixels.Count; i++)
                {
                    var pixel = perPixelColors[i];
                    frameBuffer.PutPixel(pixel.x, pixel.y, pixel.z, pixel.color);
                }
            }
        }
    }
}
