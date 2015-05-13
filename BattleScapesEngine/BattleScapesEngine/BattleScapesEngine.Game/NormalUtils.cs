using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiliconStudio.Core.Mathematics;

namespace BattleScapesEngine
{
    public static class NormalUtils
    {
        public static Vertex[] CalculateNormals(int[] Indices, Vertex[] vertices)
        {
            for (int tri = 0; tri < Indices.Length; tri += 3)
            {
                Triangle t = new Triangle();
                t.i1 = Indices[tri];
                t.i2 = Indices[tri + 1];
                t.i3 = Indices[tri + 2];
                t.p1 = vertices[t.i1];
                t.p2 = vertices[t.i2];
                t.p3 = vertices[t.i3];

                Vector3 normal = GetNormal(t.p1.Position, t.p2.Position, t.p3.Position);
                t.n = normal;

                t.p1.ConnectedTriangles.Add(t);
                t.p2.ConnectedTriangles.Add(t);
                t.p3.ConnectedTriangles.Add(t);
            }

            for (int v = 0; v < vertices.Length; v++)
            {
                for (int ct = 0; ct < vertices[v].ConnectedTriangles.Count; ct++)
                {
                    vertices[v].Normal += vertices[v].ConnectedTriangles[ct].n;
                }

                vertices[v].Normal = vertices[v].Normal;
                vertices[v].Normal.Normalize();
            }

            return vertices;
        }

        public static Vector3 GetNormal(Vector3 v1, Vector3 v2, Vector3 v3)
        {
            Vector3 a = v2 - v1;
            Vector3 b = v3 - v1;

            Vector3 normal = Vector3.Cross(b, a);
            normal.Normalize();

            return normal;
        }
    }
}
