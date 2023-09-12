using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SoftRenderingApp3D {
    public class TexturedModel : BasicModel {
        public TexturedModel(Volume volume, List<Vector2> textureCoordinates,Texture texture): base (volume) {
            this.TextureCoordinates = textureCoordinates;
            Texture = texture;
        }

        public List<Vector2> TextureCoordinates;
        public Texture Texture { get; set; }

        public void ChangeTexture(Texture texture) {
            Texture = texture;
        }

    }
}
