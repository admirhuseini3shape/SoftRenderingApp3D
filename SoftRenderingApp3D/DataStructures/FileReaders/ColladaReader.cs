using SoftRenderingApp3D.DataStructures.Drawables;
using SoftRenderingApp3D.DataStructures.Meshes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace SoftRenderingApp3D.DataStructures.FileReaders
{
    public class ColladaReader : FileReader
    {
        
        private static XNamespace ns = "http://www.collada.org/2005/11/COLLADASchema";

        public static IEnumerable<IMesh> NewImportCollada(string fileName)
        {
            XNamespace ns = "http://www.collada.org/2005/11/COLLADASchema";

            var xdoc = XDocument.Load(fileName);

            var geometries = xdoc.Root.Element(ns + "library_geometries").Elements(ns + "geometry");

            foreach(var geometry in geometries)
            {
                var mesh = geometry.Element(ns + "mesh");
                
                var triangles = mesh.Element(ns + "triangles");
                if(triangles != null)
                {
                    var triangles_count = int.Parse(triangles.Attribute("count").Value);
                    var triangles_p = parseArray<int>(triangles.Element(ns + "p")?.Value).ToArray();

                    var stride = triangles_p.Count() / triangles_count;

                    getSource(triangles, "VERTEX", out var triangles_vertex_id, out _);
                    // getSource(triangles, "NORMAL", out var triangles_normal_id, out _);

                    var vertices = mesh.Elements(ns + "vertices")
                        .FirstOrDefault(e => e.Attribute("id")?.Value == triangles_vertex_id);
                    getSource(vertices, "POSITION", out var vertices_position_id, out _);

                    var vertices_position = getArraySource<Vector3>(mesh, vertices_position_id);
                    // var triangles_normal = getArraySource<Vector3>(mesh, triangles_normal_id);

                    yield return new Mesh(
                        vertices_position.ToArray(),
                        getTriangles(triangles_p, stride).ToArray());
                }
            }
        }

        
        private static IEnumerable<Facet> getTriangles(int[] array, int stride, int offset = 0)
        {
            var l = array.Length / stride;
            for(var i = 0; i < l; i++)
            {
                yield return new Facet(array[i * stride + offset], array[i * stride + 4 + offset],
                    array[i * stride + 8 + offset]);
            }
        }

        private static IEnumerable<T> parseArray<T>(string value)
        {
            return value?.Split(new[] { ' ', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                       ?.Select(v => (T)Convert.ChangeType(v, typeof(T), CultureInfo.InvariantCulture)) ??
                   Enumerable.Empty<T>();
        }

        private static void getSource(XElement element, string semantic, out string id, out int offset)
        {
            var e = element?.Elements(element?.GetDefaultNamespace() + "input")
                ?.FirstOrDefault(e => string.Equals(e.Attribute("semantic")?.Value, semantic));
            id = e?.Attribute("source")?.Value?.TrimStart('#');
            offset = int.Parse(e?.Attribute("offset")?.Value ?? "0");
            
        }

        private static IEnumerable<T> getArraySource<T>(XElement mesh, string id)
        {
            var data = mesh
                .Elements(ns + "source")
                .FirstOrDefault(e => e.Attribute("id").Value == id)
                .Element(ns + "float_array")
                .Value;
            

            var floats = parseArray<float>(data).ToArray();

            if(typeof(T) == typeof(Vector3))
            {
                for(var i = 0; i < floats.Length; i += 3)
                {
                    yield return (T)(object)new Vector3(floats[i], floats[i + 1], floats[i + 2]);
                }
            }

            if(typeof(T) == typeof(Vector2))
            {
                for(var i = 0; i < floats.Length; i += 2)
                {
                    yield return (T)(object)new Vector2(floats[i], floats[i + 2]);
                }
            }
        }

        public override IDrawable ReadFile(string fileName)
        {
            var meshes = NewImportCollada(fileName).ToList();
            var result = new Mesh();
            result.Append(meshes);
            return result.ToDrawable();
        }
    }
}
