using SoftRenderingApp3D.Utils;
using System.Collections.Generic;
using System.Linq;

namespace SoftRenderingApp3D.DataStructures.Materials
{
    public class FacetColorMaterial : MaterialBase, IFacetColorMaterial
    {
        public FacetColorMaterial(IReadOnlyList<ColorRGB> facetColors)
        {
            FacetColors = facetColors;
        }

        public FacetColorMaterial(int facetCount, ColorRGB? color = null)
        {
            color ??= Constants.StandardColor;

            FacetColors = Enumerable.Repeat(color.Value, facetCount).ToArray();
        }

        public IReadOnlyList<ColorRGB> FacetColors { get; }
    }
}
