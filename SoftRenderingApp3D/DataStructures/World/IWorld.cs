using SoftRenderingApp3D.DataStructures.Drawables;
using SoftRenderingApp3D.DataStructures.Textures;
using System.Collections.Generic;
using System.ComponentModel;

namespace SoftRenderingApp3D.DataStructures.World
{
    public interface IWorld : INotifyPropertyChanged
    {
        List<IDrawable> Drawables { get; set; }
        List<ILightSource> LightSources { get; set; }

        List<Texture> Textures { get; set; }
    }
}
