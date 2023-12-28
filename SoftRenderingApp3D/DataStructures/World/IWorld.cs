using SoftRenderingApp3D.DataStructures.Volume;
using System.Collections.Generic;

namespace SoftRenderingApp3D.DataStructures.World {
    public interface IWorld {
        List<IVolume> Volumes { get; set; }
        List<ILightSource> LightSources { get; set; }

        List<Texture.Texture> Textures { get; set; }
    }
}