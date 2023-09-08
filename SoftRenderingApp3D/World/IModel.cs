using SoftRenderingApp3D;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SoftRenderingApp3D {
    public interface IModel {
        IVolume Volume { get; }
    }
}
