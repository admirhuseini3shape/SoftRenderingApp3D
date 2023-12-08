using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Globalization;
using System.Threading;
using System.Numerics;


namespace SoftRenderingApp3D {
    public class STLReader : FileReader {
        public override IEnumerable<Volume> ReadFile(string fileName) {
            this.path = fileName;
            return NewSTLImport();
        }

        public string path; // file path

        private enum FileType { NONE, BINARY, ASCII }; // stl file type enumeration
        private bool processError;

        /**
        * @brief  Class instance constructor
        * @param  none
        * @retval none
        */
        public STLReader(string filePath = "") {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
            path = filePath;
            processError = false;
        }


        /**
        * @brief  This function returns the process error value if its true there is an error on process
        * @param  none
        * @retval none
        */
        public bool Get_Process_Error() {
            return processError;
        }


        /**
        * @brief  *.stl file main read function
        * @param  none
        * @retval Volume
        */
        public IEnumerable<Volume> NewSTLImport() {

            FileType stlFileType = GetFileType(path);

            if(stlFileType == FileType.ASCII) {
                yield return ReadASCIIFile(path);
            }
            else if(stlFileType == FileType.BINARY) {
                yield return ReadBinaryFile(path);
            }
            else {
                throw new FileLoadException($"Cannot load file format of {path}");
            }
        }
        
        private FileType GetFileType(string filePath) 
        {
            // check if file exists
            if (!File.Exists(filePath)) 
            {
                return FileType.NONE;
            }
		
            // read only the first few bytes from the file
            byte[] data = new byte[5]; // length of 'solid' which is 5
            try 
            {
                using (FileStream fs = File.Open(filePath, FileMode.Open))
                {
                    fs.Read(data, 0, data.Length);
                }
            }
            catch (Exception)
            {
                processError = true;
                throw new FileLoadException($"Error reading file: {filePath}!");
            }

            // convert bytes to string
            string dataAsString = Encoding.ASCII.GetString(data);
        
            if (dataAsString.ToLower().StartsWith("solid")) 
            {
                // if file starts with 'solid' it's potentially an ASCII STL file
                return FileType.ASCII;
            } 

            // if not ASCII, then it's potentially a binary STL file
            return FileType.BINARY;    
        }


        /**
        * @brief  *.stl file binary read function
        * @param  filePath
        * @retval meshList
        */ 
        
        private Volume ReadBinaryFile(string filePath) {
            
            List<Vector3> vertices = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<Triangle> triangleIndices = new List<Triangle>();
          
            byte[] fileBytes = File.ReadAllBytes(filePath);
            byte[] temp = new byte[4];
          
            if(fileBytes.Length > 120) 
            {
                System.Buffer.BlockCopy(fileBytes, 80, temp, 0, 4);
                var numOfMesh = System.BitConverter.ToInt32(temp, 0);
                var byteIndex = 84;
                var index = 0;

                for(int i = 0; i < numOfMesh; i++) 
                {
                    try 
                    {
                        normals.Add(GetVector3(fileBytes, ref byteIndex));
                        vertices.Add(GetVector3(fileBytes, ref byteIndex));
                        vertices.Add(GetVector3(fileBytes, ref byteIndex));
                        vertices.Add(GetVector3(fileBytes, ref byteIndex));

                        normals.Add(normals[index]);
                        normals.Add(normals[index]);

                        triangleIndices.Add(new Triangle(index, index + 1, index + 2));
          
                        byteIndex += 2; // Attribute byte count
                        index += 3;
                    }
                    catch 
                    {
                        processError = true;
                        break;
                    }
                }
            }

            if(processError)
                throw new FileLoadException($"Error reading file: {path}!");

            return new Volume(vertices.ToArray(),
                              triangleIndices.ToArray(),
                              normals.ToArray(),
                              null,
                              null);
        }

    private Vector3 GetVector3(byte[] fileBytes, ref int startIndex)
    {
        byte[] buffer = new byte[4];

        System.Buffer.BlockCopy(fileBytes, startIndex, buffer, 0, buffer.Length);
        float x = BitConverter.ToSingle(buffer, 0);
        startIndex += buffer.Length;

        System.Buffer.BlockCopy(fileBytes, startIndex, buffer, 0, buffer.Length);
        float y = BitConverter.ToSingle(buffer, 0);
        startIndex += buffer.Length;

        System.Buffer.BlockCopy(fileBytes, startIndex, buffer, 0, buffer.Length);
        float z = BitConverter.ToSingle(buffer, 0);
        startIndex += buffer.Length;

        return new Vector3(x, y, z);
    }
    
        /**
        * @brief  *.stl file ascii read function
        * @param  filePath
        * @retval meshList
        */
        private Volume ReadASCIIFile(string filePath) {
            
            List<Vector3> vertices = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<Triangle> triangleIndices = new List<Triangle>();


            using(StreamReader txtReader = new StreamReader(filePath)) {
                
                string lineString;

            while(!txtReader.EndOfStream) {
                lineString = txtReader.ReadLine().Trim(); /* delete whitespace in front and tail of the string */
                string[] lineData = lineString.Split(' ');

                var index = 0;

                if(lineData[0] == "solid") {
                    while(lineData[0] != "endsolid") {
                        lineString = txtReader.ReadLine().Trim(); // facetnormal
                        lineData = lineString.Split(' ');

                        if(lineData[0] == "endsolid") // check if we reach at the end of file
                        {
                            break;
                        }

                        /* this try-catch block will be reviewed */
                        try {
                            // FaceNormal 

                            Vector3 normal = new Vector3(float.Parse(lineData[2]), float.Parse(lineData[3]), float.Parse(lineData[4]));

                            normals.Add(normal);

                            /* normals of vertex 2 and 3 equals to vertex 1's normals */
                            normals.Add(normal);
                            normals.Add(normal);

                            //----------------------------------------------------------------------
                            lineString = txtReader.ReadLine(); // Just skip the OuterLoop line
                            //----------------------------------------------------------------------

                            // Vertex1
                            lineString = txtReader.ReadLine().Trim();
                            /* reduce spaces until string has proper format for split */
                            while(lineString.IndexOf("  ") != -1) lineString = lineString.Replace("  ", " ");
                            lineData = lineString.Split(' ');

                            vertices.Add(new Vector3(float.Parse(lineData[1]), float.Parse(lineData[2]), float.Parse(lineData[3]))); // x1

                            // Vertex2
                            lineString = txtReader.ReadLine().Trim();
                            /* reduce spaces until string has proper format for split */
                            while(lineString.IndexOf("  ") != -1) lineString = lineString.Replace("  ", " ");
                            lineData = lineString.Split(' ');

                            vertices.Add(new Vector3(float.Parse(lineData[1]), float.Parse(lineData[2]), float.Parse(lineData[3])));

                            // Vertex3
                            lineString = txtReader.ReadLine().Trim();
                            /* reduce spaces until string has proper format for split */
                            while(lineString.IndexOf("  ") != -1) lineString = lineString.Replace("  ", " ");
                            lineData = lineString.Split(' ');

                            vertices.Add(new Vector3(float.Parse(lineData[1]), float.Parse(lineData[2]), float.Parse(lineData[3])));

                            triangleIndices.Add(new Triangle(index, index + 1, index + 2));

                            index += 3;
                        }
                        catch {
                            processError = true;
                            break;
                        }

                        //----------------------------------------------------------------------
                        lineString = txtReader.ReadLine(); // Just skip the endloop
                        //----------------------------------------------------------------------
                        lineString = txtReader.ReadLine(); // Just skip the endfacet


                    } // while linedata[0]
                } // if solid
            } // while !endofstream
                
            }

            

            return new Volume(vertices.ToArray(),
                              triangleIndices.ToArray(),
                              normals.ToArray(),
                              null,
                              null);
        }
    }
}