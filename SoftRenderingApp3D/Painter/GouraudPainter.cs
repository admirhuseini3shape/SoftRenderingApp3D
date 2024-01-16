using SoftRenderingApp3D.Buffer;
using SoftRenderingApp3D.DataStructures;
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
        public List<FacetPixelData> DrawTriangle(VertexBuffer vertexBuffer, List<Vector3> pixels, int faId)
        {
            var perPixelColors = new List<FacetPixelData>(pixels.Count);

            var facet = vertexBuffer.Drawable.Mesh.Facets[faId];

            var barycentricPoints = Barycentric2d.ConvertToBarycentricPoints(pixels,
                vertexBuffer.ScreenPointVertices[facet.I0],
                vertexBuffer.ScreenPointVertices[facet.I1],
                vertexBuffer.ScreenPointVertices[facet.I2]);

            if(barycentricPoints == null)
                return perPixelColors;

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


            for(var i = 0; i < pixels.Count; i++)
            {
                var alpha = barycentricPoints[i].X;
                var beta = barycentricPoints[i].Y;
                var gamma = barycentricPoints[i].Z;
                var lightContribution = (alpha * nl0 + beta * nl1 + gamma * nl2).Clamp();

                // interpolate
                var r = (byte)(alpha * color0.R + beta * color1.R + gamma * color2.R);//.Clamp(0, maxR);
                var g = (byte)(alpha * color0.G + beta * color1.G + gamma * color2.G);//.Clamp(0, maxG);
                var b = (byte)(alpha * color0.B + beta * color1.B + gamma * color2.B);//.Clamp(0, maxB);
                var color = (lightContribution * new ColorRGB(r, g, b, 255)).Color;

                var pixelData = new FacetPixelData((int)pixels[i].X, (int)pixels[i].Y, pixels[i].Z, color, faId);
                perPixelColors.Add(pixelData);
            }

            return perPixelColors;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<FacetPixelData> DrawTriangleTextured(Texture texture, VertexBuffer vertexBuffer, List<Vector3> pixels, int faId, bool linearFiltering)
        {
            var perPixelColors = new List<FacetPixelData>(pixels.Count);

            var facet = vertexBuffer.Drawable.Mesh.Facets[faId];

            var barycentricPoints = Barycentric2d.ConvertToBarycentricPoints(pixels,
                vertexBuffer.ScreenPointVertices[facet.I0],
                vertexBuffer.ScreenPointVertices[facet.I1],
                vertexBuffer.ScreenPointVertices[facet.I2]);

            if(barycentricPoints == null)
                return perPixelColors;

            // Get the texture coordinates of each point of the triangle
            var uv0 = vertexBuffer.Drawable.Mesh.TextureCoordinates[facet.I0];
            var uv1 = vertexBuffer.Drawable.Mesh.TextureCoordinates[facet.I1];
            var uv2 = vertexBuffer.Drawable.Mesh.TextureCoordinates[facet.I2];

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

            for(var i = 0; i < pixels.Count; i++)
            {
                var alpha = barycentricPoints[i].X;
                var beta = barycentricPoints[i].Y;
                var gamma = barycentricPoints[i].Z;
                var lightContribution = alpha * nl0 + beta * nl1 + gamma * nl2;

                var texX = uv0.X * alpha + uv1.X * beta + uv2.X * gamma;
                var texY = uv0.Y * alpha + uv1.Y * beta + uv2.Y * gamma;

                var initialColor = linearFiltering ?
                    texture.GetPixelColorLinearFiltering(texX, texY) :
                    texture.GetPixelColorNearestFiltering(texX, texY);

                var color = (lightContribution * initialColor).Color;

                var pixelData = new FacetPixelData((int)pixels[i].X, (int)pixels[i].Y, pixels[i].Z, color, faId);
                perPixelColors.Add(pixelData);
            }

            return perPixelColors;
        }

    }
}
