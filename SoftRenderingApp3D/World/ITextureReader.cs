using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinForms3D.World {
    interface ITextureReader {

        public Texture ReadImage(string path);
        
    }
}
