using SoftRenderingApp3D;
using System.Collections.Generic;

namespace SubsurfaceScattering.World {
    public interface ISubsurfaceScatteringWorld {
        List<ISubsurfaceScatteringVolume> Volumes { get; set; }
        List<ILightSource> LightSources { get; set; }

        List<Texture> Textures { get; set; }
    }
}