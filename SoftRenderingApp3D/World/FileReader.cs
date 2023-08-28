using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftRenderingApp3D {
    public abstract class FileReader {
        public abstract IEnumerable<Volume> ReadFile(string fileName);
    }
}
