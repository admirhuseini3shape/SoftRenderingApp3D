using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftRenderingApp3D {
    public class  FileNotSupported : Exception {
        public FileNotSupported(string error) : base(error) {

        }
    }
    public static class ModelReader {
        public static IEnumerable<BasicModel> ReadFile(string path) {
            if(path.Contains(".stl")) {
                return new STLReader().ReadFile(path);
            }
            if(path.Contains(".dae")) {
                return new ColladaReader().ReadFile(path);
            }
            else {
                throw new FileNotSupported($"The file type {new FileInfo(path).Extension} is not supported!");
            }
        }
    }
}
