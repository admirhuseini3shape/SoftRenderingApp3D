using SoftRenderingApp3D.DataStructures.Shapes;
using System;
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
        
        
        public static void CreateCubes(World world) {
            var d = 5;
            var s = 2;
            var r = new Random();
            for(var x = -d; x <= d; x += s)
                for(var y = -d; y <= d; y += s)
                    for(var z = -d; z <= d; z += s) {
                        world.Volumes.Add(
                            new Cube {
                                Position = new Vector3(x, y, z),
                                Rotation = new Rotation3D(
                                    r.Next(-90, 90),
                                    r.Next(-90, 90),
                                    r.Next(-90, 90)).ToRad()
                            });
                    }
        }
        
        public static void CreateSphere(World world) {
            var d = 5;
            var s = 2;
            var r = new Random();
            for(var x = -d; x <= d; x += s)
            for(var y = -d; y <= d; y += s)
            for(var z = -d; z <= d; z += s) {
                world.Volumes.Add(
                    new IcoSphere(2) {
                        Position = new Vector3(x, y, z),
                        Rotation = new Rotation3D(
                            r.Next(-90, 90),
                            r.Next(-90, 90),
                            r.Next(-90, 90)).ToRad()
                    });
            }
        }
    }
    
    
}
