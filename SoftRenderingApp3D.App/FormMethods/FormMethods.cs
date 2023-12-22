using SoftRenderingApp3D.Camera;
using SoftRenderingApp3D.DataStructures.FileReaders;
using SoftRenderingApp3D.DataStructures.Shapes;
using SoftRenderingApp3D.DataStructures.TextureReaders;
using SoftRenderingApp3D.DataStructures.World;
using System;
using System.Numerics;
using System.Windows.Forms;

namespace SoftRenderingApp3D.App.FormMethods {
    public class FormMethods {
        public World prepareWorld(string id, ArcBallCam arcBallCam, CheckBox chkShowTexture, Panel3D panel3D1) {
            var world = new World();

            var colladaReader = new ColladaReader();
            var stlReader = new STLReader();
            ITextureReader textureReader = new TextureReaderBMP();
            world.Textures.Add(textureReader.ReadImage(@"textures\bone.bmp"));
            world.Textures.Add(textureReader.ReadImage(@"textures\glass_effect.bmp"));
            world.Textures.Add(textureReader.ReadImage(@"textures\bone_high.bmp"));

            switch(id) {
                case "jaw":
                    panel3D1.RendererSettings.ShowTextures = false;
                    chkShowTexture.Enabled = false;
                    chkShowTexture.Checked = false;
                    world.Volumes.AddRange(stlReader.ReadFile(@"models\original.stl"));
                    world.Volumes.AddRange(stlReader.ReadFile(@"models\offset_2.stl"));
                    arcBallCam.Position += new Vector3(0, 0, -5 - arcBallCam.Position.Z);
                    break;
                case "stl-mesh-1":
                    panel3D1.RendererSettings.ShowTextures = false;
                    chkShowTexture.Enabled = false;
                    chkShowTexture.Checked = false;
                    world.Volumes.AddRange(stlReader.ReadFile(@"models\Planetary_Toy_D80.stl"));
                    arcBallCam.Position += new Vector3(0, 0, -5 - arcBallCam.Position.Z);
                    break;
                case "stl-mesh-2":
                    panel3D1.RendererSettings.ShowTextures = false;
                    chkShowTexture.Enabled = false;
                    chkShowTexture.Checked = false;
                    world.Volumes.AddRange(stlReader.ReadFile(@"models\Star_Destroyer_Fixed.stl"));
                    arcBallCam.Position += new Vector3(0, 0, -5 - arcBallCam.Position.Z);
                    break;
                case "skull":
                    panel3D1.RendererSettings.ShowTextures = true;
                    chkShowTexture.Enabled = true;
                    chkShowTexture.Checked = true;
                    world.Volumes.AddRange(colladaReader.ReadFile(@"models\skull.dae"));
                    arcBallCam.Position += new Vector3(0, 0, -5 - arcBallCam.Position.Z);
                    break;

                case "teapot":
                    panel3D1.RendererSettings.ShowTextures = false;
                    chkShowTexture.Enabled = false;
                    chkShowTexture.Checked = false;
                    world.Volumes.AddRange(colladaReader.ReadFile(@"models\teapot.dae"));
                    break;

                case "empty":
                    panel3D1.RendererSettings.ShowTextures = false;
                    chkShowTexture.Enabled = false;
                    chkShowTexture.Checked = false;
                    break;

                case "town": {
                    panel3D1.RendererSettings.ShowTextures = false;
                    chkShowTexture.Enabled = false;
                    chkShowTexture.Checked = false;
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

                    break;
                }

                case "littletown": {
                    panel3D1.RendererSettings.ShowTextures = false;
                    chkShowTexture.Enabled = false;
                    chkShowTexture.Checked = false;
                    var d = 10;
                    var s = 2;
                    for(var x = -d; x <= d; x += s)
                    for(var z = -d; z <= d; z += s) {
                        world.Volumes.Add(
                            new Cube {
                                Position = new Vector3(x, 0, z)
                                // Scale = new Vector3(1, r.Next(1, 50), 1)
                            });
                    }

                    break;
                }

                case "bigtown": {
                    panel3D1.RendererSettings.ShowTextures = false;
                    chkShowTexture.Enabled = false;
                    chkShowTexture.Checked = false;
                    var d = 200;
                    var s = 2;
                    for(var x = -d; x <= d; x += s)
                    for(var z = -d; z <= d; z += s) {
                        world.Volumes.Add(
                            new Cube {
                                Position = new Vector3(x, 0, z)
                                // Scale = new Vector3(1, r.Next(1, 50), 1)
                            });
                    }

                    break;
                }

                case "cube":
                    panel3D1.RendererSettings.ShowTextures = false;
                    chkShowTexture.Enabled = false;
                    chkShowTexture.Checked = false;
                    world.Volumes.Add(new Cube());
                    break;

                case "bigcube":
                    panel3D1.RendererSettings.ShowTextures = false;
                    chkShowTexture.Enabled = false;
                    chkShowTexture.Checked = false;
                    world.Volumes.Add(new Cube { Scale = new Vector3(100, 100, 100) });
                    break;

                case "spheres": {
                    panel3D1.RendererSettings.ShowTextures = false;
                    chkShowTexture.Enabled = false;
                    chkShowTexture.Checked = false;
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

                    break;
                }

                case "cubes": {
                    panel3D1.RendererSettings.ShowTextures = false;
                    chkShowTexture.Enabled = false;
                    chkShowTexture.Checked = false;
                    var d = 20;
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

                    break;
                }
            }

            world.LightSources.Add(new LightSource { Position = new Vector3(0, 0, 10) });


            return world;
        }
    }
}