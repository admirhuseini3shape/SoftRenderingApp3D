using SoftRenderingApp3D.App.DataStructures;
using SoftRenderingApp3D.DataStructures.Drawables;
using SoftRenderingApp3D.DataStructures.FileReaders;
using SoftRenderingApp3D.DataStructures.Materials;
using SoftRenderingApp3D.DataStructures.TextureReaders;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftRenderingApp3D.App.Utils
{
    public static class DisplayModelHelpers
    {

        private static readonly Dictionary<string, FileReader> Readers =
            new Dictionary<string, FileReader>
        {
            { Constants.Readers.ColladaReader, new ColladaReaderOld() },
            { Constants.Readers.StlReader, new STLReader() }
        };

        private static readonly Dictionary<string, Func<List<IDrawable>>> GeneratingMethods =
            new Dictionary<string, Func<List<IDrawable>>>
        {
            { Constants.GeneratingFunctions.CreateTown, ShapeGenerator.CreateTown},
            { Constants.GeneratingFunctions.CreateLittleTown, ShapeGenerator.CreateLittleTown},
            { Constants.GeneratingFunctions.CreateCubes, ShapeGenerator.CreateCubes},
            { Constants.GeneratingFunctions.CreateSphere, ShapeGenerator.CreateSphere},
            { Constants.GeneratingFunctions.CreateCube, ShapeGenerator.CreateCube},
            { Constants.GeneratingFunctions.CreateBigCube, ShapeGenerator.CreateBigCube},
            { Constants.GeneratingFunctions.CreateBigTown, ShapeGenerator.CreateBigTown}
        };

        public static List<IDrawable> GetDrawables(DisplayModelData data)
        {
            var functionName = data.GeneratingFunctionName;
            if(data.InputFileName != null && data.ReaderType != null)
            {
                if(!Readers.TryGetValue(data.ReaderType, out var reader))
                    throw new Exception($"Could not find reader for {data.ReaderType}!");

                var drawables = reader.ReadFile(data.InputFileName).ToList();
                if(!data.HasTexture)
                    return drawables;

                var textureReader = new TextureReaderBMP();
                var texture = textureReader.ReadImage(@"textures\bone.bmp");
                //var texture = textureReader.ReadImage(@"textures\glass_effect.bmp");
                //var texture = textureReader.ReadImage(@"textures\bone_high.bmp");
                for(var i = 0; i < drawables.Count; i++)
                    drawables[i] = new Drawable(drawables[i].Mesh, new TextureMaterial(texture));

                return drawables;
            }

            if(functionName == null)
                return new List<IDrawable>();

            if(!GeneratingMethods.TryGetValue(functionName, out var method))
                throw new Exception($"Could not find generating function for {functionName}!");

            return method();

        }
    }
}
