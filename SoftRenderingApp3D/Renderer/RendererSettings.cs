namespace SoftRenderingApp3D.Renderer
{
    public class RendererSettings
    {
        public bool Blur = false;
        public bool BackFaceCulling { get; set; }
        public bool ShowTriangleNormals { get; set; }
        public bool ShowTriangles { get; set; }
        public bool ShowXzGrid { get; set; }
        public bool ShowAxes { get; set; }
        public bool ShowTextures { get; set; }
        public bool LinearTextureFiltering { get; set; }
        public int ActiveTexture { get; set; }

        public int Translucency { get; set; }

        public void ChangeActiveTexture()
        {
            ActiveTexture += 1;
        }
    }
}
