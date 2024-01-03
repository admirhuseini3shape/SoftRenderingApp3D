using SoftRenderingApp3D.DataStructures.Drawables;
using SoftRenderingApp3D.DataStructures.Meshes;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace SoftRenderingApp3D.DataStructures.Shapes
{
    public class Sphere
    {
        private int index;

        private Dictionary<long, int> middlePointIndexCache;

        public Sphere(int recursionLevel)
        {
            create(recursionLevel);
        }

        public List<Vector3> points { get; } = new List<Vector3>();
        public List<Facet> faces { get; private set; } = new List<Facet>();

        // return index of point in the middle of p1 and p2
        private int getMiddlePoint(int p1, int p2)
        {
            // first check if we have it already
            var firstIsSmaller = p1 < p2;
            long smallerIndex = firstIsSmaller ? p1 : p2;
            long greaterIndex = firstIsSmaller ? p2 : p1;
            var key = (smallerIndex << 32) + greaterIndex;

            if(middlePointIndexCache.TryGetValue(key, out var ret))
            {
                return ret;
            }

            // not in cache, calculate it
            var point1 = points[p1];
            var point2 = points[p2];
            var middle = new Vector3(
                (point1.X + point2.X) / 2f,
                (point1.Y + point2.Y) / 2f,
                (point1.Z + point2.Z) / 2f);

            // add vertex makes sure point is on unit sphere
            var i = addVertex(middle);

            // store it, return index
            middlePointIndexCache.Add(key, i);
            return i;
        }

        private int addVertex(Vector3 p)
        {
            points.Add(Vector3.Normalize(p));
            return index++;
        }

        private void create(int recursionLevel)
        {
            middlePointIndexCache = new Dictionary<long, int>();
            index = 0;

            // create 12 vertices of a icosahedron
            var t = (1f + (float)Math.Sqrt(5f)) / 2f;

            addVertex(new Vector3(-1, t, 0));
            addVertex(new Vector3(1, t, 0));
            addVertex(new Vector3(-1, -t, 0));
            addVertex(new Vector3(1, -t, 0));

            addVertex(new Vector3(0, -1, t));
            addVertex(new Vector3(0, 1, t));
            addVertex(new Vector3(0, -1, -t));
            addVertex(new Vector3(0, 1, -t));

            addVertex(new Vector3(t, 0, -1));
            addVertex(new Vector3(t, 0, 1));
            addVertex(new Vector3(-t, 0, -1));
            addVertex(new Vector3(-t, 0, 1));


            // create 20 triangles of the icosahedron
            faces = new List<Facet> {
                    // 5 faces around point 0
                    new Facet(0, 11, 5),
                    new Facet(0, 5, 1),
                    new Facet(0, 1, 7),
                    new Facet(0, 7, 10),
                    new Facet(0, 10, 11),
                    
                    // 5 adjacent faces 
                    new Facet(1, 5, 9),
                    new Facet(5, 11, 4),
                    new Facet(11, 10, 2),
                    new Facet(10, 7, 6),
                    new Facet(7, 1, 8),
                    
                    // 5 faces around point 3
                    new Facet(3, 9, 4),
                    new Facet(3, 4, 2),
                    new Facet(3, 2, 6),
                    new Facet(3, 6, 8),
                    new Facet(3, 8, 9),
                    
                    // 5 adjacent faces 
                    new Facet(4, 9, 5),
                    new Facet(2, 4, 11),
                    new Facet(6, 2, 10),
                    new Facet(8, 6, 7),
                    new Facet(9, 8, 1)
                };


            // refine triangles
            for(var r = 0; r < recursionLevel; r++)
            {
                var faces2 = new List<Facet>();
                foreach(var tri in faces)
                {
                    // replace triangle by 4 triangles
                    var a = getMiddlePoint(tri.I0, tri.I1);
                    var b = getMiddlePoint(tri.I1, tri.I2);
                    var c = getMiddlePoint(tri.I2, tri.I0);

                    faces2.Add(new Facet(tri.I0, a, c));
                    faces2.Add(new Facet(tri.I1, b, a));
                    faces2.Add(new Facet(tri.I2, c, b));
                    faces2.Add(new Facet(a, b, c));
                }

                faces = faces2;
            }
        }

        public static IDrawable GetDrawable(int recursionLevel)
        {
            var sphere = new Sphere(recursionLevel);
            return new Mesh(sphere.points.ToArray(), sphere.faces.ToArray())
                .ToDrawable();
        }
    }
}
