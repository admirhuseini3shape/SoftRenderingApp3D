using SoftRenderingApp3D.DataStructures.Meshes;
using SoftRenderingApp3D.DataStructures.Textures;
using SoftRenderingApp3D.DataStructures.World;
using System.Collections.Generic;
using System.ComponentModel;

namespace SoftRenderingApp3D
{
    public class World : IWorld, INotifyPropertyChanged
    {
        public List<IMesh> Meshes { get; set; } = new List<IMesh>();
        public List<Texture> Textures { get; set; } = new List<Texture>();
        public List<ILightSource> LightSources { get; set; } = new List<ILightSource>();
        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged(object sender = null, string propertyName = null)
        {
            PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(propertyName));
        }
    }
}
