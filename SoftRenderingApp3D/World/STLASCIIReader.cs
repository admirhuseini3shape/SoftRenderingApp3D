using System;
using System.Collections.Generic;
using System.Numerics;
using System.IO;

namespace SoftRenderingApp3D {
    public static class STLASCIIReader {
        /**
        * @brief  *.stl file ascii read function
        * @param  filePath
        * @retval meshList
        */
        public static Volume ReadFile(string filePath) {
            Dictionary<Vector3, int> indices = new Dictionary<Vector3, int>();
            List<Vector3> vertices = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<Triangle> triangleIndices = new List<Triangle>();

            StreamReader txtReader = new StreamReader(filePath);

            string lineString;

            while(!txtReader.EndOfStream) {
                lineString = txtReader.ReadLine().Trim(); /* delete whitespace in front and tail of the string */
                string[] lineData = lineString.Split(' ');

                if(lineData[0] == "solid") {
                    while(lineData[0] != "endsolid") {
                        /* this try-catch block will be reviewed */
                        try {
                            //Normal
                            lineString = txtReader.ReadLine().Trim();
                            if(lineString.Split(' ')[0] == "endsolid") // check if we reach at the end of file
                            {
                                break;
                            }
                            var normal = ReadVector(lineString, 2);
                            //----------------------------------------------------------------------
                            lineString = txtReader.ReadLine(); // Just skip the OuterLoop line
                            //----------------------------------------------------------------------
                            // Vertex1
                            lineString = txtReader.ReadLine().Trim();
                            var vertex1 = ReadVector(lineString, 1); // x1
                            // Vertex2
                            lineString = txtReader.ReadLine().Trim();
                            var vertex2 = ReadVector(lineString, 1); // x2
                            // Vertex3
                            lineString = txtReader.ReadLine().Trim();
                            var vertex3 = ReadVector(lineString, 1); // x3

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
                        }
                        catch {
                            throw new FileLoadException($"Error reading file {filePath}!");
                        }

                        //----------------------------------------------------------------------
                        lineString = txtReader.ReadLine(); // Just skip the endloop
                        //----------------------------------------------------------------------
                        lineString = txtReader.ReadLine(); // Just skip the endfacet


                    } // while linedata[0]
                } // if solid
            } // while !endofstream

            return new Volume(vertices.ToArray(),
                              triangleIndices.ToArray(),
                              normals.ToArray());
        }

        public static float ReadFloat(string data) {
            return float.Parse(data);
        }

        public static Vector3 ReadVector(string line, int startIndex) {
            /* reduce spaces until string has proper format for split */
            while(line.IndexOf("  ") != -1) line = line.Replace("  ", " ");
            var lineData = line.Split(' ');

            var x = ReadFloat(lineData[startIndex]);
            var y = ReadFloat(lineData[startIndex + 1]);
            var z = ReadFloat(lineData[startIndex + 2]);

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
