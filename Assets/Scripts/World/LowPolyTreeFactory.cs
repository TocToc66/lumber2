using UnityEngine;

namespace Lumber.World
{
    /// Procedurally builds a low-poly PS1-style pine tree: a faceted trunk cylinder
    /// with a few stacked faceted foliage cones on top. No imported art required.
    public static class LowPolyTreeFactory
    {
        public static GameObject Create(Material trunkMaterial, Material leafMaterial, System.Random rng)
        {
            var root = new GameObject("Tree");

            float trunkHeight = Lerp(rng, 2.2f, 3.4f);
            float trunkRadius = Lerp(rng, 0.18f, 0.3f);

            var trunkGo = CreateMeshObject("Trunk", trunkMaterial,
                MeshBuilder.Cylinder(6, trunkRadius, trunkRadius * 0.7f, trunkHeight));
            trunkGo.transform.SetParent(root.transform, false);
            trunkGo.transform.localPosition = new Vector3(0f, trunkHeight * 0.5f, 0f);

            var capsule = trunkGo.AddComponent<CapsuleCollider>();
            capsule.height = trunkHeight;
            capsule.radius = trunkRadius * 1.4f;
            capsule.center = Vector3.zero;

            int foliageLayers = rng.Next(2, 4);
            float baseY = trunkHeight * 0.55f;
            for (int i = 0; i < foliageLayers; i++)
            {
                float h = Lerp(rng, 1.1f, 1.6f) * (1f - i * 0.12f);
                float r = Lerp(rng, 1.0f, 1.5f) * (1f - i * 0.18f);

                var coneGo = CreateMeshObject("Foliage" + i, leafMaterial,
                    MeshBuilder.Cylinder(7, r, 0.02f, h));
                coneGo.transform.SetParent(root.transform, false);
                coneGo.transform.localPosition = new Vector3(0f, baseY + h * 0.5f, 0f);
                baseY += h * 0.62f;
            }

            root.transform.localRotation = Quaternion.Euler(0f, (float)(rng.NextDouble() * 360.0), 0f);
            float scale = Lerp(rng, 0.85f, 1.25f);
            root.transform.localScale = Vector3.one * scale;

            return root;
        }

        private static GameObject CreateMeshObject(string name, Material material, Mesh mesh)
        {
            var go = new GameObject(name);
            var filter = go.AddComponent<MeshFilter>();
            var renderer = go.AddComponent<MeshRenderer>();
            filter.sharedMesh = mesh;
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            return go;
        }

        private static float Lerp(System.Random rng, float a, float b)
        {
            return a + (float)rng.NextDouble() * (b - a);
        }
    }
}
