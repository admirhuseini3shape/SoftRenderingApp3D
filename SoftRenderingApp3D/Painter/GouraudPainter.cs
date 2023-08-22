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

            // Get the indices for the texture coordinates
            int texIndex0 = index0;
            int texIndex1 = index1;
            int texIndex2 = index2;

            var texCoord0 = vbx.Volume.TexCoordinates[texIndex0];
            var texCoord1 = vbx.Volume.TexCoordinates[texIndex1];
            var texCoord2 = vbx.Volume.TexCoordinates[texIndex2];


            var yStart = (int)Math.Max(p0.Y, 0);
            var yEnd = (int)Math.Min(p2.Y, surface.Height - 1);

            // Out if clipped
            if(yStart > yEnd) return;

            var yMiddle = MathUtils.Clamp((int)p1.Y, yStart, yEnd);
            
            // Calculates the change to the y-coordinate of the texture coordinates for each next pixel
            var yMiddleTexture = MathUtils.Clamp(texCoord1.Y, texCoord0.Y, texCoord2.Y);
            var yTextureChange1 = (yMiddleTexture - texCoord0.Y) / (yStart - yEnd);
            var yTextureChange2 = (texCoord2.Y - yMiddleTexture) / (yStart - yEnd);

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
                paintHalfTriangleTextured(yStart, (int)yMiddle - 1, texture, p0, p2, p0, p1, nl0, nl2, nl0, nl1, texCoord0, texCoord1, texCoord2, yTextureChange1, linearFiltering);
                paintHalfTriangleTextured((int)yMiddle, yEnd, texture, p0, p2, p1, p2, nl0, nl2, nl1, nl2, texCoord0, texCoord1, texCoord2, yTextureChange2, linearFiltering);
            }
            else {
                //   P0
                // P1 
                //   P2
                paintHalfTriangleTextured(yStart, (int)yMiddle - 1, texture, p0, p1, p0, p2, nl0, nl1, nl0, nl2, texCoord0, texCoord2, texCoord1, yTextureChange1, linearFiltering);
                paintHalfTriangleTextured((int)yMiddle, yEnd, texture, p1, p2, p0, p2, nl1, nl2, nl0, nl2, texCoord0, texCoord2, texCoord1, yTextureChange2, linearFiltering);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void paintHalfTriangleTextured(int yStart, int yEnd, Texture texture, Vector3 pa, Vector3 pb, Vector3 pc, Vector3 pd, float nla, float nlb, float nlc, float nld, Vector2 texCoord0, Vector2 texCoord1, Vector2 texCoord2, float yTexChange, bool linearFiltering) {
            var mg1 = pa.Y == pb.Y ? 1f : 1 / (pb.Y - pa.Y);
            var mg2 = pd.Y == pc.Y ? 1f : 1 / (pd.Y - pc.Y);

            var textureY = texCoord0.Y;

            for(var y = yStart; y <= yEnd; y++) {
                var gradient1 = ((y - pa.Y) * mg1).Clamp();
                var gradient2 = ((y - pc.Y) * mg2).Clamp();

                var sx = MathUtils.Lerp(pa.X, pb.X, gradient1);
                var ex = MathUtils.Lerp(pc.X, pd.X, gradient2);

                if(sx >= ex) continue;

                var sl = MathUtils.Lerp(nla, nlb, gradient1);
                var el = MathUtils.Lerp(nlc, nld, gradient2);

                var startTextureX = MathUtils.Lerp(texCoord0.X, texCoord1.X, gradient1);
                var endTextureX = MathUtils.Lerp(texCoord0.X, texCoord2.X, gradient2);

                var sz = MathUtils.Lerp(pa.Z, pb.Z, gradient1);
                var ez = MathUtils.Lerp(pc.Z, pd.Z, gradient2);

                paintScanlineTextured(y, sx, ex, sz, ez, sl, el, texture, startTextureX, endTextureX, textureY, linearFiltering);

                // Increase the y texture coordinate in each iteration
                textureY += yTexChange;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void paintScanlineTextured(float y, float sx, float ex, float sz, float ez, float sl, float el, Texture texture, float startTextureX, float endTextureX, float textureY, bool linearFiltering) {
            var surface = RendererContext.Surface;

            var minX = Math.Max(sx, 0);
            var maxX = Math.Min(ex, surface.Width);

            var mx = 1 / (ex - sx);

            var xTextureChange = (endTextureX - startTextureX) / (ex - sx);

            var textureX = startTextureX;

            for(var x = minX; x < maxX; x++) {
                var gradient = (x - sx) * mx;

                var z = MathUtils.Lerp(sz, ez, gradient);
                var c = MathUtils.Lerp(sl, el, gradient);

                if(linearFiltering) {
                    var color = texture.GetPixelColorLinearFiltering(textureX, textureY);
                    surface.PutPixel((int)x, (int)y, (int)z, c * color);
                }
                else {
                    var color = texture.GetPixelColorNearestFiltering(textureX, textureY);
                    surface.PutPixel((int)x, (int)y, (int)z, c * color);
                }

                textureX += xTextureChange;
            }
        }
    }
}
