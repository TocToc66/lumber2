using System;
using System.Collections.Generic;
using UnityEngine;
using Lumber.Player;
using Lumber.Economy;

namespace Lumber.Shop
{
    [Serializable]
    public class AxeTier
    {
        public string tierName;
        public int cost;
        public int damage;
        public float cooldown;

        public AxeTier(string tierName, int cost, int damage, float cooldown)
        {
            this.tierName = tierName;
            this.cost = cost;
            this.damage = damage;
            this.cooldown = cooldown;
        }
    }

    /// Holds the axe upgrade tiers and applies the currently equipped one to the AxeTool.
    public class ShopManager : MonoBehaviour
    {
        public readonly List<AxeTier> tiers = new List<AxeTier>
        {
            new AxeTier("Hache en bois",       0,    10, 0.45f),
            new AxeTier("Hache en pierre",     60,   16, 0.40f),
            new AxeTier("Hache en fer",        220,  26, 0.34f),
            new AxeTier("Hache en acier",      650,  40, 0.28f),
            new AxeTier("Hache en or",         1600, 60, 0.22f),
            new AxeTier("Hache legendaire",    4000, 95, 0.16f),
        };

        public int CurrentTierIndex { get; private set; }
        public event Action<int> OnTierChanged;

        private AxeTool axeTool;
        private EconomyManager economy;

        public void Init(AxeTool axe, EconomyManager economyManager, int startingTier)
        {
            axeTool = axe;
            economy = economyManager;
            CurrentTierIndex = Mathf.Clamp(startingTier, 0, tiers.Count - 1);
            Apply();
        }

        public bool TryBuyNext()
        {
            int next = CurrentTierIndex + 1;
            if (next >= tiers.Count) return false;

            var tier = tiers[next];
            if (!economy.TrySpend(tier.cost)) return false;

            CurrentTierIndex = next;
            Apply();
            OnTierChanged?.Invoke(CurrentTierIndex);
            return true;
        }

        private void Apply()
        {
            var tier = tiers[CurrentTierIndex];
            axeTool.SetStats(tier.damage, tier.cooldown);
        }
    }
}
