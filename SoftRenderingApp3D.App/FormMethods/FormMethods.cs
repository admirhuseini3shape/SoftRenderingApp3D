using SoftRenderingApp3D.App.DataStructures;
using SoftRenderingApp3D.App.Utils;
using SoftRenderingApp3D.Camera;
using SoftRenderingApp3D.DataStructures.FileReaders;
using SoftRenderingApp3D.DataStructures.TextureReaders;
using SoftRenderingApp3D.DataStructures.World;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Reflection;
using System.Windows.Forms;

namespace SoftRenderingApp3D.App.FormMethods {
    public class FormMethods {

        private readonly Dictionary<string, FileReader> _readers = new Dictionary<string, FileReader>
        {
            { "colladaReader", new ColladaReader() },
            { "stlReader", new STLReader() }
        };

        private readonly Dictionary<string, Action<World>> _generatingMethods = new Dictionary<string, Action<World>>
        {
            { Constants.GeneratingFunctions.CreateTown, ShapeGenerator.CreateTown},
            { Constants.GeneratingFunctions.CreateLittleTown, ShapeGenerator.CreateLittleTown},
            { Constants.GeneratingFunctions.CreateCubes, ShapeGenerator.CreateCubes},
            { Constants.GeneratingFunctions.CreateSphere, ShapeGenerator.CreateSphere},
            { Constants.GeneratingFunctions.CreateCube, ShapeGenerator.CreateCube},
            { Constants.GeneratingFunctions.CreateBigCube, ShapeGenerator.CreateBigCube},
            { Constants.GeneratingFunctions.CreateBigTown, ShapeGenerator.CreateBigTown}
        };
        
        public World prepareWorld(string id, ArcBallCam arcBallCam, CheckBox chkShowTexture, Panel3D panel3D1) {

            var basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;
            var jsonDirectory = Path.Combine(basePath, "JsonData");
            if(!Directory.Exists(jsonDirectory)) {
                throw new DirectoryNotFoundException($"Could not find the directory {jsonDirectory} that contains the json files!");
            }
            
            var deserializer = new DeserializeAllModelConfigs();
            
            var models = deserializer.DeserializeAllFiles(jsonDirectory);

            var world = new World();

            ITextureReader textureReader = new TextureReaderBMP();
            world.Textures.Add(textureReader.ReadImage(@"textures\bone.bmp"));
            world.Textures.Add(textureReader.ReadImage(@"textures\glass_effect.bmp"));
            world.Textures.Add(textureReader.ReadImage(@"textures\bone_high.bmp"));

            foreach(var model in models) {

                switch(id) {
                        case "skull":
                              LoadModel(model, panel3D1, chkShowTexture, world);
                              arcBallCam.Position += new Vector3(0, 0, -5 - arcBallCam.Position.Z);
                              break;
                        case "teapot":
                              LoadModel(model, panel3D1, chkShowTexture, world);
                              break;
                        case "town": 
                              panel3D1.RendererSettings.ShowTextures = false;
                              chkShowTexture.Enabled = false;
                              chkShowTexture.Checked = false;
                              ShapeGenerator.CreateTown(world);
                              break;
                        case "littletown": 
                              panel3D1.RendererSettings.ShowTextures = false;
                              chkShowTexture.Enabled = false;
                              chkShowTexture.Checked = false;
                              ShapeGenerator.CreateLittleTown(world);
                              break;
                        case "bigtown": 
                              panel3D1.RendererSettings.ShowTextures = false;
                              chkShowTexture.Enabled = false;
                              chkShowTexture.Checked = false;
                              ShapeGenerator.CreateBigTown(world);
                              break;
                        case "cube":
                              panel3D1.RendererSettings.ShowTextures = false;
                              chkShowTexture.Enabled = false;
                              chkShowTexture.Checked = false;
                              ShapeGenerator.CreateCube(world);
                              break;
                        case "bigcube":
                              panel3D1.RendererSettings.ShowTextures = false;
                              chkShowTexture.Enabled = false;
                              chkShowTexture.Checked = false;
                              ShapeGenerator.CreateBigCube(world);
                              break;
                        case "spheres": 
                              panel3D1.RendererSettings.ShowTextures = false;
                              chkShowTexture.Enabled = false;
                              chkShowTexture.Checked = false;
                              ShapeGenerator.CreateSphere(world);
                              break;
                        case "cubes": 
                              panel3D1.RendererSettings.ShowTextures = false;
                              chkShowTexture.Enabled = false;
                              chkShowTexture.Checked = false;
                              ShapeGenerator.CreateCubes(world);
                              break;                 
                        case "jaw":
                              LoadModel(model, panel3D1, chkShowTexture, world);
                              arcBallCam.Position += new Vector3(0, 0, -5 - arcBallCam.Position.Z);
                              break;
                        case "empty":
                              panel3D1.RendererSettings.ShowTextures = false;
                              chkShowTexture.Enabled = false;
                              chkShowTexture.Checked = false;
                              break;
                }
            }

            world.LightSources.Add(new LightSource { Position = new Vector3(0, 0, 10) });


            return world;
        }

        private void LoadModel(DisplayModelJsonData data, Panel3D panel3D1, CheckBox chkShowTexture, World world) {
            panel3D1.RendererSettings.ShowTextures = data.ShowTexture;
            chkShowTexture.Enabled = data.HasTexture;
            chkShowTexture.Checked = data.ShowTexture;
            if(!data.InputFileName.Equals(string.Empty)) {
                if(!_readers.TryGetValue(data.ReaderType, out var reader)) {
                    throw new Exception($"Could not find reader for {data.ReaderType}!");
                }

                var volumes = reader.ReadFile(data.InputFileName);
                world.Volumes.AddRange(volumes);
            }
            else if(!data.GeneratingFunctionName.Equals(string.Empty)) {
                if(!_generatingMethods.TryGetValue(data.GeneratingFunctionName, out var function)) {
                    throw new Exception($"Could not find generating function for {data.GeneratingFunctionName}!");
                }

                function(world);
            }
        }
    }
}