using SoftRenderingApp3D.App.DataStructures;
using System.Text.Json;
using System.IO;


namespace SoftRenderingApp3D.App.DataStructures  {
    public class ObjectSerializer
    {
        public void SerializeObjectToFile(DisplayModelData model, string filePath)
        {
            string jsonString = JsonSerializer.Serialize(model, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, jsonString);
        }
    
        public DisplayModelJsonData DeserializeObjectFromFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"The file '{filePath}' was not found.");
            }
            string jsonString = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<DisplayModelJsonData>(jsonString);
        }
    }
}
