using SoftRenderingApp3D.DataStructures.Meshes;
using System.Collections.Generic;
using System.ComponentModel;

namespace SoftRenderingApp3D.DataStructures.World {
    public interface IWorld : INotifyPropertyChanged {
        List<IMesh> Meshes { get; set; }
        List<ILightSource> LightSources { get; set; }

        List<Texture.Texture> Textures { get; set; }
    }
}