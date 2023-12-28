using SoftRenderingApp3D.App.DataStructures;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SoftRenderingApp3D.App.Utils  {
    public static class JsonHelpers
    {
        public static List<DisplayModelData> GetDisplayModelsFromJson() {
            var basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if(basePath == null)
                throw new FileNotFoundException("Cannot find location of executing assembly!");

            var jsonDirectory = Path.Combine(basePath, Constants.JsonDataFolder);
            if(!Directory.Exists(jsonDirectory))
                throw new DirectoryNotFoundException(
                    $"Could not find the directory {jsonDirectory} that contains the json files!");

            var fileName = Path.Combine(jsonDirectory, Constants.JsonFileName);
            if(!File.Exists(fileName))
                throw new FileNotFoundException(
                    $"Could not find the file {fileName} that contains the json data!");

            return DisplayModelsFromJsonFile(fileName);
        }

        public static List<DisplayModelData> DisplayModelsFromJsonFile(string filePath) {
            if(!File.Exists(filePath)) {
                throw new FileNotFoundException($"The file '{filePath}' was not found.");
            }
            var jsonString = File.ReadAllText(filePath);
            var options = new JsonSerializerOptions {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter() } // Converts Enum into String so json can be deserialized
            };
            return JsonSerializer.Deserialize<List<DisplayModelData>>(jsonString, options);
        }
    }
}
