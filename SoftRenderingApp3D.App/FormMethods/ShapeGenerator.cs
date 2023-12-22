using SoftRenderingApp3D.DataStructures.Shapes;
using System.Numerics;

namespace SoftRenderingApp3D.App.FormMethods {
    public class ShapeGenerator {
        public static void CreateTown(World world) {
            var d = 50;
            var s = 2;
            for(var x = -d; x <= d; x += s)
                for(var z = -d; z <= d; z += s) {
                    world.Volumes.Add(
                        new Cube {
                            Position = new Vector3(x, 0, z)
                            //Scale = new Vector3(1, r.Next(1, 50), 1)
                        });
                }
        }
    }
}
