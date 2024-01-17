using SoftRenderingApp3D.DataStructures.Textures;

namespace SoftRenderingApp3D.DataStructures.TextureReaders
{
    public interface ITextureReader
    {
        public Texture ReadImage(string path);
    }
}
