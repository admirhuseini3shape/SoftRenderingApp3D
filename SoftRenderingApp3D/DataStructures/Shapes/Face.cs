using SoftRenderingApp3D.DataStructures.Volume;
using System.Linq;
using System.Numerics;

namespace SoftRenderingApp3D.DataStructures.Shapes {
    public class Face : Volume.Volume {
        public Face() : base(
            new[] {
                new ColoredVertex(new Vector3(1, 1, 1) - new Vector3(.5f, .5f, .5f)),
                new ColoredVertex(new Vector3(0, 1, 1) - new Vector3(.5f, .5f, .5f)),
                new ColoredVertex(new Vector3(0, 0, 1) - new Vector3(.5f, .5f, .5f))
            },
            new[] { 0, 1, 2 }.BuildTriangleIndices().ToArray()) {
        }
    }
}