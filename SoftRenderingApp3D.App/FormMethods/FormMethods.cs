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
            { Constants.GeneratingFunctions.CreateTown, ShapeGenerator.CreateTown}
        };

        public FormMethods() {

        }

        public List<DisplayModelJsonData> DeserializeAllModelConfigs(string directoryPath) {
            var deserializer = new JsonDeserializer();
            var models = new List<DisplayModelJsonData>();
            var files = Directory.GetFiles(directoryPath, "*.json");

            foreach(var jsonFile in files) {
                var model = deserializer.DeserializeObjectFromFile(jsonFile);
                models.Add(model);
                Console.WriteLine(model.Id);
            }

            return models;
        }

        public World prepareWorld(string id, ArcBallCam arcBallCam, CheckBox chkShowTexture, Panel3D panel3D1) {

            var basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;
            var jsonDirectory = Path.Combine(basePath, "JsonData");
            if(!Directory.Exists(jsonDirectory)) {
                throw new DirectoryNotFoundException($"Could not find the directory {jsonDirectory} that contains the json files!");
            }
            var models = DeserializeAllModelConfigs(jsonDirectory);

            var world = new World();

            var colladaReader = new ColladaReader();
            var stlReader = new STLReader();
            ITextureReader textureReader = new TextureReaderBMP();
            world.Textures.Add(textureReader.ReadImage(@"textures\bone.bmp"));
            world.Textures.Add(textureReader.ReadImage(@"textures\glass_effect.bmp"));
            world.Textures.Add(textureReader.ReadImage(@"textures\bone_high.bmp"));

            foreach(var model in models) {

                switch(model.Id) {
                    case "teapot":
                        panel3D1.RendererSettings.ShowTextures = model.ShowTexture;
                        chkShowTexture.Enabled = model.ShowTexture;
                        chkShowTexture.Checked = model.ShowTexture;
                        world.Volumes.AddRange(_readers[model.ReaderType].ReadFile(model.InputFileName));
                        break;
                        // case "jaw":
                        //     panel3D1.RendererSettings.ShowTextures = false;
                        //     chkShowTexture.Enabled = false;
                        //     chkShowTexture.Checked = false;
                        //     world.Volumes.AddRange(stlReader.ReadFile(@"models\original.stl"));
                        //     world.Volumes.AddRange(stlReader.ReadFile(@"models\offset_2.stl"));
                        //     arcBallCam.Position += new Vector3(0, 0, -5 - arcBallCam.Position.Z);
                        //     break;
                        // case "stl-mesh-1":
                        //     panel3D1.RendererSettings.ShowTextures = false;
                        //     chkShowTexture.Enabled = false;
                        //     chkShowTexture.Checked = false;
                        //     world.Volumes.AddRange(stlReader.ReadFile(@"models\Planetary_Toy_D80.stl"));
                        //     arcBallCam.Position += new Vector3(0, 0, -5 - arcBallCam.Position.Z);
                        //     break;
                        // case "stl-mesh-2":
                        //     panel3D1.RendererSettings.ShowTextures = false;
                        //     chkShowTexture.Enabled = false;
                        //     chkShowTexture.Checked = false;
                        //     world.Volumes.AddRange(stlReader.ReadFile(@"models\Star_Destroyer_Fixed.stl"));
                        //     arcBallCam.Position += new Vector3(0, 0, -5 - arcBallCam.Position.Z);
                        //     break;
                        // case "skull":
                        //     panel3D1.RendererSettings.ShowTextures = true;
                        //     chkShowTexture.Enabled = true;
                        //     chkShowTexture.Checked = true;
                        //     world.Volumes.AddRange(colladaReader.ReadFile(@"models\skull.dae"));
                        //     arcBallCam.Position += new Vector3(0, 0, -5 - arcBallCam.Position.Z);
                        //     break;
                        // case "empty":
                        //     panel3D1.RendererSettings.ShowTextures = false;
                        //     chkShowTexture.Enabled = false;
                        //     chkShowTexture.Checked = false;
                        //     break;
                        //
                        // case "town": {
                        //         panel3D1.RendererSettings.ShowTextures = false;
                        //         chkShowTexture.Enabled = false;
                        //         chkShowTexture.Checked = false;
                        //         ShapeGenerator.CreateTown(world);
                        //         break;
                        //     }
                        //
                        // case "littletown": {
                        //         panel3D1.RendererSettings.ShowTextures = false;
                        //         chkShowTexture.Enabled = false;
                        //         chkShowTexture.Checked = false;
                        //         var d = 10;
                        //         var s = 2;
                        //         for(var x = -d; x <= d; x += s)
                        //             for(var z = -d; z <= d; z += s) {
                        //                 world.Volumes.Add(
                        //                     new Cube {
                        //                         Position = new Vector3(x, 0, z)
                        //                         // Scale = new Vector3(1, r.Next(1, 50), 1)
                        //                     });
                        //             }
                        //
                        //         break;
                        //     }
                        //
                        // case "bigtown": {
                        //         panel3D1.RendererSettings.ShowTextures = false;
                        //         chkShowTexture.Enabled = false;
                        //         chkShowTexture.Checked = false;
                        //         var d = 200;
                        //         var s = 2;
                        //         for(var x = -d; x <= d; x += s)
                        //             for(var z = -d; z <= d; z += s) {
                        //                 world.Volumes.Add(
                        //                     new Cube {
                        //                         Position = new Vector3(x, 0, z)
                        //                         // Scale = new Vector3(1, r.Next(1, 50), 1)
                        //                     });
                        //             }
                        //
                        //         break;
                        //     }
                        //
                        // case "cube":
                        //     panel3D1.RendererSettings.ShowTextures = false;
                        //     chkShowTexture.Enabled = false;
                        //     chkShowTexture.Checked = false;
                        //     world.Volumes.Add(new Cube());
                        //     break;
                        //
                        // case "bigcube":
                        //     panel3D1.RendererSettings.ShowTextures = false;
                        //     chkShowTexture.Enabled = false;
                        //     chkShowTexture.Checked = false;
                        //     world.Volumes.Add(new Cube { Scale = new Vector3(100, 100, 100) });
                        //     break;
                        //
                        // case "spheres": {
                        //         panel3D1.RendererSettings.ShowTextures = false;
                        //         chkShowTexture.Enabled = false;
                        //         chkShowTexture.Checked = false;
                        //         var d = 5;
                        //         var s = 2;
                        //         var r = new Random();
                        //         for(var x = -d; x <= d; x += s)
                        //             for(var y = -d; y <= d; y += s)
                        //                 for(var z = -d; z <= d; z += s) {
                        //                     world.Volumes.Add(
                        //                         new IcoSphere(2) {
                        //                             Position = new Vector3(x, y, z),
                        //                             Rotation = new Rotation3D(
                        //                                 r.Next(-90, 90),
                        //                                 r.Next(-90, 90),
                        //                                 r.Next(-90, 90)).ToRad()
                        //                         });
                        //                 }
                        //
                        //         break;
                        //     }
                        //
                        // case "cubes": {
                        //         panel3D1.RendererSettings.ShowTextures = false;
                        //         chkShowTexture.Enabled = false;
                        //         chkShowTexture.Checked = false;
                        //         var d = 20;
                        //         var s = 2;
                        //         var r = new Random();
                        //         for(var x = -d; x <= d; x += s)
                        //             for(var y = -d; y <= d; y += s)
                        //                 for(var z = -d; z <= d; z += s) {
                        //                     world.Volumes.Add(
                        //                         new Cube {
                        //                             Position = new Vector3(x, y, z),
                        //                             Rotation = new Rotation3D(
                        //                                 r.Next(-90, 90),
                        //                                 r.Next(-90, 90),
                        //                                 r.Next(-90, 90)).ToRad()
                        //                         });
                        //                 }
                        //
                        //         break;
                        //     }
                        // }
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