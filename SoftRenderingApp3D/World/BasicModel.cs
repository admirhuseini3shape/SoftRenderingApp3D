using System.IO;
using System.Linq;
using System.Numerics;

namespace SoftRenderingApp3D {
    public class BasicModel : IModel {
        public BasicModel(BasicVolume volume) {
            this.volume = volume;
            this.colors = this.colors ?? Enumerable.Repeat(ColorRGB.Gray, volume.Triangles.Length).ToArray();
        }
        public ColorRGB[] colors { get; set; }
        public IVolume volume { get; private set; }
    }

}
