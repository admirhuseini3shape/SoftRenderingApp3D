using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Globalization;
using System.Threading;


namespace SoftRenderingApp3D {
    public class STLReader : FileReader {
        public IEnumerable<BasicModel> ReadFile(string fileName) {
            this.path = fileName;
            return STLImport();
        }

        public string path; // file path

        private enum FileType { NONE, BINARY, ASCII }; // stl file type enumeration

        /**
        * @brief  Class instance constructor
        * @param  none
        * @retval none
        */
        public STLReader(string filePath = "") {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
            path = filePath;
        }

        /**
        * @brief  *.stl file main read function
        * @param  none
        * @retval Volume
        */
        public IEnumerable<BasicModel> STLImport() {

            FileType stlFileType = GetFileType(path);

            if(stlFileType == FileType.ASCII) {
                yield return new BasicModel(STLASCIIReader.ReadFile(path));
            }
            else if(stlFileType == FileType.BINARY) {
                yield return new BasicModel(STLBinaryReader.ReadFile(path));
            }
            else {
                throw new FileLoadException($"Cannot load file format of {path}");
            }
        }

        /**
        * @brief  This function checks the type of stl file binary or ascii, function is assuming
        *         given file as proper *.stl file 
        * @param  none
        * @retval stlFileType
        */
        private FileType GetFileType(string filePath) {
            FileType stlFileType = FileType.NONE;

            /* check path is exist */
            if(File.Exists(filePath)) {
                var fileData = File.ReadAllLines(filePath);
                var lineCount = fileData.Count(); // number of lines in the file

                string firstLine = fileData[0];

                string endLines = fileData[lineCount - 1].ToString() +
                                  fileData[lineCount - 2].ToString();

                /* check the file is ascii or not */
                if((firstLine.IndexOf("solid") != -1) &
                    (endLines.IndexOf("endsolid") != -1)) {
                    stlFileType = FileType.ASCII;
                    // Save the data in the class
                }
                else {
                    stlFileType = FileType.BINARY;
                    // Save the data in the class
                }

            }
            else {
                stlFileType = FileType.NONE;
            }


            return stlFileType;
        }
    }
}
