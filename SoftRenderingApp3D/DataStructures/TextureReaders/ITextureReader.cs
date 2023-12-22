namespace SoftRenderingApp3D.DataStructures.TextureReaders {
    public interface ITextureReader {
        public Texture.Texture ReadImage(string path);
    }
}