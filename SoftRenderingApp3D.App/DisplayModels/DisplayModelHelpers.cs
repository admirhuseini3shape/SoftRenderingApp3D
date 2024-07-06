using SoftRenderingApp3D.App.DataStructures;
using SoftRenderingApp3D.App.Utils;
using SoftRenderingApp3D.DataStructures.Drawables;
using SoftRenderingApp3D.DataStructures.FileReaders;
using SoftRenderingApp3D.DataStructures.Materials;
using SoftRenderingApp3D.DataStructures.TextureReaders;
using System;
using System.Collections.Generic;

namespace SoftRenderingApp3D.App.DisplayModels
{
    public static class DisplayModelHelpers
    {
        private static readonly Dictionary<string, FileReader> Readers =
            new Dictionary<string, FileReader>
        {
            { Constants.Readers.ColladaReader, new ColladaReaderOld() },
            { Constants.Readers.StlReader, new STLReader() }
        };

        private static readonly Dictionary<string, Func<IDrawable>> GeneratingMethods =
            new Dictionary<string, Func<IDrawable>>
        {
            { Constants.GeneratingFunctions.CreateTown, ShapeGenerator.CreateTown},
            { Constants.GeneratingFunctions.CreateLittleTown, ShapeGenerator.CreateLittleTown},
            { Constants.GeneratingFunctions.CreateCubes, ShapeGenerator.CreateCubes},
            { Constants.GeneratingFunctions.CreateSphere, ShapeGenerator.CreateSpheres},
            { Constants.GeneratingFunctions.CreateCube, ShapeGenerator.CreateCube},
            { Constants.GeneratingFunctions.CreateBigTown, ShapeGenerator.CreateBigTown},
            { Constants.GeneratingFunctions.CreateBigCube, ShapeGenerator.CreateBigCube},
            { Constants.GeneratingFunctions.CreateTetrahedron, ShapeGenerator.CreateTetrahedron},
            { Constants.GeneratingFunctions.CreateTetrahedralOctahedralHoneycomb, ShapeGenerator.CreateTetrahedralOctahedralHoneycomb},
            { Constants.GeneratingFunctions.CreateOctahedron, ShapeGenerator.CreateOctahedron},
            { Constants.GeneratingFunctions.CreateRecursiveHoneycomb, ShapeGenerator.CreateRecursiveHoneycomb},
            { Constants.GeneratingFunctions.CreateTruncatedOctahedron, ShapeGenerator.CreateTruncatedOctahedron},
            { Constants.GeneratingFunctions.CreateRecursiveTruncated, ShapeGenerator.CreateRecursiveTruncated}
            
        };

        public static IDrawable GetDrawable(DisplayModelData data)
        {
            var functionName = data.GeneratingFunctionName;
            if(data.InputFileName != null && data.ReaderType != null)
            {
                if(!Readers.TryGetValue(data.ReaderType, out var reader))
                    throw new Exception($"Could not find reader for {data.ReaderType}!");

                var drawable = reader.ReadFile(data.InputFileName);
                if(!data.HasTexture)
                    return drawable;

                var textureReader = new TextureReaderBMP();
                var texture = textureReader.ReadImage(@"textures\bone.bmp");
                //var texture = textureReader.ReadImage(@"textures\glass_effect.bmp");
                //var texture = textureReader.ReadImage(@"textures\bone_high.bmp");
                drawable = new Drawable(drawable.Mesh, new TextureMaterial(texture));

                return drawable;
            }

            if(functionName == null)
                return new Drawable() ;

            if(!GeneratingMethods.TryGetValue(functionName, out var method))
                throw new Exception($"Could not find generating function for {functionName}!");

            return method();
        }
    }
}
