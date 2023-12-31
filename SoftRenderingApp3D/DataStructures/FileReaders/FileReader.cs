using SoftRenderingApp3D.DataStructures.Meshes;
using System.Collections.Generic;

namespace SoftRenderingApp3D.DataStructures.FileReaders {
    public abstract class FileReader {
        public abstract IEnumerable<Mesh> ReadFile(string fileName);
    }
}