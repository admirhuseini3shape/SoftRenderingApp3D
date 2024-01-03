using SoftRenderingApp3D.Utils;
using System.Collections.Generic;
using System.Linq;

namespace SoftRenderingApp3D.DataStructures.Materials
{
    public class VertexColorMaterial : MaterialBase, IVertexColorMaterial
    {
        public VertexColorMaterial(IReadOnlyList<ColorRGB> vertexColors)
        {
            VertexColors = vertexColors;
        }

        public VertexColorMaterial(int vertexCount)
        {
            VertexColors = Enumerable.Repeat(Constants.StandardColor, vertexCount).ToArray();
        }

        public IReadOnlyList<ColorRGB> VertexColors { get; }
    }
}
