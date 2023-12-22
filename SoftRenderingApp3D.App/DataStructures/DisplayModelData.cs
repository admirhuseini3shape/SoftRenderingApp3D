using System.Collections.Generic;

namespace SoftRenderingApp3D.App.DataStructures {
    public class DisplayModelData {
        public string Display { get; }
        public string Id { get; }

        public DisplayModelData(string display, string id) {
            Display = display;
            Id = id;
        }

        public static readonly List<DisplayModelData> Models = new List<DisplayModelData>() {
            new DisplayModelData("Crane",             "skull"     ),
            new DisplayModelData("Teapot",            "teapot"    ),
            new DisplayModelData("Cubes",             "cubes"     ),
            new DisplayModelData("Spheres",           "spheres"   ),
            new DisplayModelData("Little town",       "littletown"),
            new DisplayModelData("Town",              "town"      ),
            new DisplayModelData("Big town",          "bigtown"   ),
            new DisplayModelData("Cube",              "cube"      ),
            new DisplayModelData("Big cube",          "bigcube"   ),
            new DisplayModelData("Empty",             "empty"     ),
            new DisplayModelData("Planetary Toy STL", "stl-mesh-1"),
            new DisplayModelData("Star Destroyer STL","stl-mesh-2"),
            new DisplayModelData("Jaw",               "jaw"       )
        };
    }
}
