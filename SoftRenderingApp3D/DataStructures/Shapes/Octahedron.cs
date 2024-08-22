using SoftRenderingApp3D.DataStructures.Drawables;
using SoftRenderingApp3D.DataStructures.Materials;
using SoftRenderingApp3D.DataStructures.Meshes;
using System.Numerics;

namespace SoftRenderingApp3D.DataStructures.Shapes
{
    public static class Octahedron
    {
        private static readonly Vector3[] _vertices = {
            new Vector3(1, 0, 0),
            new Vector3(-1, 0, 0),
            new Vector3(0, 1, 0),
            new Vector3(0, -1, 0),
            new Vector3(0, 0, 1),
            new Vector3(0, 0, -1)
        };

        private static readonly Facet[] _triangleIndices = new[] {
                0, 2, 4,
                2, 1, 4,
                1, 3, 4,
                3, 0, 4,
                0, 2, 5,
                2, 1, 5,
                1, 3, 5,
                3, 0, 5
            }
            .BuildTriangleIndices();

        private static readonly Vector2[] _textureCoordinates = {
            new Vector2(1.0f, 0f), new Vector2(0f, 1.0f),
            new Vector2(0.0f, 0f), new Vector2(0f, 0f),
            new Vector2(0f, 0f), new Vector2(0f, 0f), 

        };

        private static readonly ColorRGB[] _triangleColors = {
            new ColorRGB(255,255,255), new ColorRGB(240,240,240),
            new ColorRGB(224, 224,224), new ColorRGB(208, 208,208),
            new ColorRGB(255,255,255), new ColorRGB(240,240,240),
            new ColorRGB(224, 224,224), new ColorRGB(208, 208,208)
        };
        
        
        public static IDrawable GetDrawable()
        {
            var octahedron = new Mesh(_vertices, _triangleIndices, null, _textureCoordinates);
    
        
            var triangleColorMaterial = new FacetColorMaterial(_triangleColors);
        
            return new Drawable(octahedron, triangleColorMaterial);
        }
    }
}
