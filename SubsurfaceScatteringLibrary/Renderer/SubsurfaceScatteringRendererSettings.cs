namespace SubsurfaceScatteringLibrary.Renderer {
    public class SubsurfaceScatteringRendererSettings {
        public bool blur = false;
        public bool BackFaceCulling { get; set; }
        public bool ShowTriangleNormals { get; set; }
        public bool ShowTriangles { get; set; }
        public bool ShowXZGrid { get; set; }
        public bool ShowAxes { get; set; }
        public bool ShowTextures { get; set; }
        public bool LinearTextureFiltering { get; set; }
        public int ActiveTexture { get; set; }

        public int translucency { get; set; }

        public void ChangeActiveTexture() {
            ActiveTexture += 1;
        }
    }
}