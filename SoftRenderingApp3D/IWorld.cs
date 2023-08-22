using System.Collections.Generic;

namespace SoftRenderingApp3D {
    public interface IWorld {
        List<IVolume> Volumes { get; set; }
        List<ILightSource> LightSources { get; set; }

        List<Texture> Textures { get; set; }
    }
}