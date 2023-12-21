using System.Collections.Generic;

namespace SoftRenderingApp3D.DataStructures {
    public abstract class FileReader {
        public abstract IEnumerable<Volume> ReadFile(string fileName);
    }
}
