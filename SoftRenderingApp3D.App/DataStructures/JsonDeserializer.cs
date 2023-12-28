using System.Text.Json;
using System.IO;
using System.Text.Json.Serialization;

namespace SoftRenderingApp3D.App.DataStructures  {
    public class JsonDeserializer
    {
        public DisplayModelJsonData DeserializeObjectFromFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"The file '{filePath}' was not found.");
            }
            var jsonString = File.ReadAllText(filePath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true, 
                Converters = { new JsonStringEnumConverter() } // Converts Enum into String so json can be deserialized
            };
            return JsonSerializer.Deserialize<DisplayModelJsonData>(jsonString, options);
        }
    }
}
