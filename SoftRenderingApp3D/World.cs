using System.Collections.Generic;

namespace SoftRenderingApp3D {
    public class World : IWorld {
        public List<IModel> Models { get; set; }
        public List<ILightSource> LightSources { get; set; }        

        public World() {
            this.Models = new List<IModel>();
            this.LightSources = new List<ILightSource>();
        }
    }
}
