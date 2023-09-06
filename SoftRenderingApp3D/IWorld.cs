using System.Collections.Generic;

namespace SoftRenderingApp3D {
    public interface IWorld {
        List<IModel> Models { get; set; }
        List<ILightSource> LightSources { get; set; }
    }
}