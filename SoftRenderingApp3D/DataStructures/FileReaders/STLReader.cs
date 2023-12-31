using SoftRenderingApp3D.DataStructures.Meshes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading;

namespace SoftRenderingApp3D.DataStructures.FileReaders {
    public class STLReader : FileReader {
        private readonly Dictionary<Vector3, int> indices;

        public string path; // file path
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
            indices = new Dictionary<Vector3, int>();
        }

        public override IEnumerable<Mesh> ReadFile(string fileName) {
            path = fileName;
            return NewSTLImport();
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
        * @retval SubsurfaceScatteringVolume
        */
        public IEnumerable<Mesh> NewSTLImport() {
            var stlFileType = GetFileType(path);

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

        /**
         * @brief  This function checks the type of stl file binary or ascii, function is assuming
         * given file as proper *.stl file 
         * @param  none
         * @retval stlFileType
         */
        private FileType GetFileType(string filePath) {
            var stlFileType = FileType.NONE;

            /* check path is exist */
            if(File.Exists(filePath)) {
                var lineCount = 0;
                lineCount = File.ReadLines(filePath).Count(); // number of lines in the file

                var firstLine = File.ReadLines(filePath).First();

                var endLines = File.ReadLines(filePath).Skip(lineCount - 1).Take(1).First() +
                               File.ReadLines(filePath).Skip(lineCount - 2).Take(1).First();

                /* check the file is ascii or not */
                if((firstLine.IndexOf("solid") != -1) &
                   (endLines.IndexOf("endsolid") != -1)) {
                    stlFileType = FileType.ASCII;
                }
                else {
                    stlFileType = FileType.BINARY;
                }
            }
            else {
                stlFileType = FileType.NONE;
            }


            return stlFileType;
        }


        /**
        * @brief  *.stl file binary read function
        * @param  filePath
        * @retval meshList
        */
        private Mesh ReadBinaryFile(string filePath) {
            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();
            var triangleIndices = new List<Triangle>();

            var fileBytes = File.ReadAllBytes(filePath);

            var temp = new byte[4];

            /* 80 bytes title + 4 byte num of triangles + 50 bytes (1 of triangular mesh)  */
            if(fileBytes.Length > 120) {
                temp[0] = fileBytes[80];
                temp[1] = fileBytes[81];
                temp[2] = fileBytes[82];
                temp[3] = fileBytes[83];

                var numOfMesh = BitConverter.ToInt32(temp, 0);

                var byteIndex = 84;

                // Used to index the vertices
                var vertexIndex = 0;

                for(var i = 0; i < numOfMesh; i++) {
                    /* this try-catch block will be reviewed */
                    try {
                        /* face normal */
                        var normalX = BitConverter.ToSingle(
                            new[] {
                                fileBytes[byteIndex], fileBytes[byteIndex + 1], fileBytes[byteIndex + 2],
                                fileBytes[byteIndex + 3]
                            }, 0);
                        byteIndex += 4;
                        var normalY = BitConverter.ToSingle(
                            new[] {
                                fileBytes[byteIndex], fileBytes[byteIndex + 1], fileBytes[byteIndex + 2],
                                fileBytes[byteIndex + 3]
                            }, 0);
                        byteIndex += 4;
                        var normalZ = BitConverter.ToSingle(
                            new[] {
                                fileBytes[byteIndex], fileBytes[byteIndex + 1], fileBytes[byteIndex + 2],
                                fileBytes[byteIndex + 3]
                            }, 0);
                        byteIndex += 4;

                        /* vertex 1 */
                        var x = BitConverter.ToSingle(
                            new[] {
                                fileBytes[byteIndex], fileBytes[byteIndex + 1], fileBytes[byteIndex + 2],
                                fileBytes[byteIndex + 3]
                            }, 0);
                        byteIndex += 4;
                        var y = BitConverter.ToSingle(
                            new[] {
                                fileBytes[byteIndex], fileBytes[byteIndex + 1], fileBytes[byteIndex + 2],
                                fileBytes[byteIndex + 3]
                            }, 0);
                        byteIndex += 4;
                        var z = BitConverter.ToSingle(
                            new[] {
                                fileBytes[byteIndex], fileBytes[byteIndex + 1], fileBytes[byteIndex + 2],
                                fileBytes[byteIndex + 3]
                            }, 0);
                        byteIndex += 4;

                        var vertex1 = new Vector3(x, y, z);

                        /* vertex 2 */
                        x = BitConverter.ToSingle(
                            new[] {
                                fileBytes[byteIndex], fileBytes[byteIndex + 1], fileBytes[byteIndex + 2],
                                fileBytes[byteIndex + 3]
                            }, 0);
                        byteIndex += 4;
                        y = BitConverter.ToSingle(
                            new[] {
                                fileBytes[byteIndex], fileBytes[byteIndex + 1], fileBytes[byteIndex + 2],
                                fileBytes[byteIndex + 3]
                            }, 0);
                        byteIndex += 4;
                        z = BitConverter.ToSingle(
                            new[] {
                                fileBytes[byteIndex], fileBytes[byteIndex + 1], fileBytes[byteIndex + 2],
                                fileBytes[byteIndex + 3]
                            }, 0);
                        byteIndex += 4;

                        var vertex2 = new Vector3(x, y, z);

                        /* vertex 3 */
                        x = BitConverter.ToSingle(
                            new[] {
                                fileBytes[byteIndex], fileBytes[byteIndex + 1], fileBytes[byteIndex + 2],
                                fileBytes[byteIndex + 3]
                            }, 0);
                        byteIndex += 4;
                        y = BitConverter.ToSingle(
                            new[] {
                                fileBytes[byteIndex], fileBytes[byteIndex + 1], fileBytes[byteIndex + 2],
                                fileBytes[byteIndex + 3]
                            }, 0);
                        byteIndex += 4;
                        z = BitConverter.ToSingle(
                            new[] {
                                fileBytes[byteIndex], fileBytes[byteIndex + 1], fileBytes[byteIndex + 2],
                                fileBytes[byteIndex + 3]
                            }, 0);
                        byteIndex += 4;

                        var vertex3 = new Vector3(x, y, z);

                        // Create triangle, check if vertices already exist
                        int I1, I2, I3 = -1;
                        // First vertex
                        if(indices.ContainsKey(vertex1)) {
                            I1 = indices[vertex1];
                        }
                        else {
                            I1 = vertexIndex;
                            // Add vertex to dictionary
                            indices.Add(vertex1, vertexIndex);
                            // Add vertex to list of vertices
                            vertices.Add(vertex1);
                            // Add the normal for the vertex, same for all vertices of a triangle
                            normals.Add(new Vector3(normalX, normalY, normalZ));
                            vertexIndex++;
                        }

                        // Second vertex
                        if(indices.ContainsKey(vertex2)) {
                            I2 = indices[vertex2];
                        }
                        else {
                            I2 = vertexIndex;
                            // Add vertex to dictionary
                            indices.Add(vertex2, vertexIndex);
                            // Add vertex to list of vertices
                            vertices.Add(vertex2);
                            // Add the normal for the vertex, same for all vertices of a triangle
                            normals.Add(new Vector3(normalX, normalY, normalZ));
                            vertexIndex++;
                        }

                        // Third vertex
                        if(indices.ContainsKey(vertex3)) {
                            I3 = indices[vertex3];
                        }
                        else {
                            I3 = vertexIndex;
                            // Add vertex to dictionary
                            indices.Add(vertex3, vertexIndex);
                            // Add vertex to list of vertices
                            vertices.Add(vertex3);
                            // Add the normal for the vertex, same for all vertices of a triangle
                            normals.Add(new Vector3(normalX, normalY, normalZ));
                            vertexIndex++;
                        }

                        // Add triangle to list of triangles
                        triangleIndices.Add(new Triangle(I1, I2, I3));

                        byteIndex += 2; // Attribute byte count
                    }
                    catch {
                        processError = true;
                        break;
                    }
                }
            }

            // itentionally left blank
            if(processError) {
                throw new FileLoadException($"Error reading file: {path}!");
            }

            return new Mesh(vertices.ToArray(),
                triangleIndices.ToArray(),
                normals.ToArray());
        }


        /**
        * @brief  *.stl file ascii read function
        * @param  filePath
        * @retval meshList
        */
        private Mesh ReadASCIIFile(string filePath) {
            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();
            var triangleIndices = new List<Triangle>();

            var txtReader = new StreamReader(filePath);

            string lineString;

            while(!txtReader.EndOfStream) {
                lineString = txtReader.ReadLine().Trim(); /* delete whitespace in front and tail of the string */
                var lineData = lineString.Split(' ');

                var vertexIndex = 0;

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

                            var normal = new Vector3(float.Parse(lineData[2]), float.Parse(lineData[3]),
                                float.Parse(lineData[4]));

                            //----------------------------------------------------------------------
                            lineString = txtReader.ReadLine(); // Just skip the OuterLoop line
                            //----------------------------------------------------------------------

                            // Vertex1
                            lineString = txtReader.ReadLine().Trim();
                            /* reduce spaces until string has proper format for split */
                            while(lineString.IndexOf("  ") != -1) {
                                lineString = lineString.Replace("  ", " ");
                            }

                            lineData = lineString.Split(' ');

                            var vertex1 = new Vector3(float.Parse(lineData[1]), float.Parse(lineData[2]),
                                float.Parse(lineData[3])); // x1

                            // Vertex2
                            lineString = txtReader.ReadLine().Trim();
                            /* reduce spaces until string has proper format for split */
                            while(lineString.IndexOf("  ") != -1) {
                                lineString = lineString.Replace("  ", " ");
                            }

                            lineData = lineString.Split(' ');

                            var vertex2 = new Vector3(float.Parse(lineData[1]), float.Parse(lineData[2]),
                                float.Parse(lineData[3])); // x1

                            // Vertex3
                            lineString = txtReader.ReadLine().Trim();
                            /* reduce spaces until string has proper format for split */
                            while(lineString.IndexOf("  ") != -1) {
                                lineString = lineString.Replace("  ", " ");
                            }

                            lineData = lineString.Split(' ');

                            var vertex3 = new Vector3(float.Parse(lineData[1]), float.Parse(lineData[2]),
                                float.Parse(lineData[3])); // x1

                            // Create triangle, check if vertices already exist
                            int I1, I2, I3 = -1;
                            // First vertex
                            if(indices.ContainsKey(vertex1)) {
                                I1 = indices[vertex1];
                            }
                            else {
                                I1 = vertexIndex;
                                // Add vertex to dictionary
                                indices.Add(vertex1, vertexIndex);
                                // Add vertex to list of vertices
                                vertices.Add(vertex1);
                                // Add the normal for the vertex, same for all vertices of a triangle
                                normals.Add(normal);
                                vertexIndex++;
                            }

                            // Second vertex
                            if(indices.ContainsKey(vertex2)) {
                                I2 = indices[vertex2];
                            }
                            else {
                                I2 = vertexIndex;
                                // Add vertex to dictionary
                                indices.Add(vertex2, vertexIndex);
                                // Add vertex to list of vertices
                                vertices.Add(vertex2);
                                // Add the normal for the vertex, same for all vertices of a triangle
                                normals.Add(normal);
                                vertexIndex++;
                            }

                            // Third vertex
                            if(indices.ContainsKey(vertex3)) {
                                I3 = indices[vertex3];
                            }
                            else {
                                I3 = vertexIndex;
                                // Add vertex to dictionary
                                indices.Add(vertex3, vertexIndex);
                                // Add vertex to list of vertices
                                vertices.Add(vertex3);
                                // Add the normal for the vertex, same for all vertices of a triangle
                                normals.Add(normal);
                                vertexIndex++;
                            }

                            // Add triangle to list of triangles
                            triangleIndices.Add(new Triangle(I1, I2, I3));
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

            return new Mesh(vertices.ToArray(),
                triangleIndices.ToArray(),
                normals.ToArray());
        }

        private enum FileType { NONE, BINARY, ASCII } // stl file type enumeration
    }
}