namespace SoftRenderingApp3D.Renderer {
    public class RendererSettings {
        public bool blur = false;
        public bool BackFaceCulling { get; set; }
        public bool ShowTriangleNormals { get; set; }
        public bool ShowTriangles { get; set; }
        public bool ShowXZGrid { get; set; }
        public bool ShowAxes { get; set; }
        public bool ShowTextures { get; set; }
        public bool LiearTextureFiltering { get; set; }
        public int activeTexture { get; set; }

        public int translucency { get; set; }

        public void ChangeActiveTexture() {
            activeTexture += 1;
        }
    }
}