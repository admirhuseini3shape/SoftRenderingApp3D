using SoftRenderingApp3D.DataStructures.Drawables;
using SoftRenderingApp3D.DataStructures.Materials;
using SoftRenderingApp3D.DataStructures.Meshes;
using System.Collections.Generic;
using System.Numerics;

namespace SoftRenderingApp3D.DataStructures.Shapes

{
    public class TruncatedOctahedron
    {
     private static readonly Vector3[] _vertices = {
            new Vector3(0f, 0f, 1.054093f),
            new Vector3(0.6324555f, 0f, 0.843274f),
            new Vector3(-0.421637f, 0.4714045f, 0.843274f),
            new Vector3(-0.07027284f, -0.6285394f, 0.843274f),
            new Vector3(0.843274f, 0.4714045f, 0.421637f),
            new Vector3(0.5621827f, -0.6285394f, 0.6324555f),
            new Vector3(-0.9135469f, 0.3142697f, 0.421637f),
            new Vector3(-0.2108185f, 0.942809f, 0.421637f),
            new Vector3(-0.5621827f, -0.7856742f, 0.421637f),
            new Vector3(0.9838197f, 0.3142697f, -0.2108185f),
            new Vector3(0.421637f, 0.942809f, 0.2108185f),
            new Vector3(0.7027284f, -0.7856742f, 0f),
            new Vector3(-0.7027284f, 0.7856742f, 0f),
            new Vector3(-0.9838197f, -0.3142697f, 0.2108185f),
            new Vector3(-0.421637f, -0.942809f, -0.2108185f),
            new Vector3(0.5621827f, 0.7856742f, -0.421637f),
            new Vector3(0.9135469f, -0.3142697f, -0.421637f),
            new Vector3(0.2108185f, -0.942809f, -0.421637f),
            new Vector3(-0.5621827f, 0.6285394f, -0.6324555f),
            new Vector3(-0.843274f, -0.4714045f, -0.421637f),
            new Vector3(0.07027284f, 0.6285394f, -0.843274f),
            new Vector3(0.421637f, -0.4714045f, -0.843274f),
            new Vector3(-0.6324555f, 0f, -0.843274f),
            new Vector3(0f, 0f, -1.054093f),
        };

        private static readonly int[][] _faces = {
            new[] {0, 3, 5, 1},
            new[] {2, 7, 12, 6},
            new[] {4, 9, 15, 10},
            new[] {8, 13, 19, 14},
            new[] {11, 17, 21, 16},
            new[] {18, 20, 23, 22},
            new[] {0, 1, 4, 10, 7, 2},
            new[] {0, 2, 6, 13, 8, 3},
            new[] {1, 5, 11, 16, 9, 4},
            new[] {3, 8, 14, 17, 11, 5},
            new[] {6, 12, 18, 22, 19, 13},
            new[] {7, 10, 15, 20, 18, 12},
            new[] {9, 16, 21, 23, 20, 15},
            new[] {14, 19, 22, 23, 21, 17}
        };

        private static Facet[] GenerateTriangleIndices()
        {
            var triangleIndices = new List<int>();
            foreach (var face in _faces)
            {
                for (int i = 1; i < face.Length - 1; i++)
                {
                    triangleIndices.Add(face[0]);
                    triangleIndices.Add(face[i]);
                    triangleIndices.Add(face[i + 1]);
                }
            }
            return triangleIndices.ToArray().BuildTriangleIndices();
        }

        private static readonly Facet[] _triangleIndices = GenerateTriangleIndices();

        private static readonly Vector2[] _textureCoordinates = new Vector2[24];
        
        public static IDrawable GetDrawable()
        {
            var truncatedOctahedron = new Mesh(_vertices, _triangleIndices, null, _textureCoordinates);
            
            var triangleColorMaterial = new MaterialBase();
            
            
            return new Drawable(truncatedOctahedron, triangleColorMaterial);
        }
    }
}
