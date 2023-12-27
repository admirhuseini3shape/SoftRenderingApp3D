using System.Collections.Generic;

namespace SoftRenderingApp3D.App.DataStructures {
    public class DisplayModelJsonData {
        public string Id { get; set; }
        public bool HasTexture { get; set; }
        public bool ShowTexture { get; set; }
        public string InputFileName { get; set; }
        public float InitialZoomLevel { get; set; }
        public Enums.TextureType Texture { get; set; }
        public Enums.DisplayModelType DisplayModelType { get; set; }
        public string GeneratingFunctionName { get; set; }
        
        

        public DisplayModelJsonData(string id, bool hasTexture, bool showTexture, string inputFileName,
                float initialZoomLevel, Enums.TextureType texture, Enums.DisplayModelType displayModelType,
                string generatingFunctionName) 
        {
            Id = id;
            HasTexture = hasTexture;
            ShowTexture = showTexture;
            InputFileName = inputFileName;
            InitialZoomLevel = initialZoomLevel;
            Texture = texture;  
            DisplayModelType = displayModelType;
            GeneratingFunctionName = generatingFunctionName;
        }
    }
}
