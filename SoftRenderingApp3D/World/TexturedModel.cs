using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SoftRenderingApp3D {
    public class TexturedModel : IModel {
        public TexturedModel(TexturedVolume volume, Texture texture) {
            Texture = texture;
            Volume = volume;
        }
        public IVolume Volume { get; private set; }
        public Texture Texture { get; private set; }

        public void ChangeTexture(Texture texture) {
            Texture = texture;
        }

    }
}
