using SoftRenderingApp3D.DataStructures.Drawables;
using System.Collections.Generic;

namespace SoftRenderingApp3D.DataStructures.FileReaders
{
    public abstract class FileReader
    {
        public abstract IEnumerable<IDrawable> ReadFile(string fileName);
    }
}
