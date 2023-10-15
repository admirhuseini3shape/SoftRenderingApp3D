using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;


namespace SoftRenderingApp3D {
    public class ColoredVertex {

        public ColoredVertex(Vector3 position) {
            this.position = position;
            this.color = ColorRGB.Black;
        }
        public Vector3 position { get; }

        public ColorRGB color { get; set; }

    }
}
