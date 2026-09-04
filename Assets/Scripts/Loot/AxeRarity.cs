using System.Collections.Generic;
using UnityEngine;

namespace Lumber.Loot
{
    public enum AxeRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }

    /// Static tuning table: drop weight, color and stat range for each rarity.
    public class RarityDef
    {
        public readonly string label;
        public readonly float weight;
        public readonly Color color;
        public readonly int minDamage;
        public readonly int maxDamage;
        public readonly float minCooldown;
        public readonly float maxCooldown;

        public RarityDef(string label, float weight, Color color, int minDamage, int maxDamage, float minCooldown, float maxCooldown)
        {
            this.label = label;
            this.weight = weight;
            this.color = color;
            this.minDamage = minDamage;
            this.maxDamage = maxDamage;
            this.minCooldown = minCooldown;
            this.maxCooldown = maxCooldown;
        }
    }

    public static class RarityInfo
    {
        public static readonly Dictionary<AxeRarity, RarityDef> Table = new Dictionary<AxeRarity, RarityDef>
        {
            { AxeRarity.Common,    new RarityDef("Commune",     0.60f, new Color(0.72f, 0.72f, 0.72f), 8,  14, 0.42f, 0.50f) },
            { AxeRarity.Uncommon,  new RarityDef("Peu commune", 0.25f, new Color(0.35f, 0.82f, 0.42f), 14, 22, 0.34f, 0.42f) },
            { AxeRarity.Rare,      new RarityDef("Rare",        0.10f, new Color(0.30f, 0.55f, 0.95f), 22, 34, 0.28f, 0.34f) },
            { AxeRarity.Epic,      new RarityDef("Epique",      0.04f, new Color(0.65f, 0.30f, 0.90f), 34, 52, 0.20f, 0.28f) },
            { AxeRarity.Legendary, new RarityDef("Legendaire",  0.01f, new Color(0.95f, 0.70f, 0.15f), 52, 80, 0.13f, 0.20f) },
        };

        /// Rarities ordered rarest-first, handy for weighted rolls and sorted UI listings.
        public static readonly AxeRarity[] RarestFirst =
        {
            AxeRarity.Legendary, AxeRarity.Epic, AxeRarity.Rare, AxeRarity.Uncommon, AxeRarity.Common
        };
    }
}
