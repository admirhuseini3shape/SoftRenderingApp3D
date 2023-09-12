using System.IO;
using System.Linq;
using System.Numerics;

namespace SoftRenderingApp3D {
    public class BasicModel {
        public BasicModel(Volume volume) {
            Volume = volume;
            Colors = Colors ?? Enumerable.Repeat(ColorRGB.Gray, volume.Triangles.Length).ToArray();
        }
        public ColorRGB[] Colors { get; set; }
        public Volume Volume { get; private set; }
    }

}
