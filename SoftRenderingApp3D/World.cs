using SoftRenderingApp3D.DataStructures.Drawables;
using SoftRenderingApp3D.DataStructures.Textures;
using SoftRenderingApp3D.DataStructures.World;
using System.Collections.Generic;
using System.ComponentModel;

namespace SoftRenderingApp3D
{
    public class World : IWorld, INotifyPropertyChanged
    {
        public List<IDrawable> Drawables { get; set; } = new List<IDrawable>();
        public List<Texture> Textures { get; set; } = new List<Texture>();
        public List<ILightSource> LightSources { get; set; } = new List<ILightSource>();
        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged(object sender = null, string propertyName = null)
        {
            PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(propertyName));
        }
    }
}
