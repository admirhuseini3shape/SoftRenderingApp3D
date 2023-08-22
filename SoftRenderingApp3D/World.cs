using System.Collections.Generic;

namespace SoftRenderingApp3D {
    public class World : IWorld {
        public List<IVolume> Volumes { get; set; }
        public List<Texture> Textures { get; set; }
        public List<ILightSource> LightSources { get; set; }        

        public World() {
            this.Volumes = new List<IVolume>();
            this.LightSources = new List<ILightSource>();
            this.Textures = new List<Texture>();
        }
    }
}
