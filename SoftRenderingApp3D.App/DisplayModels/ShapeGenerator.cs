using SoftRenderingApp3D.DataStructures.Drawables;
using SoftRenderingApp3D.DataStructures.Materials;
using SoftRenderingApp3D.DataStructures.Meshes;
using SoftRenderingApp3D.DataStructures.Shapes;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace SoftRenderingApp3D.App.DisplayModels
{
    public static class ShapeGenerator
    {
        public static IDrawable CreateCube()
        {
            return Cube.GetDrawable();
        }

        public static IDrawable CreateBigCube()
        {
            var cube = Cube.GetDrawable();
            var scaling = new Vector3(100, 100, 100);
            var matrix = Matrix4x4.CreateScale(scaling);
            cube.Mesh.Transform(matrix);

            return cube;
        }

        public static IDrawable CreateTetrahedron()
        {
            return Tetrahedron.GetDrawable();
        }

        public static IDrawable CreateOctahedron()
        {
            return Octahedron.GetDrawable();
        }

        public static IDrawable CreateCubes()
        {
            const int d = 5;
            const int s = 2;
            const int maxAngle = 0;
            const int loopCount = 3;
            var r = new Random();

            var capacity = (int)Math.Pow((2.0 * d) / s, loopCount);
            var meshes = new List<IMesh>(capacity);
            var materials = new List<IFacetColorMaterial>(capacity);

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
                meshes.Add(cube.Mesh);
                materials.Add((IFacetColorMaterial)cube.Material);
            }

            var resultMesh = new Mesh();
            var resultMaterial = new FacetColorMaterial();
            var (vertexMappings, facetMappings) = resultMesh.Append(meshes);
            resultMaterial.Append(materials, vertexMappings, facetMappings);
            return new Drawable(resultMesh, resultMaterial);
        }

        public static IDrawable CreateTown()
        {
            const int d = 50;
            const int s = 2;
            const int loopCount = 2;
            var translateToOriginY = new Vector3(0, .5f, 0);
            var random = new Random();

            var capacity = (int)Math.Pow((2.0 * d) / s, loopCount);
            var meshes = new List<IMesh>(capacity);
            var materials = new List<IFacetColorMaterial>(capacity);

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
                meshes.Add(cube.Mesh);
                materials.Add((IFacetColorMaterial)cube.Material);
            }

            var resultMesh = new Mesh();
            var resultMaterial = new FacetColorMaterial();
            var (vertexMappings, facetMappings) = resultMesh.Append(meshes);
            resultMaterial.Append(materials, vertexMappings, facetMappings);
            return new Drawable(resultMesh, resultMaterial);
        }

        public static IDrawable CreateLittleTown()
        {
            const int d = 10;
            const int s = 2;
            const int loopCount = 2;

            var translateToOriginY = new Vector3(0, .5f, 0);
            var random = new Random();

            var capacity = (int)Math.Pow((2.0 * d) / s, loopCount);
            var meshes = new List<IMesh>(capacity);
            var materials = new List<IFacetColorMaterial>(capacity);

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
                meshes.Add(cube.Mesh);
                materials.Add((IFacetColorMaterial)cube.Material);
            }

            var resultMesh = new Mesh();
            var resultMaterial = new FacetColorMaterial();
            var (vertexMappings, facetMappings) = resultMesh.Append(meshes);
            resultMaterial.Append(materials, vertexMappings, facetMappings);
            return new Drawable(resultMesh, resultMaterial);
        }

        public static IDrawable CreateBigTown()
        {
            const int d = 200;
            const int s = 2;
            const int loopCount = 2;

            var translateToOriginY = new Vector3(0, .5f, 0);
            var random = new Random();

            var capacity = (int)Math.Pow((2.0 * d) / s, loopCount);
            var meshes = new List<IMesh>(capacity);
            var materials = new List<IFacetColorMaterial>(capacity);

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
                meshes.Add(cube.Mesh);
                materials.Add((IFacetColorMaterial)cube.Material);
            }

            var resultMesh = new Mesh();
            var resultMaterial = new FacetColorMaterial();
            var (vertexMappings, facetMappings) = resultMesh.Append(meshes);
            resultMaterial.Append(materials, vertexMappings, facetMappings);
            return new Drawable(resultMesh, resultMaterial);
        }

        public static IDrawable CreateSpheres()
        {
            const int d = 5;
            const int s = 2;
            const int loopCount = 2;

            var r = new Random();
            var capacity = (int)Math.Pow((2.0 * d) / s, loopCount);
            var meshes = new List<IMesh>(capacity);
            var materials = new List<IMaterial>(capacity);

            for(var x = -d; x <= d; x += s)
            for(var y = -d; y <= d; y += s)
            for(var z = -d; z <= d; z += s)
            {
                var sphere = Sphere.GetDrawable(0.7f, 2);
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
                meshes.Add(sphere.Mesh);
                materials.Add(sphere.Material);
                
                Console.WriteLine($"Added shape at position ({x}, {y}, {z})");
            }

            var resultMesh = new Mesh();
            var resultMaterial = new MaterialBase();
            var (vertexMappings, facetMappings) = resultMesh.Append(meshes);
            resultMaterial.Append(materials, vertexMappings, facetMappings);
            return new Drawable(resultMesh, resultMaterial);
        }

        public static IDrawable CreateTetrahedralOctahedralHoneycomb()
        {
            const float radius = 1.0f;
            float spacing = radius;

            var meshes = new List<IMesh>();
            var materials = new List<IMaterial>();

            // Add 6 octahedra
            Vector3[] octaPositions = {
                new Vector3( 1,  0,  0),
                new Vector3(-1,  0,  0),
                new Vector3( 0,  1,  0),
                new Vector3( 0, -1,  0),
                new Vector3( 0,  0,  1),
                new Vector3( 0,  0, -1)
            };

            foreach (var pos in octaPositions)
            {
                AddShape(meshes, materials, pos * spacing, Octahedron.GetDrawable(), radius);
            }

            // Add 8 tetrahedra
            Vector3[] tetraPositions = {
                new Vector3( 0.5f,  0.5f,  0.5f),
                new Vector3(-0.5f,  0.5f,  0.5f),
                new Vector3( 0.5f, -0.5f,  0.5f),
                new Vector3(-0.5f, -0.5f,  0.5f),
                new Vector3( 0.5f,  0.5f, -0.5f),
                new Vector3(-0.5f,  0.5f, -0.5f),
                new Vector3( 0.5f, -0.5f, -0.5f),
                new Vector3(-0.5f, -0.5f, -0.5f)
            };

            foreach (var pos in tetraPositions)
            {
                AddTetrahedron(meshes, materials, pos, radius / 2, NeedsAdjustment(pos));
            }

            var resultMesh = new Mesh();
            var resultMaterial = new FacetColorMaterial();
            var (vertexMappings, facetMappings) = resultMesh.Append(meshes);
            resultMaterial.Append(materials, vertexMappings, facetMappings);
            return new Drawable(resultMesh, resultMaterial);
        }
        
        private static void AddShape(List<IMesh> meshes, List<IMaterial> materials, Vector3 position, IDrawable shape, float radius)
        {
            var mesh = shape.Mesh;
            shape.Mesh.Transform(Matrix4x4.CreateScale(radius));
            shape.Mesh.Transform(Matrix4x4.CreateTranslation(position));
            
            meshes.Add(shape.Mesh);
            materials.Add(shape.Material);
        }

        private static void AddTetrahedron(List<IMesh> meshes, List<IMaterial> materials, Vector3 position,
            float radius, bool needsAdjustment)
        {
            var tetrahedron = Tetrahedron.GetDrawable();
            var mesh = tetrahedron.Mesh;

            // Scale the tetrahedron
            mesh.Transform(Matrix4x4.CreateScale(radius));

            if (needsAdjustment)
            {
                // Apply a 180-degree rotation around the X-axis for misaligned tetrahedra
                mesh.Transform(Matrix4x4.CreateRotationX((float)Math.PI / 2));
                mesh.Transform(Matrix4x4.CreateTranslation(position + new Vector3(0.25f, -0.25f, 0.25f)));
            }
            else
            {
                mesh.Transform(Matrix4x4.CreateTranslation(position + new Vector3(0.25f, 0.25f, 0.25f))); 
            }
           
            
            meshes.Add(mesh);
            materials.Add(tetrahedron.Material);
        }
        
        private static bool NeedsAdjustment(Vector3 position)
        {
            // Count the number of negative coordinates
            int negativeCount = (position.X < 0 ? 1 : 0) + 
                                (position.Y < 0 ? 1 : 0) + 
                                (position.Z < 0 ? 1 : 0);
    
            // Return true if the number of negative coordinates is odd
            return negativeCount % 2 == 0;
        }
        
        public static IDrawable CreateRecursiveHoneycomb()
        {
            int depth = 2;
            float spacing = 2.0f;
            float scale = 2.0f;
            var r = new Random();
            var meshes = new List<IMesh>();
            var materials = new List<IMaterial>();

            RecursiveHoneycomb(depth, spacing, scale, Vector3.Zero, meshes, materials, r);

            var resultMesh = new Mesh();
            var resultMaterial = new FacetColorMaterial();
            var (vertexMappings, facetMappings) = resultMesh.Append(meshes);
            resultMaterial.Append(materials, vertexMappings, facetMappings);
            return new Drawable(resultMesh, resultMaterial);
        }

        private static void RecursiveHoneycomb(int depth, float spacing, float scale, Vector3 position, List<IMesh> meshes, List<IMaterial> materials, Random r)
        {
            if (depth <= 0) return;

            // Create a honeycomb at the current position
            var honeycomb = CreateTetrahedralOctahedralHoneycomb();
            ApplyTransformation(honeycomb, position, r);
            meshes.Add(honeycomb.Mesh);
            materials.Add(honeycomb.Material);

            Console.WriteLine($"Added honeycomb at position ({position.X}, {position.Y}, {position.Z})");

            // Recursive calls for surrounding positions
            for (int x = -depth; x <= depth; x++)
            {
                for (int y = -depth; y <= depth; y++)
                {
                    for (int z = -depth; z <= depth; z++)
                    {
                        if (Math.Abs(x) + Math.Abs(y) + Math.Abs(z) == depth)
                        {
                            Vector3 newPosition = position + new Vector3(x, y, z) * spacing;
                            RecursiveHoneycomb(depth - 1, spacing, scale * 0.9f, newPosition, meshes, materials, r);
                        }
                    }
                }
            }
        }
        
        private static void ApplyTransformation(IDrawable drawable, Vector3 position, Random r)
        {
            var rotation = new Rotation3D(
                    r.Next(-15, 15),
                    r.Next(-15, 15),
                    r.Next(-15, 15))
                .ToRad();
            var rotationMatrix = Matrix4x4.CreateFromYawPitchRoll(
                rotation.YYaw, rotation.XPitch, rotation.ZRoll);
            var matrix = Matrix4x4.CreateTranslation(position);
            drawable.Mesh.Transform(matrix);
        }
        
    }
}


            // mesh.Transform(Matrix4x4.CreateScale(scaling));
