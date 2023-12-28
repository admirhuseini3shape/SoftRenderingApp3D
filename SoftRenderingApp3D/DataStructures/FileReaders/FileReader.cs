using System.Collections.Generic;

namespace SoftRenderingApp3D.DataStructures.FileReaders {
    public abstract class FileReader {
        public abstract IEnumerable<Volume.Volume> ReadFile(string fileName);
    }
}