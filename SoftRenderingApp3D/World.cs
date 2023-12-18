using SoftRenderingApp3D.DataStructures;
using System.Collections.Generic;

namespace SoftRenderingApp3D {
    public class World : IWorld {
        public List<IVolume> Volumes { get; set; } = new List<IVolume>();
        public List<Texture> Textures { get; set; } = new List<Texture>();
        public List<ILightSource> LightSources { get; set; } = new List<ILightSource>();
    }
}