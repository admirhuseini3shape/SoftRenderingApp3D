using SoftRenderingApp3D.Buffer;
using SoftRenderingApp3D.DataStructures.Materials;
using SoftRenderingApp3D.DataStructures.Textures;
using SoftRenderingApp3D.Utils;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace SoftRenderingApp3D.Painter
{
    public class GouraudPainter : IPainter
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawTriangle(VertexBuffer vertexBuffer, FrameBuffer frameBuffer, int faId)
        {
            vertexBuffer.Drawable.Mesh.Facets[faId].TransformWorld(vertexBuffer);

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

            var sortedIndices = PainterUtils.SortIndices(vertexBuffer.ScreenPointVertices, facet.I0, facet.I1, facet.I2);

            var pt0 = vertexBuffer.ScreenPointVertices[sortedIndices.i0];
            var pt1 = vertexBuffer.ScreenPointVertices[sortedIndices.i1];
            var pt2 = vertexBuffer.ScreenPointVertices[sortedIndices.i2];

            //if(pt0.IsNaN() || pt1.IsNaN() || pt2.IsNaN())
            //    return;

            var yStart = (int)Math.Max(pt0.Y, 0);
            var yEnd = (int)Math.Min(pt2.Y, frameBuffer.Height - 1);

            // Out if clipped
            if(yStart > yEnd)
            {
                return;
            }

            var yMiddle = MathUtils.Clamp((int)pt1.Y, yStart, yEnd);

            // This has to move elsewhere
            // var lightPos = RendererContext.Camera.GetType() == typeof(ArcBallCam) ? -(RendererContext.Camera as ArcBallCam).Position : -(RendererContext.Camera as FlyCam).Position;
            var lightPos = new Vector3(0, 10, 50);

            // computing the cos of the angle between the light vector and the normal vector
            // it will return a value between 0 and 1 that will be used as the intensity of the color

            var nl0 = MathUtils.ComputeNDotL(
                vertexBuffer.WorldVertices[sortedIndices.i0],
                vertexBuffer.WorldVertexNormals[sortedIndices.i0],
                lightPos);
            var nl1 = MathUtils.ComputeNDotL(
            vertexBuffer.WorldVertices[sortedIndices.i1],
            vertexBuffer.WorldVertexNormals[sortedIndices.i1],
            lightPos);
            var nl2 = MathUtils.ComputeNDotL(
                vertexBuffer.WorldVertices[sortedIndices.i2],
                vertexBuffer.WorldVertexNormals[sortedIndices.i2],
                lightPos);

            var color0 = vertexBuffer.VertexColors[sortedIndices.i0];
            var color1 = vertexBuffer.VertexColors[sortedIndices.i1];
            var color2 = vertexBuffer.VertexColors[sortedIndices.i2];
            var result = new List<(int x, int y, int z, ColorRGB Color)>();
            if(PainterUtils.Cross2D(pt0, pt1, pt2) > 0)
            {
                // P0
                //   P1
                // P2
                var firstHalf = PaintHalfTriangle(
                    frameBuffer.Width, yStart, (int)yMiddle - 1,
                    pt0, pt2, pt0, pt1, nl0, nl2, nl0, nl1,
                    pt0, pt1, pt2, color0, color1, color2);
                var secondHalf = PaintHalfTriangle(
                    frameBuffer.Width, (int)yMiddle, yEnd,
                    pt0, pt2, pt1, pt2, nl0, nl2, nl1, nl2,
                    pt0, pt1, pt2, color0, color1, color2);

                result.AddRange(firstHalf);
                result.AddRange(secondHalf);
            }
            else
            {
                //   P0
                // P1 
                //   P2
                var firstHalf = PaintHalfTriangle(
                    frameBuffer.Width, yStart, (int)yMiddle - 1,
                    pt0, pt1, pt0, pt2, nl0, nl1, nl0, nl2,
                    pt0, pt1, pt2, color0, color1, color2);
                var secondHalf = PaintHalfTriangle(
                    frameBuffer.Width, (int)yMiddle, yEnd,
                    pt1, pt2, pt0, pt2, nl1, nl2, nl0, nl2,
                    pt0, pt1, pt2, color0, color1, color2);
                result.AddRange(firstHalf);
                result.AddRange(secondHalf);
            }

            lock(frameBuffer)
            {
                for(var i = 0; i < result.Count; i++)
                {
                    var pixel = result[i];
                    frameBuffer.PutPixel(pixel.x, pixel.y, pixel.z, pixel.Color);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static List<(int, int, int, ColorRGB)> PaintHalfTriangle(
            float frameWidth, int yStart, int yEnd,
            Vector3 pa, Vector3 pb, Vector3 pc, Vector3 pd,
            float nla, float nlb, float nlc, float nld,
            Vector3 pt0, Vector3 pt1, Vector3 pt2, ColorRGB color0, ColorRGB color1, ColorRGB color2)
        {
            var result = new List<(int, int, int, ColorRGB)>();
            var mg1 = Math.Abs(pa.Y - pb.Y) < float.Epsilon ? 1f : 1 / (pb.Y - pa.Y);
            var mg2 = Math.Abs(pd.Y - pc.Y) < float.Epsilon ? 1f : 1 / (pd.Y - pc.Y);

            for(var y = yStart; y <= yEnd; y++)
            {
                var gradient1 = ((y - pa.Y) * mg1).Clamp();
                var gradient2 = ((y - pc.Y) * mg2).Clamp();

                var sx = MathUtils.Lerp(pa.X, pb.X, gradient1);
                var ex = MathUtils.Lerp(pc.X, pd.X, gradient2);

                if(sx >= ex)
                {
                    continue;
                }

                var sl = MathUtils.Lerp(nla, nlb, gradient1);
                var el = MathUtils.Lerp(nlc, nld, gradient2);

                var sz = MathUtils.Lerp(pa.Z, pb.Z, gradient1);
                var ez = MathUtils.Lerp(pc.Z, pd.Z, gradient2);

                var line = PaintScanLine(frameWidth, y, sx, ex, sz, ez, sl, el,
                    pt0, pt1, pt2, color0, color1, color2);
                result.AddRange(line);
            }
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static List<(int, int, int, ColorRGB)> PaintScanLine(float frameWidth,
            float y, float sx, float ex, float sz, float ez, float sl, float el,
            Vector3 pt0, Vector3 pt1, Vector3 pt2, ColorRGB color0, ColorRGB color1, ColorRGB color2)
        {
            var result = new List<(int, int, int, ColorRGB)>();
            var minX = Math.Max(sx, 0);
            var maxX = Math.Min(ex, frameWidth);

            var mx = 1 / (ex - sx);

            for(var x = minX; x < maxX; x++)
            {
                var gradient = (x - sx) * mx;

                var z = MathUtils.Lerp(sz, ez, gradient);
                var c = MathUtils.Lerp(sl, el, gradient);

                var pointInTriangle = new Vector3(x, y, z);

                //calculate barycentric weight for each vertex
                var barycentric = GetBarycentricCoordinates(pointInTriangle, pt0, pt1, pt2);
                var color = InterpolateColor(barycentric, color0, color1, color2);

                //Console.WriteLine($"newColor {newColor}. alpha {newColor.Alpha}");
                result.Add(((int)x, (int)y, (int)z, c * color));
            }
            return result;
        }

        private static ColorRGB InterpolateColor(Vector3 barycentric, ColorRGB c0, ColorRGB c1, ColorRGB c2)
        {
            var maxR = Math.Max(c0.R, Math.Max(c1.R, c2.R));
            var maxG = Math.Max(c0.G, Math.Max(c1.G, c2.G));
            var maxB = Math.Max(c0.B, Math.Max(c1.B, c2.B));
            // interpolate
            var R = (int)(barycentric.X * c0.R + barycentric.Y * c1.R + barycentric.Z * c2.R).Clamp(0, maxR);
            var G = (int)(barycentric.X * c0.G + barycentric.Y * c1.G + barycentric.Z * c2.G).Clamp(0, maxG);
            var B = (int)(barycentric.X * c0.B + barycentric.Y * c1.B + barycentric.Z * c2.B).Clamp(0, maxB);
            var finalColor = new ColorRGB((byte)R, (byte)G, (byte)B, 255);


            return finalColor;
        }

        private static bool CheckIfBarycentricOutsideTriangle(Vector3 barycentric)
        {
            return barycentric.X < 0 || barycentric.X > 1
                                     || barycentric.Y < 0 || barycentric.Y > 1
                                     || barycentric.Z < 0 || barycentric.Z > 1
                                     || barycentric.X + barycentric.Y + barycentric.Z > 1;
        }

        // deals with edge cases in the scanline algorithm
        private static Vector3 GetAdjustedBarycentric(Vector3 barycentric)
        {
            if(barycentric.X > 1)
            {
                return new Vector3(1, 0, 0);
            }

            if(barycentric.Y > 1)
            {
                return new Vector3(0, 1, 0);
            }

            if(barycentric.Z > 1)
            {
                return new Vector3(0, 0, 1);
            }

            if(barycentric.X < 0)
            {
                return new Vector3(0, barycentric.Y, barycentric.Z);
            }

            if(barycentric.Y < 0)
            {
                return new Vector3(barycentric.X, 0, barycentric.Z);
            }

            if(barycentric.Z < 0)
            {
                return new Vector3(barycentric.X, barycentric.Y, 0);
            }

            if(barycentric.X + barycentric.Y + barycentric.Z > 1)
            {
                var sumDenom = 1 / (barycentric.X + barycentric.Y + barycentric.Z);
                return new Vector3(barycentric.X * sumDenom, barycentric.Y * sumDenom, barycentric.Z * sumDenom);
            }

            throw new Exception("something not good if this happens");
        }


        private static Vector3 GetBarycentricCoordinates(Vector3 p, Vector3 pt0, Vector3 pt1, Vector3 pt2)
        {
            var barycentric = CalculateBarycentricCoordinates(p, pt0, pt1, pt2);
            return CheckIfBarycentricOutsideTriangle(barycentric) ?
                GetAdjustedBarycentric(barycentric)
                : barycentric;
        }

        // Efficient calculation of barycentric coordinates
        // taken from https://gamedev.stackexchange.com/a/23745
        private static Vector3 CalculateBarycentricCoordinates(Vector3 p, Vector3 v0, Vector3 v1, Vector3 v2)
        {
            var n = Vector3.Cross(v1 - v0, v2 - v0);
            var na = Vector3.Cross(v2 - v1, p - v1);
            var nb = Vector3.Cross(v0 - v2, p - v2);
            var nc = Vector3.Cross(v1 - v0, p - v0);

            var normFactor = 1 / Vector3.Dot(n, n);

            var alpha = Vector3.Dot(n, na) * normFactor;
            var beta = Vector3.Dot(n, nb) * normFactor;
            var gamma = Vector3.Dot(n, nc) * normFactor;

            return new Vector3(alpha, beta, gamma);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawTriangleTextured(Texture texture, VertexBuffer vertexBuffer, FrameBuffer frameBuffer, int faId, bool linearFiltering)
        {
            var mesh = vertexBuffer.Drawable.Mesh;
            mesh.Facets[faId].TransformWorld(vertexBuffer);

            var facet = vertexBuffer.Drawable.Mesh.Facets[faId];

            vertexBuffer.ScreenPointVertices[facet.I0] = frameBuffer.ToScreen3(vertexBuffer.ProjectionVertices[facet.I0]);
            vertexBuffer.ScreenPointVertices[facet.I1] = frameBuffer.ToScreen3(vertexBuffer.ProjectionVertices[facet.I1]);
            vertexBuffer.ScreenPointVertices[facet.I2] = frameBuffer.ToScreen3(vertexBuffer.ProjectionVertices[facet.I2]);

            var sortedIndices = PainterUtils.SortIndices(vertexBuffer.ScreenPointVertices, facet.I0, facet.I1, facet.I2);

            var i0 = sortedIndices.i0;
            var i1 = sortedIndices.i1;
            var i2 = sortedIndices.i2;

            var result = ScanLineTriangle(vertexBuffer, frameBuffer, i0, i1, i2);

            var p0 = vertexBuffer.ScreenPointVertices[i0];
            var p1 = vertexBuffer.ScreenPointVertices[i1];
            var p2 = vertexBuffer.ScreenPointVertices[i2];
            

            var delta21 = p2 - p1;
            var delta10 = p1 - p0;
            var delta02 = p0 - p2;
            var numAlpha = delta10.X * delta21.Y - delta10.Y * delta21.X;
            var numBeta = delta21.X * delta02.Y - delta21.Y * delta02.X;

            var barycentricPoints = new List<Vector4>(result.Count);
            for(var i = 0; i < result.Count; i++)
            {
                var x = result[i].X;
                var y = result[i].Y;

                // Barycentric coordinates are calculated
                var alpha = (-(x - p1.X) * delta21.Y + (y - p1.Y) * delta21.X) / numAlpha;
                var beta = (-(x - p2.X) * delta02.Y + (y - p2.Y) * delta02.X) / numBeta;
                var gamma = 1 - alpha - beta;

                barycentricPoints.Add(new Vector4(alpha, beta, gamma, result[i].W));
            }

            // Get the texture coordinates of each point of the triangle
            var uv0 = mesh.TexCoordinates[i0];
            var uv1 = mesh.TexCoordinates[i1];
            var uv2 = mesh.TexCoordinates[i2];

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

        private static List<Vector4> ScanLineTriangle(VertexBuffer vertexBuffer, FrameBuffer frameBuffer, int i0, int i1, int i2)
        {
            var result = new List<Vector4>();

            var p0 = vertexBuffer.ScreenPointVertices[i0];
            var p1 = vertexBuffer.ScreenPointVertices[i1];
            var p2 = vertexBuffer.ScreenPointVertices[i2];

            //if(p0.IsNaN() || p1.IsNaN() || p2.IsNaN())
            //    return;

            var yStart = (int)Math.Max(p0.Y, 0);
            var yEnd = (int)Math.Min(p2.Y, frameBuffer.Height - 1);

            // Out if clipped
            if(yStart > yEnd)
                return result;

            var yMiddle = MathUtils.Clamp((int)p1.Y, yStart, yEnd);

            // This has to move elsewhere
            var lightPos = new Vector3(0, 10, 10);

            // computing the cos of the angle between the light vector and the normal vector
            // it will return a value between 0 and 1 that will be used as the intensity of the color

            var nl0 = MathUtils.ComputeNDotL(
                vertexBuffer.WorldVertices[i0],
                vertexBuffer.WorldVertexNormals[i0],
                lightPos);
            var nl1 = MathUtils.ComputeNDotL(
                vertexBuffer.WorldVertices[i1],
                vertexBuffer.WorldVertexNormals[i1],
                lightPos);
            var nl2 = MathUtils.ComputeNDotL(
                vertexBuffer.WorldVertices[i2],
                vertexBuffer.WorldVertexNormals[i2],
                lightPos);
            if(PainterUtils.Cross2D(p0, p1, p2) > 0)
            {
                // P0
                //   P1
                // P2
                var firstHalf = ScanLineHalfTriangle(frameBuffer.Width,
                    yStart, (int)yMiddle - 1, p0, p2, p0, p1, nl0, nl2, nl0, nl1);
                var secondHalf = ScanLineHalfTriangle(frameBuffer.Width,
                    (int)yMiddle, yEnd, p0, p2, p1, p2, nl0, nl2, nl1, nl2);
                result.AddRange(firstHalf);
                result.AddRange(secondHalf);
            }
            else
            {
                //   P0
                // P1 
                //   P2
                var firstHalf = ScanLineHalfTriangle(frameBuffer.Width, yStart, (int)yMiddle - 1,
                    p0, p1, p0, p2, nl0, nl1, nl0, nl2);
                var secondHalf = ScanLineHalfTriangle(frameBuffer.Width, (int)yMiddle,
                    yEnd, p1, p2, p0, p2, nl1, nl2, nl0, nl2);
                result.AddRange(firstHalf);
                result.AddRange(secondHalf);
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static List<Vector4> ScanLineHalfTriangle(
            float frameWidth, int yStart, int yEnd, Vector3 pa, Vector3 pb,
            Vector3 pc, Vector3 pd, float nla, float nlb, float nlc, float nld)
        {
            var result = new List<Vector4>();
            var mg1 = Math.Abs(pa.Y - pb.Y) < float.Epsilon ? 1f : 1 / (pb.Y - pa.Y);
            var mg2 = Math.Abs(pd.Y - pc.Y) < float.Epsilon ? 1f : 1 / (pd.Y - pc.Y);


            for(var y = yStart; y <= yEnd; y++)
            {
                var gradient1 = ((y - pa.Y) * mg1).Clamp();
                var gradient2 = ((y - pc.Y) * mg2).Clamp();

                var sx = MathUtils.Lerp(pa.X, pb.X, gradient1);
                var ex = MathUtils.Lerp(pc.X, pd.X, gradient2);

                if(sx >= ex)
                    continue;

                var sl = MathUtils.Lerp(nla, nlb, gradient1);
                var el = MathUtils.Lerp(nlc, nld, gradient2);

                var sz = MathUtils.Lerp(pa.Z, pb.Z, gradient1);
                var ez = MathUtils.Lerp(pc.Z, pd.Z, gradient2);

                var line = ScanLine(frameWidth, y, sx, ex, sz, ez, sl, el);
                result.AddRange(line);
            }
            return result;
        }

        /// <summary>
        ///     Colors a line of pixels using a texture.
        /// </summary>
        /// <param name="frameWidth"></param>
        /// <param name="y">The y coordinate of the line.</param>
        /// <param name="sx">The left x coordinate of the line.</param>
        /// <param name="ex">The right x coordinate of the line.</param>
        /// <param name="sz">The start z coordinate of the line.</param>
        /// <param name="ez">The end z coordinate of the line</param>
        /// <param name="sl">The start light amount.</param>
        /// <param name="el">The end light amount.</param>
        /// <param name="texture">The texture that will be used to color the line.</param>
        /// <param name="linearFiltering">
        ///     If true, color is linear interpolation of the neighbouring texels, if false, the closes
        ///     texel is selected as the color.
        /// </param>
        /// <param name="vertex0">Point A of the triangle on which the line lays.</param>
        /// <param name="vertex1">Point B of the triangle on which the line lays</param>
        /// <param name="vertex2">Point C of the triangle on which the line lays</param>
        /// <param name="uv0">Texture coordinates of point A.</param>
        /// <param name="uv1">Texture coordinates of point B.</param>
        /// <param name="uv2">Texture coordinates of point C.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static List<Vector4> ScanLine(
            float frameWidth, float y, float sx, float ex, float sz, float ez, float sl, float el)
        {
            var result = new List<Vector4>();
            var minX = Math.Max(sx, 0);
            var maxX = Math.Min(ex, frameWidth);

            var mx = 1 / (ex - sx);


            for(var x = minX; x < maxX; x++)
            {
                var gradient = (x - sx) * mx;

                var z = MathUtils.Lerp(sz, ez, gradient);
                var lightContribution = MathUtils.Lerp(sl, el, gradient);
                result.Add(new Vector4(x, y, z, lightContribution));
            }

            return result;
        }
    }
}
