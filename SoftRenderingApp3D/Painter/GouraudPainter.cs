using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace SoftRenderingApp3D {

    public class GouraudPainter : IPainter {
        public RenderContext RendererContext { get; set; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawTriangle(ColorRGB color, VertexBuffer vbx, int triangleIndice) {
            vbx.Volume.Triangles[triangleIndice].TransformWorld(vbx);

            var surface = RendererContext.Surface;
            PainterUtils.SortTrianglePoints(vbx, surface, triangleIndice, out var v0, out var v1, out var v2, out var index0, out var index1, out var index2);

            var p0 = v0.ScreenPoint; var p1 = v1.ScreenPoint; var p2 = v2.ScreenPoint;

            var yStart = (int)Math.Max(p0.Y, 0);
            var yEnd = (int)Math.Min(p2.Y, surface.Height - 1);

            // Out if clipped
            if(yStart > yEnd) return;

            var yMiddle = MathUtils.Clamp((int)p1.Y, yStart, yEnd);

            // This has to move elsewhere
            var lightPos = new Vector3(0, 10, 10);

            // computing the cos of the angle between the light vector and the normal vector
            // it will return a value between 0 and 1 that will be used as the intensity of the color

            var nl0 = MathUtils.ComputeNDotL(v0.WorldPoint, v0.WorldNormal, lightPos);
            var nl1 = MathUtils.ComputeNDotL(v1.WorldPoint, v1.WorldNormal, lightPos);
            var nl2 = MathUtils.ComputeNDotL(v2.WorldPoint, v2.WorldNormal, lightPos);

            if(PainterUtils.Cross2D(p0, p1, p2) > 0) {
                // P0
                //   P1
                // P2
                paintHalfTriangle(yStart, (int)yMiddle - 1, color, p0, p2, p0, p1, nl0, nl2, nl0, nl1);
                paintHalfTriangle((int)yMiddle, yEnd, color, p0, p2, p1, p2, nl0, nl2, nl1, nl2);
            }
            else {
                //   P0
                // P1 
                //   P2
                paintHalfTriangle(yStart, (int)yMiddle - 1, color, p0, p1, p0, p2, nl0, nl1, nl0, nl2);
                paintHalfTriangle((int)yMiddle, yEnd, color, p1, p2, p0, p2, nl1, nl2, nl0, nl2);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void paintHalfTriangle(int yStart, int yEnd, ColorRGB color, Vector3 pa, Vector3 pb, Vector3 pc, Vector3 pd, float nla, float nlb, float nlc, float nld) {
            var mg1 = pa.Y == pb.Y ? 1f : 1 / (pb.Y - pa.Y);
            var mg2 = pd.Y == pc.Y ? 1f : 1 / (pd.Y - pc.Y);

            for(var y = yStart; y <= yEnd; y++) {
                var gradient1 = ((y - pa.Y) * mg1).Clamp();
                var gradient2 = ((y - pc.Y) * mg2).Clamp();

                var sx = MathUtils.Lerp(pa.X, pb.X, gradient1);
                var ex = MathUtils.Lerp(pc.X, pd.X, gradient2);

                if(sx >= ex) continue;

                var sl = MathUtils.Lerp(nla, nlb, gradient1);
                var el = MathUtils.Lerp(nlc, nld, gradient2);

                var sz = MathUtils.Lerp(pa.Z, pb.Z, gradient1);
                var ez = MathUtils.Lerp(pc.Z, pd.Z, gradient2);

                paintScanline(y, sx, ex, sz, ez, sl, el, color);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void paintScanline(float y, float sx, float ex, float sz, float ez, float sl, float el, ColorRGB color) {
            var surface = RendererContext.Surface;

            var minX = Math.Max(sx, 0);
            var maxX = Math.Min(ex, surface.Width);

            var mx = 1 / (ex - sx);

            for(var x = minX; x < maxX; x++) {
                var gradient = (x - sx) * mx;

                var z = MathUtils.Lerp(sz, ez, gradient);
                var c = MathUtils.Lerp(sl, el, gradient);

                surface.PutPixel((int)x, (int)y, (int)z, c * color);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawTriangleTextured(Texture texture, VertexBuffer vbx, int triangleIndice, bool linearFiltering) {
            vbx.Volume.Triangles[triangleIndice].TransformWorld(vbx);

            var surface = RendererContext.Surface;
            PainterUtils.SortTrianglePoints(vbx, surface, triangleIndice, out var v0, out var v1, out var v2, out var index0, out var index1, out var index2);

            var p0 = v0.ScreenPoint; var p1 = v1.ScreenPoint; var p2 = v2.ScreenPoint;

            // Get the texture coordinates of each point of the triangle
            var texCoord0 = vbx.Volume.TexCoordinates[index0];
            var texCoord1 = vbx.Volume.TexCoordinates[index1];
            var texCoord2 = vbx.Volume.TexCoordinates[index2];


            var yStart = (int)Math.Max(p0.Y, 0);
            var yEnd = (int)Math.Min(p2.Y, surface.Height - 1);

            // Out if clipped
            if(yStart > yEnd) return;

            var yMiddle = MathUtils.Clamp((int)p1.Y, yStart, yEnd);

            // This has to move elsewhere
            var lightPos = new Vector3(0, 10, 10);

            // computing the cos of the angle between the light vector and the normal vector
            // it will return a value between 0 and 1 that will be used as the intensity of the color

            var nl0 = MathUtils.ComputeNDotL(v0.WorldPoint, v0.WorldNormal, lightPos);
            var nl1 = MathUtils.ComputeNDotL(v1.WorldPoint, v1.WorldNormal, lightPos);
            var nl2 = MathUtils.ComputeNDotL(v2.WorldPoint, v2.WorldNormal, lightPos);

            if(PainterUtils.Cross2D(p0, p1, p2) > 0) {
                // P0
                //   P1
                // P2
                paintHalfTriangleTextured(yStart, (int)yMiddle - 1, texture, p0, p2, p0, p1, nl0, nl2, nl0, nl1, linearFiltering, p0, p1, p2, texCoord0, texCoord1, texCoord2);
                paintHalfTriangleTextured((int)yMiddle, yEnd, texture, p0, p2, p1, p2, nl0, nl2, nl1, nl2, linearFiltering, p0, p1, p2, texCoord0, texCoord1, texCoord2);
            }
            else {
                //   P0
                // P1 
                //   P2
                paintHalfTriangleTextured(yStart, (int)yMiddle - 1, texture, p0, p1, p0, p2, nl0, nl1, nl0, nl2, linearFiltering, p0, p1, p2, texCoord0, texCoord1, texCoord2);
                paintHalfTriangleTextured((int)yMiddle, yEnd, texture, p1, p2, p0, p2, nl1, nl2, nl0, nl2, linearFiltering, p0, p1, p2, texCoord0, texCoord1, texCoord2);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void paintHalfTriangleTextured(int yStart, int yEnd, Texture texture, Vector3 pa, Vector3 pb, Vector3 pc, Vector3 pd, float nla, float nlb, float nlc, float nld, bool linearFiltering, Vector3 vertex0, Vector3 vertex1, Vector3 vertex2, Vector2 texCoord0, Vector2 texCoord1, Vector2 texCoord2) {
            var mg1 = pa.Y == pb.Y ? 1f : 1 / (pb.Y - pa.Y);
            var mg2 = pd.Y == pc.Y ? 1f : 1 / (pd.Y - pc.Y);


            for(var y = yStart; y <= yEnd; y++) {
                var gradient1 = ((y - pa.Y) * mg1).Clamp();
                var gradient2 = ((y - pc.Y) * mg2).Clamp();

                var sx = MathUtils.Lerp(pa.X, pb.X, gradient1);
                var ex = MathUtils.Lerp(pc.X, pd.X, gradient2);

                if(sx >= ex) continue;

                var sl = MathUtils.Lerp(nla, nlb, gradient1);
                var el = MathUtils.Lerp(nlc, nld, gradient2);

                var sz = MathUtils.Lerp(pa.Z, pb.Z, gradient1);
                var ez = MathUtils.Lerp(pc.Z, pd.Z, gradient2);

                paintScanlineTextured(y, sx, ex, sz, ez, sl, el, texture, linearFiltering, vertex0, vertex1, vertex2, texCoord0, texCoord1, texCoord2);

            }
        }

        /// <summary>
        /// Colors a line of pixels using a texture.
        /// </summary>
        /// <param name="y">The y coordinate of the line.</param>
        /// <param name="sx">The left x coordinate of the line.</param>
        /// <param name="ex">The right x coordinate of the line.</param>
        /// <param name="sz">The start z coordinate of the line.</param>
        /// <param name="ez">The end z coordinate of the line</param>
        /// <param name="sl">The start light amount.</param>
        /// <param name="el">The end light amount.</param>
        /// <param name="texture">The texture that will be used to color the line.</param>
        /// <param name="linearFiltering">If true, color is linear interpolation of the neighbouring texels, if false, the closes texel is selected as the color.</param>
        /// <param name="vertex0">Point A of the triangle on which the line lays.</param>
        /// <param name="vertex1">Point B of the triangle on which the line lays</param>
        /// <param name="vertex2">Point C of the triangle on which the line lays</param>
        /// <param name="texCoord0">Texture coordinates of point A.</param>
        /// <param name="texCoord1">Texture coordinates of point B.</param>
        /// <param name="texCoord2">Texture coordinates of point C.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void paintScanlineTextured(float y, float sx, float ex, float sz, float ez, float sl, float el, Texture texture, bool linearFiltering, Vector3 vertex0, Vector3 vertex1, Vector3 vertex2, Vector2 texCoord0, Vector2 texCoord1, Vector2 texCoord2) {
            var surface = RendererContext.Surface;

            var minX = Math.Max(sx, 0);
            var maxX = Math.Min(ex, surface.Width);

            var mx = 1 / (ex - sx);


            for(var x = minX; x < maxX; x++) {
                var gradient = (x - sx) * mx;

                var z = MathUtils.Lerp(sz, ez, gradient);
                var c = MathUtils.Lerp(sl, el, gradient);

                // TODO: implement the barycentric coordinates calculations using a precomputed matrix of the 3 points of the triangle
                // This can be optimes by using matrix vector multiplication

                // Barycentric coordinates are calculated
                var alpha = (-(x - vertex1.X) * (vertex2.Y - vertex1.Y) + (y - vertex1.Y) * (vertex2.X - vertex1.X)) /
                    (-(vertex0.X - vertex1.X) * (vertex2.Y - vertex1.Y) + (vertex0.Y - vertex1.Y) * (vertex2.X - vertex1.X));
                var beta = (-(x - vertex2.X) * (vertex0.Y - vertex2.Y) + (y - vertex2.Y) * (vertex0.X - vertex2.X)) /
                    (-(vertex1.X - vertex2.X) * (vertex0.Y - vertex2.Y) + (vertex1.Y - vertex2.Y) * (vertex0.X - vertex2.X));
                var gamma = 1 - alpha - beta;

                var texX = texCoord0.X * alpha + texCoord1.X * beta + texCoord2.X * gamma;
                var texY = texCoord0.Y * alpha + texCoord1.Y * beta + texCoord2.Y * gamma;

                if (linearFiltering)
                    surface.PutPixel((int)x, (int)y, (int)z, c * texture.GetPixelColorLinearFiltering(texX, texY));
                else
                    surface.PutPixel((int)x, (int)y, (int)z, c * texture.GetPixelColorNearestFiltering(texX, texY));


            }

            
        }

    }
}
