using SoftRenderingApp3D.DataStructures.Meshes;
using SoftRenderingApp3D.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftRenderingApp3D.DataStructures.Materials
{
    public class VertexColorMaterial : MaterialBase, IVertexColorMaterial
    {
        private ColorRGB[] vertexColors;

        public VertexColorMaterial(ColorRGB[] vertexColors)
        {
            this.vertexColors = vertexColors;
        }

        public VertexColorMaterial(int vertexCount)
        {
            vertexColors = Enumerable.Repeat(Constants.StandardColor, vertexCount).ToArray();
        }

        public IReadOnlyList<ColorRGB> VertexColors => vertexColors;


        public override void Append(IMaterial material, int[] vertexMapping, int[] facetMapping)
        {
            if(!(material is IVertexColorMaterial vertexColorMaterial) ||
               vertexColorMaterial.VertexColors == null)
                return;

            var veCount = vertexColorMaterial.VertexColors.Count;

            Array.Resize(ref vertexColors, vertexColors.Length + veCount);
            for(var i = 0; i < veCount; i++)
            {
                vertexColors[vertexMapping[i]] = vertexColorMaterial.VertexColors[i];
            }
        }

        public override void Append(IReadOnlyList<IMaterial> materials,
            IReadOnlyList<int[]> vertexMappings, IReadOnlyList<int[]> facetMappings)
        {
            if(materials.Any(x => !(x is IVertexColorMaterial xMat) || xMat.VertexColors == null))
                return;

            var vertexColorMaterials = materials.Cast<IVertexColorMaterial>().ToArray();
            var totalVertexCount = vertexColorMaterials.Sum(x => x.VertexColors.Count);
            Array.Resize(ref vertexColors, vertexColors.Length + totalVertexCount);
            for(var k = 0; k < vertexColorMaterials.Length; k++)
            {
                var veCount = vertexColorMaterials[k].VertexColors.Count;

                for(var i = 0; i < veCount; i++)
                {
                    vertexColors[vertexMappings[k][i]] = vertexColorMaterials[k].VertexColors[i];
                }
            }
        }
    }
}
