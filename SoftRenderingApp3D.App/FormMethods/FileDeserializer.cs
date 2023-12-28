using SoftRenderingApp3D.App.DataStructures;
using System;
using System.Collections.Generic;
using System.IO;

namespace SoftRenderingApp3D.App.FormMethods {
    public class DeserializeAllModelConfigs {
        public List<DisplayModelJsonData> DeserializeAllFiles(string directoryPath) {
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
    }
}