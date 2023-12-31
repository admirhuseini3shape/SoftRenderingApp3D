using SoftRenderingApp3D.App.DataStructures;
using SoftRenderingApp3D.DataStructures.FileReaders;
using SoftRenderingApp3D.DataStructures.TextureReaders;
using SoftRenderingApp3D.DataStructures.World;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace SoftRenderingApp3D.App.Utils {
    public static class DisplayModelHelpers {

        private static readonly Dictionary<string, FileReader> _readers =
            new Dictionary<string, FileReader>
        {
            { Constants.Readers.ColladaReader, new ColladaReader() },
            { Constants.Readers.StlReader, new STLReader() }
        };

        private static readonly Dictionary<string, Action<World>> _generatingMethods = 
            new Dictionary<string, Action<World>>
        {
            { Constants.GeneratingFunctions.CreateTown, ShapeGenerator.CreateTown},
            { Constants.GeneratingFunctions.CreateLittleTown, ShapeGenerator.CreateLittleTown},
            { Constants.GeneratingFunctions.CreateCubes, ShapeGenerator.CreateCubes},
            { Constants.GeneratingFunctions.CreateSphere, ShapeGenerator.CreateSphere},
            { Constants.GeneratingFunctions.CreateCube, ShapeGenerator.CreateCube},
            { Constants.GeneratingFunctions.CreateBigCube, ShapeGenerator.CreateBigCube},
            { Constants.GeneratingFunctions.CreateBigTown, ShapeGenerator.CreateBigTown}
        };

        public static World GenerateWorld(DisplayModelData data) {
            var world = new World();

            ITextureReader textureReader = new TextureReaderBMP();
            if(data.HasTexture) {
                world.Textures.Add(textureReader.ReadImage(@"textures\bone.bmp"));
                world.Textures.Add(textureReader.ReadImage(@"textures\glass_effect.bmp"));
                world.Textures.Add(textureReader.ReadImage(@"textures\bone_high.bmp"));
            }
            else {
                world.Textures.Add(textureReader.ReadImage(@"textures\bone.bmp"));
            }

            LoadModel(data, world);

            world.LightSources.Add(new LightSource { Position = new Vector3(0, 0, 10) });

            return world;
        }

        private static void LoadModel(DisplayModelData data, World world) {
            var functionName = data.GeneratingFunctionName;
            if(data.InputFileName != null && data.ReaderType != null) {
                if(!_readers.TryGetValue(data.ReaderType, out var reader))
                    throw new Exception($"Could not find reader for {data.ReaderType}!");

                var volumes = reader.ReadFile(data.InputFileName);
                world.Meshes.AddRange(volumes);
            }
            else if(functionName != null) {
                if(!_generatingMethods.TryGetValue(functionName, out var method))
                    throw new Exception($"Could not find generating function for {functionName}!");

                method(world);
            }
        }
    }
}