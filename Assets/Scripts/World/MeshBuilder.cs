using System.Collections.Generic;
using UnityEngine;

namespace Lumber.World
{
    /// Builds simple flat-shaded (faceted) low-poly meshes, PS1-style: every triangle
    /// gets its own vertices/normals so lighting reads as hard-edged facets, never smooth.
    public static class MeshBuilder
    {
        public static Mesh Cylinder(int sides, float radiusBottom, float radiusTop, float height)
        {
            sides = Mathf.Max(3, sides);

            var verts = new List<Vector3>();
            var tris = new List<int>();
            var normals = new List<Vector3>();

            float halfHeight = height * 0.5f;

            for (int i = 0; i < sides; i++)
            {
                float a0 = (float)i / sides * Mathf.PI * 2f;
                float a1 = (float)(i + 1) / sides * Mathf.PI * 2f;

                Vector3 b0 = new Vector3(Mathf.Cos(a0) * radiusBottom, -halfHeight, Mathf.Sin(a0) * radiusBottom);
                Vector3 b1 = new Vector3(Mathf.Cos(a1) * radiusBottom, -halfHeight, Mathf.Sin(a1) * radiusBottom);
                Vector3 t0 = new Vector3(Mathf.Cos(a0) * radiusTop, halfHeight, Mathf.Sin(a0) * radiusTop);
                Vector3 t1 = new Vector3(Mathf.Cos(a1) * radiusTop, halfHeight, Mathf.Sin(a1) * radiusTop);

                AddTri(verts, tris, normals, b0, b1, t1);
                AddTri(verts, tris, normals, b0, t1, t0);
            }

            Vector3 bottomCenter = new Vector3(0f, -halfHeight, 0f);
            for (int i = 0; i < sides; i++)
            {
                float a0 = (float)i / sides * Mathf.PI * 2f;
                float a1 = (float)(i + 1) / sides * Mathf.PI * 2f;
                Vector3 b0 = new Vector3(Mathf.Cos(a0) * radiusBottom, -halfHeight, Mathf.Sin(a0) * radiusBottom);
                Vector3 b1 = new Vector3(Mathf.Cos(a1) * radiusBottom, -halfHeight, Mathf.Sin(a1) * radiusBottom);
                AddTri(verts, tris, normals, bottomCenter, b1, b0);
            }

            var mesh = new Mesh();
            mesh.indexFormat = verts.Count > 65000
                ? UnityEngine.Rendering.IndexFormat.UInt32
                : UnityEngine.Rendering.IndexFormat.UInt16;
            mesh.SetVertices(verts);
            mesh.SetTriangles(tris, 0);
            mesh.SetNormals(normals);
            mesh.RecalculateBounds();
            return mesh;
        }

        private static void AddTri(List<Vector3> verts, List<int> tris, List<Vector3> normals, Vector3 a, Vector3 b, Vector3 c)
        {
            int idx = verts.Count;
            Vector3 n = Vector3.Cross(b - a, c - a).normalized;

            verts.Add(a);
            verts.Add(b);
            verts.Add(c);

            normals.Add(n);
            normals.Add(n);
            normals.Add(n);

            tris.Add(idx);
            tris.Add(idx + 1);
            tris.Add(idx + 2);
        }
    }
}
