namespace SoftRenderingApp3D.DataStructures
{
    public struct FacetPixelData
    {
        public int xScreen; 
        public int yScreen;
        public float zDepth;
        public int ColorAsInt;
        public int FacetId;

        public FacetPixelData(int x, int y, float z, int color, int faId)
        {
            xScreen = x;
            yScreen = y;
            zDepth = z;
            ColorAsInt = color;
            FacetId = faId;
        }
    }
}
