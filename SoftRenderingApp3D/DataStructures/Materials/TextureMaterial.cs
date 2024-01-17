using SoftRenderingApp3D.DataStructures.Textures;

namespace SoftRenderingApp3D.DataStructures.Materials
{
    public class TextureMaterial : MaterialBase, ITextureMaterial
    {
        public TextureMaterial(Texture texture, bool showTextures = true, bool linearTextureFiltering = true)
        {
            ShowTextures = showTextures;
            LinearTextureFiltering = linearTextureFiltering;
            Texture = texture;
        }

        public bool ShowTextures { get; set; }
        public bool LinearTextureFiltering { get; set; }
        public Texture Texture { get; }
    }
}
