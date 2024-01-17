using SoftRenderingApp3D.DataStructures.Drawables;
using SoftRenderingApp3D.DataStructures.Meshes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Numerics;
using System.Threading;

namespace SoftRenderingApp3D.DataStructures.FileReaders
{
    public class STLReader : FileReader
    {
        private readonly Dictionary<Vector3, int> indices;

        public string path; // file path
        private bool processError;

        /**
        * @brief  Class instance constructor
        * @param  none
        * @retval none
        */
        public STLReader(string filePath = "")
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
            path = filePath;
            processError = false;
            indices = new Dictionary<Vector3, int>();
        }

        public override IDrawable ReadFile(string fileName)
        {
            path = fileName;
            return NewSTLImport().ToDrawable();
        }


        /**
        * @brief  This function returns the process error value if its true there is an error on process
        * @param  none
        * @retval none
        */
        public bool Get_Process_Error()
        {
            return processError;
        }


        /**
        * @brief  *.stl file main read function
        * @param  none
        * @retval SubsurfaceScatteringVolume
        */
        private IMesh NewSTLImport()
        {
            var stlFileType = GetFileType(path);

            if(stlFileType == FileType.ASCII)
            {
                return ReadASCIIFile(path);
            }
            else if(stlFileType == FileType.BINARY)
            {
                return ReadBinaryFile(path);
            }
            else
            {
                throw new FileLoadException($"Cannot load file format of {path}");
            }
        }

        /**
         * @brief  This function checks the type of stl file binary or ascii, function is assuming
         * given file as proper *.stl file 
         * @param  none
         * @retval stlFileType
         */
        private FileType GetFileType(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return FileType.NONE;
            }

            using (var stream = File.OpenRead(filePath))
            {
                // Start by reading the first line of the file and check if it matches the "solid" keyword
                using (var reader = new StreamReader(stream))
                {
                    var firstLine = reader.ReadLine();

                    if (firstLine != null && firstLine.TrimStart().StartsWith("solid"))
                    {
                        // Move stream pointer back to the beginning to check for binary file
                        stream.Position = 0;
                        var buffer = new byte[80];
                        stream.Read(buffer, 0, 80); // Read header of binary STL
                        var header = System.Text.Encoding.ASCII.GetString(buffer).Trim();
                
                        if (!header.StartsWith("solid"))
                        {
                            return FileType.BINARY;
                        }

                        // Additional check for "endsolid" at the end of the file
                        if (stream.Length > 256) // Check only the last 256 bytes
                        {
                            stream.Position = stream.Length - 256;
                            buffer = new byte[256];
                            stream.Read(buffer, 0, 256);
                            var endOfFile = System.Text.Encoding.ASCII.GetString(buffer);

                            if (endOfFile.Contains("endsolid"))
                            {
                                return FileType.ASCII;
                            }
                        }
                    }
                }
            }

            return FileType.BINARY;
        }


        /**
        * @brief  *.stl file binary read function
        * @param  filePath
        * @retval meshList
        */
        private IMesh ReadBinaryFile(string filePath)
        {
            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();
            var triangleIndices = new List<Facet>();

            var fileBytes = File.ReadAllBytes(filePath);

            var temp = new byte[4];

            /* 80 bytes title + 4 byte num of triangles + 50 bytes (1 of triangular mesh)  */
            if(fileBytes.Length > 120)
            {
                temp[0] = fileBytes[80];
                temp[1] = fileBytes[81];
                temp[2] = fileBytes[82];
                temp[3] = fileBytes[83];

                var numOfMesh = BitConverter.ToInt32(temp, 0);

                var byteIndex = 84;

                // Used to index the vertices
                var vertexIndex = 0;

                for(var i = 0; i < numOfMesh; i++)
                {
                    /* this try-catch block will be reviewed */
                    try
                    {
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
                        if(indices.ContainsKey(vertex1))
                        {
                            I1 = indices[vertex1];
                        }
                        else
                        {
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
                        if(indices.ContainsKey(vertex2))
                        {
                            I2 = indices[vertex2];
                        }
                        else
                        {
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
                        if(indices.ContainsKey(vertex3))
                        {
                            I3 = indices[vertex3];
                        }
                        else
                        {
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
                        triangleIndices.Add(new Facet(I1, I2, I3));

                        byteIndex += 2; // Attribute byte count
                    }
                    catch
                    {
                        processError = true;
                        break;
                    }
                }
            }

            // itentionally left blank
            if(processError)
            {
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
       
        
        private void SkipLines(StreamReader reader, int count)
        {
            for (int i = 0; i < count; i++)
            {
                reader.ReadLine();
            }
        }
        
        private Vector3 ParseVector3(string[] parts, int startIndex)
        {
            return new Vector3(
                float.Parse(parts[startIndex], CultureInfo.InvariantCulture),
                float.Parse(parts[startIndex + 1], CultureInfo.InvariantCulture),
                float.Parse(parts[startIndex + 2], CultureInfo.InvariantCulture));
        }
        private int ParseVertex(StreamReader reader, Dictionary<Vector3, int> indices, List<Vector3> vertices, List<Vector3> normals, Vector3 normal)
        {
            var line = reader.ReadLine().Trim();
            var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var vertex = ParseVector3(parts, 1);

            if (indices.TryGetValue(vertex, out int index))
            {
                return index;
            }
            
            int newIndex = vertices.Count;
            vertices.Add(vertex);
            normals.Add(normal);
            indices[vertex] = newIndex;
            return newIndex;
        }
        
        private IMesh ReadASCIIFile(string filePath)
        {
            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();
            var triangleIndices = new List<Facet>();
            var indices = new Dictionary<Vector3, int>();

            using (var reader = new StreamReader(filePath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var parts = line.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length < 1) continue;

                    if (parts[0] == "solid") continue;
                    if (parts[0] == "endsolid") break;

                    try
                    {
                        if (parts[0] == "facet" && parts[1] == "normal")
                        {
                            var normal = ParseVector3(parts, 2);
                            SkipLines(reader, 1);
                            var vertex1 = ParseVertex(reader, indices, vertices, normals, normal);
                            var vertex2 = ParseVertex(reader, indices, vertices, normals, normal);
                            var vertex3 = ParseVertex(reader, indices, vertices, normals, normal);
                            triangleIndices.Add(new Facet(vertex1, vertex2, vertex3));
                            SkipLines(reader, 2);
                        }
                    }
                    catch
                    {
                        processError = true;
                        break;
                    }
                }
            }

            return processError ? null : new Mesh(vertices.ToArray(), triangleIndices.ToArray(), normals.ToArray());
        }
        
        private enum FileType { NONE, BINARY, ASCII } // stl file type enumeration
    }
}
