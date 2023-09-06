using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SoftRenderingApp3D {
    public class TexturedModel : IModel {
        public TexturedModel(TexturedVolume volume, Texture texture) {
            this.texture = texture;
            this.volume = volume;
        }
        public IVolume volume { get; private set; }
        public Texture texture { get; private set; }

        public void ChangeTexture(Texture texture) {
            this.texture = texture;
        }

    }
}
