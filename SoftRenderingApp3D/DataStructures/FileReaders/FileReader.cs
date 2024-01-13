using SoftRenderingApp3D.DataStructures.Drawables;

namespace SoftRenderingApp3D.DataStructures.FileReaders
{
    public abstract class FileReader
    {
        public abstract IDrawable ReadFile(string fileName);
    }
}
