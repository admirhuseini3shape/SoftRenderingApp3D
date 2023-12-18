using SoftRenderingApp3D;
using SubsurfaceScattering.World;
using System.Collections.Generic;
using ILightSource = SubsurfaceScattering.World.ILightSource;

namespace SubsurfaceScattering {
    public class SubsurfaceScatteringWorld : ISubsurfaceScatteringWorld {
        public List<ISubsurfaceScatteringVolume> Volumes { get; set; }
        public List<Texture> Textures { get; set; }
        public List<ILightSource> LightSources { get; set; }

        public SubsurfaceScatteringWorld() {
            this.Volumes = new List<ISubsurfaceScatteringVolume>();
            this.LightSources = new List<ILightSource>();
            this.Textures = new List<Texture>();
        }
    }
}