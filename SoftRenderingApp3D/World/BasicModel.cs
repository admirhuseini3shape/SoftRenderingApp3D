using System.IO;
using System.Linq;
using System.Numerics;

namespace SoftRenderingApp3D {
    public class BasicModel : IModel {
        public BasicModel(IVolume volume) {
            Volume = volume;
            Colors = Colors ?? Enumerable.Repeat(ColorRGB.Gray, volume.Triangles.Length).ToArray();
        }
        public ColorRGB[] Colors { get; set; }
        public IVolume Volume { get; private set; }
    }

}
