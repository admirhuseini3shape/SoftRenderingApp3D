
namespace SoftRenderingApp3D.DataStructures.Materials
{
    public interface IMaterialOptions
    {
        public bool BackFaceCulling { get; set; }
        public bool ShowTriangleNormals { get; set; }
        public bool ShowVertexNormals { get; set; }
        public bool WireFrame { get; set; }
        public bool Opacity { get; set; }
    }

    public interface ITextureMaterialOptions
    {
        public bool ShowTextures { get; set; }
        public bool LinearTextureFiltering { get; set; }
    }
}
