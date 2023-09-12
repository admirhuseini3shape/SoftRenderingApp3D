using System;
using System.Collections.Generic;
using System.Numerics;
using System.IO;

namespace SoftRenderingApp3D {
    public static class STLBinaryReader{
       
        /**
        * @brief  *.stl file binary read function
        * @param  filePath
        * @retval meshList
        */
        public static Volume ReadFile(string filePath) {
            Dictionary<Vector3, int> indices = new Dictionary<Vector3, int>();
            List<Vector3> vertices = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<Triangle> triangleIndices = new List<Triangle>();

            byte[] fileBytes = File.ReadAllBytes(filePath);

            /* 80 bytes title + 4 byte num of triangles + 50 bytes (1 of triangular mesh)  */
            if(fileBytes.Length <= 120) {
                throw new Exception("File has less than 120 bytes!");
            }

            byte[] temp = new byte[4];

            temp[0] = fileBytes[80];
            temp[1] = fileBytes[81];
            temp[2] = fileBytes[82];
            temp[3] = fileBytes[83];

            var numOfMesh = System.BitConverter.ToInt32(temp, 0);

            var byteIndex = 84;

            const int vectorSizeInBytes = 12;

            for(int i = 0; i < numOfMesh; i++) {
                /* this try-catch block will be reviewed */
                try {
                    /* face normal */
                    var normal = ReadVector(fileBytes, byteIndex + 0 * vectorSizeInBytes);
                    /* vertex 1 */
                    var vertex1 = ReadVector(fileBytes, byteIndex + 1 * vectorSizeInBytes);
                    /* vertex 2 */
                    var vertex2 = ReadVector(fileBytes, byteIndex + 2 * vectorSizeInBytes);
                    /* vertex 3 */
                    var vertex3 = ReadVector(fileBytes, byteIndex + 3 * vectorSizeInBytes);

                    byteIndex += vectorSizeInBytes * 4;

                    // Create triangle, check if vertices already exist
                    var I1 = GetOrAddVertexIndex(indices, vertex1);
                    if(I1 >= vertices.Count) {
                        if(vertices.Count != normals.Count)
                            throw new Exception("Vertices and normals should have same length!");
                        vertices.Add(vertex1);
                        normals.Add(normal);
                    }

                    var I2 = GetOrAddVertexIndex(indices, vertex2);
                    if(I2 >= vertices.Count) {
                        if(vertices.Count != normals.Count)
                            throw new Exception("Vertices and normals should have same length!");
                        vertices.Add(vertex2);
                        normals.Add(normal);
                    }

                    var I3 = GetOrAddVertexIndex(indices, vertex3);
                    if(I3 >= vertices.Count) {
                        if(vertices.Count != normals.Count)
                            throw new Exception("Vertices and normals should have same length!");
                        vertices.Add(vertex3);
                        normals.Add(normal);
                    }

                    // Add triangle to list of triangles
                    triangleIndices.Add(new Triangle(I1, I2, I3));

                    byteIndex += 2; // Attribute byte count
                }
                catch {
                    throw new FileLoadException($"Error reading file: {filePath}!");
                }
            }
            return new Volume(vertices.ToArray(),
                             triangleIndices.ToArray(),
                             normals.ToArray()
                             );
        }


        
        public static float ReadFloat(byte[] bytes, int startIndex) {
            return System.BitConverter.ToSingle(new byte[] { bytes[startIndex], bytes[startIndex + 1], bytes[startIndex + 2], bytes[startIndex + 3] }, 0);
        }

        public static Vector3 ReadVector(byte[] bytes, int startIndex) {
            var x = ReadFloat(bytes, startIndex);
            var y = ReadFloat(bytes, startIndex + 4);
            var z = ReadFloat(bytes, startIndex + 8);

            return new Vector3(x, y, z);
        }

        public static int GetOrAddVertexIndex(Dictionary<Vector3, int> indices, Vector3 vertex) {
            // Create triangle, check if vertices already exist
            int index = -1;
            // First vertex
            if(indices.ContainsKey(vertex)) {
                index = indices[vertex];
            }
            else {
                index = indices.Count;
                // Add vertex to dictionary
                indices.Add(vertex, index);
            }
            return index;
        }
    }
}
