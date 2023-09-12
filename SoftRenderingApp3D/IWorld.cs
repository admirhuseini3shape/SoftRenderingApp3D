using System.Collections.Generic;

namespace SoftRenderingApp3D {
    public interface IWorld {
        List<BasicModel> Models { get; set; }
        List<ILightSource> LightSources { get; set; }
    }
}