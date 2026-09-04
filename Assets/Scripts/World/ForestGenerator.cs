using System.Collections.Generic;
using UnityEngine;

namespace Lumber.World
{
    /// Scatters low-poly trees across a circular map using simple min-spacing
    /// rejection sampling, leaving a safe empty zone around the player's spawn.
    public class ForestGenerator : MonoBehaviour
    {
        public int treeCount = 60;
        public float mapRadius = 45f;
        public float minSpacing = 3.2f;
        public float safeZoneRadius = 6f;

        public Material trunkMaterial;
        public Material leafMaterial;

        public List<Tree> SpawnedTrees { get; } = new List<Tree>();

        public void Generate(int seed)
        {
            var rng = new System.Random(seed);
            var placed = new List<Vector2>();

            int attempts = 0;
            int maxAttempts = treeCount * 40;

            while (SpawnedTrees.Count < treeCount && attempts < maxAttempts)
            {
                attempts++;
                Vector2 p = RandomInCircle(rng, mapRadius);
                if (p.magnitude < safeZoneRadius) continue;

                bool tooClose = false;
                foreach (var q in placed)
                {
                    if ((p - q).sqrMagnitude < minSpacing * minSpacing)
                    {
                        tooClose = true;
                        break;
                    }
                }
                if (tooClose) continue;

                placed.Add(p);

                var treeGo = LowPolyTreeFactory.Create(trunkMaterial, leafMaterial, rng);
                treeGo.transform.SetParent(transform, false);
                treeGo.transform.localPosition = new Vector3(p.x, 0f, p.y);

                var tree = treeGo.AddComponent<Tree>();
                SpawnedTrees.Add(tree);
            }
        }

        private static Vector2 RandomInCircle(System.Random rng, float radius)
        {
            float angle = (float)(rng.NextDouble() * Mathf.PI * 2.0);
            float r = radius * Mathf.Sqrt((float)rng.NextDouble());
            return new Vector2(Mathf.Cos(angle) * r, Mathf.Sin(angle) * r);
        }
    }
}
