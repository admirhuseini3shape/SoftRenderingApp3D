using SoftRenderingApp3D.DataStructures.Drawables;
using SoftRenderingApp3D.DataStructures.Shapes;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace SoftRenderingApp3D.App.Utils
{
    public static class ShapeGenerator
    {
        public static List<IDrawable> CreateCube()
        {
            return new List<IDrawable> { Cube.GetDrawable() };
        }
        public static List<IDrawable> CreateBigCube()
        {
            var cube = Cube.GetDrawable();
            var scaling = new Vector3(100, 100, 100);
            var matrix = Matrix4x4.CreateScale(scaling);
            cube.Mesh.Transform(matrix);

            return new List<IDrawable> { cube };
        }

        public static List<IDrawable> CreateCubes()
        {
            const int d = 5;
            const int s = 2;
            const int maxAngle = 0;
            const int loopCount = 3;
            var r = new Random();

            var capacity = (int)Math.Pow((2.0 * d) / s, loopCount);
            var result = new List<IDrawable>(capacity);

            for(var x = -d; x <= d; x += s)
                for(var y = -d; y <= d; y += s)
                    for(var z = -d; z <= d; z += s)
                    {
                        var cube = Cube.GetDrawable();

                        var rotation = new Rotation3D(
                                r.Next(-maxAngle, maxAngle),
                                r.Next(-maxAngle, maxAngle),
                                r.Next(-maxAngle, maxAngle))
                            .ToRad();
                        var position = new Vector3(x, y, z);
                        var rotationMatrix = Matrix4x4.CreateFromYawPitchRoll(
                            rotation.YYaw, rotation.XPitch, rotation.ZRoll);
                        var matrix = rotationMatrix * Matrix4x4.CreateTranslation(position);
                        cube.Mesh.Transform(matrix);
                        result.Add(cube);
                    }

            return result;
        }
        public static List<IDrawable> CreateTown()
        {
            const int d = 50;
            const int s = 2;
            const int loopCount = 2;
            var translateToOriginY = new Vector3(0, .5f, 0);
            var random = new Random();

            var capacity = (int)Math.Pow((2.0 * d) / s, loopCount);
            var result = new List<IDrawable>(capacity);

            for(var x = -d; x <= d; x += s)
                for(var z = -d; z <= d; z += s)
                {
                    var cube = Cube.GetDrawable();

                    var position = new Vector3(x, 0, z);
                    var scaling = new Vector3(1, random.Next(1, 10), 1);
                    var matrix = Matrix4x4.CreateTranslation(translateToOriginY) *
                                 Matrix4x4.CreateScale(scaling) *
                                 Matrix4x4.CreateTranslation(position);
                    cube.Mesh.Transform(matrix);
                    result.Add(cube);
                }

            return result;
        }

        public static List<IDrawable> CreateLittleTown()
        {
            const int d = 10;
            const int s = 2;
            const int loopCount = 2;

            var translateToOriginY = new Vector3(0, .5f, 0);
            var random = new Random();

            var capacity = (int)Math.Pow((2.0 * d) / s, loopCount);
            var result = new List<IDrawable>(capacity);

            for(var x = -d; x <= d; x += s)
                for(var z = -d; z <= d; z += s)
                {
                    var cube = Cube.GetDrawable();
                    var position = new Vector3(x, 0, z);
                    var scaling = new Vector3(1, random.Next(1, 10), 1);

                    var matrix = Matrix4x4.CreateTranslation(translateToOriginY) *
                                 Matrix4x4.CreateScale(scaling) *
                                 Matrix4x4.CreateTranslation(position);
                    cube.Mesh.Transform(matrix);
                    result.Add(cube);
                }

            return result;
        }

        public static List<IDrawable> CreateBigTown()
        {
            const int d = 200;
            const int s = 2;
            const int loopCount = 2;

            var translateToOriginY = new Vector3(0, .5f, 0);
            var random = new Random();

            var capacity = (int)Math.Pow((2.0 * d) / s, loopCount);
            var result = new List<IDrawable>(capacity);

            for(var x = -d; x <= d; x += s)
                for(var z = -d; z <= d; z += s)
                {
                    var cube = Cube.GetDrawable();
                    var position = new Vector3(x, 0, z);
                    var scaling = new Vector3(1, random.Next(1, 10), 1);
                    var matrix = Matrix4x4.CreateTranslation(translateToOriginY) *
                                 Matrix4x4.CreateScale(scaling) *
                                 Matrix4x4.CreateTranslation(position);
                    cube.Mesh.Transform(matrix);
                    result.Add(cube);
                }

            return result;
        }

        public static List<IDrawable> CreateSphere()
        {
            const int d = 5;
            const int s = 2;
            const int loopCount = 2;

            var r = new Random();
            var capacity = (int)Math.Pow((2.0 * d) / s, loopCount);
            var result = new List<IDrawable>(capacity);

            for(var x = -d; x <= d; x += s)
                for(var y = -d; y <= d; y += s)
                    for(var z = -d; z <= d; z += s)
                    {
                        var sphere = Sphere.GetDrawable(2);
                        var rotation = new Rotation3D(
                                r.Next(-90, 90),
                                r.Next(-90, 90),
                                r.Next(-90, 90))
                            .ToRad();
                        var position = new Vector3(x, y, z);
                        var rotationMatrix = Matrix4x4.CreateFromYawPitchRoll(
                            rotation.YYaw, rotation.XPitch, rotation.ZRoll);
                        var matrix = rotationMatrix * Matrix4x4.CreateTranslation(position);
                        sphere.Mesh.Transform(matrix);
                        result.Add(sphere);
                    }

            return result;
        }
    }
}
