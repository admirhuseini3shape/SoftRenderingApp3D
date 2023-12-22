using System.Collections.Generic;

namespace SoftRenderingApp3D.App.DataStructures {
    public class DisplayModelJsonData {
        private string Id { get; set; }
        bool HasTexture { get; set; }
        bool ShowTexture { get; set; }
        List<string> InputFileNames { get; set; }
        float InitialZoomLevel { get; set; }
        Enums.TextureType Texture { get; set; }
        Enums.DisplayModelType DisplayModelType { get; set; }
        string GeneratingFunctionName { get; set; }

        public DisplayModelJsonData(string id, bool hasTexture, bool showTexture, List<string> inputFileNames,
                float initialZoomLevel, Enums.TextureType texture, Enums.DisplayModelType displayModelType,
                string generatingFunctionName) 
        {
            Id = id;
            HasTexture = hasTexture;
            ShowTexture = showTexture;
            InputFileNames = inputFileNames;
            InitialZoomLevel = initialZoomLevel;
            Texture = texture;
            DisplayModelType = displayModelType;
            GeneratingFunctionName = generatingFunctionName;
        }
    }
}
