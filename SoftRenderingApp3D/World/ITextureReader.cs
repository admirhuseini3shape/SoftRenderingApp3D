using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftRenderingApp3D {
    public interface ITextureReader {

        public Texture ReadImage(string path);
        
    }
}
