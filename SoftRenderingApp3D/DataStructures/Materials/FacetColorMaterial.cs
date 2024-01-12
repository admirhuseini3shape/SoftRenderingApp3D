using SoftRenderingApp3D.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftRenderingApp3D.DataStructures.Materials
{
    public class FacetColorMaterial : MaterialBase, IFacetColorMaterial
    {
        private ColorRGB[] facetColors;

        public FacetColorMaterial(ColorRGB[] facetColors)
        {
            this.facetColors = facetColors;
        }

        public FacetColorMaterial(int facetCount = 0, ColorRGB? color = null)
        {
            color ??= Constants.StandardColor;

            facetColors = Enumerable.Repeat(color.Value, facetCount).ToArray();
        }

        public IReadOnlyList<ColorRGB> FacetColors => facetColors;

        public override void Append(IMaterial material, int[] vertexMapping, int[] facetMapping)
        {
            if(!(material is IFacetColorMaterial facetColorMaterial) ||
               facetColorMaterial.FacetColors == null)
                return;

            var faCount = facetColorMaterial.FacetColors.Count;

            Array.Resize(ref facetColors, facetColors.Length + faCount);
            for(var i = 0; i < faCount; i++)
            {
                facetColors[facetMapping[i]] = facetColorMaterial.FacetColors[i];
            }
        }

        public override void Append(IReadOnlyList<IMaterial> materials,
            IReadOnlyList<int[]> vertexMappings, IReadOnlyList<int[]> facetMappings)
        {
            if(materials.Any(x => !(x is IFacetColorMaterial xMat) || xMat.FacetColors == null))
                return;

            var facetColorMaterials = materials.Cast<IFacetColorMaterial>().ToArray();
            var totalVertexCount = facetColorMaterials.Sum(x => x.FacetColors.Count);
            Array.Resize(ref facetColors, facetColors.Length + totalVertexCount);
            for(var k = 0; k < facetColorMaterials.Length; k++)
            {
                var faCount = facetColorMaterials[k].FacetColors.Count;

                for(var i = 0; i < faCount; i++)
                {
                    facetColors[facetMappings[k][i]] = facetColorMaterials[k].FacetColors[i];
                }
            }
        }
    }
}
