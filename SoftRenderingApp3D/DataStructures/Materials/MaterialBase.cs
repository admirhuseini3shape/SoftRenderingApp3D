namespace SoftRenderingApp3D.DataStructures.Materials
{
    public class MaterialBase : IMaterial
    {
        public bool BackFaceCulling { get; set; }
        public bool ShowTriangleNormals { get; set; }
        public bool ShowVertexNormals { get; set; }
        public bool WireFrame { get; set; }
        public bool Opacity { get; set; }
    }
}
