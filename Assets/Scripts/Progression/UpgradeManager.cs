using System;
using System.Collections.Generic;
using UnityEngine;
using Lumber.Economy;

namespace Lumber.Progression
{
    [Serializable]
    public class UpgradeTrack
    {
        public string key;
        public string label;
        public string description;
        public int level;
        public int maxLevel;
        public int baseCost;
        public float costGrowth;
        public float perLevelValue;

        public int CostForNextLevel()
        {
            return Mathf.RoundToInt(baseCost * Mathf.Pow(costGrowth, level));
        }
    }

    /// Permanent meta-progression bought with money: upgrades to the lumberjack
    /// (character) and to the camp (base). Replaces the old direct axe shop now that
    /// axes are found in loot boxes instead (see InventoryManager).
    public class UpgradeManager : MonoBehaviour
    {
        public static UpgradeManager Instance { get; private set; }

        public readonly List<UpgradeTrack> character = new List<UpgradeTrack>
        {
            new UpgradeTrack { key = "endurance", label = "Endurance", description = "Vitesse de sprint",       maxLevel = 5, baseCost = 40, costGrowth = 1.6f, perLevelValue = 0.10f },
            new UpgradeTrack { key = "force",     label = "Force",     description = "Degats de hache bonus",    maxLevel = 5, baseCost = 60, costGrowth = 1.7f, perLevelValue = 0.06f },
            new UpgradeTrack { key = "chance",    label = "Chance",    description = "Probabilite de caisse",    maxLevel = 5, baseCost = 80, costGrowth = 1.8f, perLevelValue = 0.03f },
        };

        public readonly List<UpgradeTrack> camp = new List<UpgradeTrack>
        {
            new UpgradeTrack { key = "scierie",   label = "Scierie",   description = "Argent gagne par buche",   maxLevel = 5, baseCost = 50, costGrowth = 1.6f, perLevelValue = 0.10f },
            new UpgradeTrack { key = "pepiniere", label = "Pepiniere", description = "Repousse des arbres",      maxLevel = 5, baseCost = 45, costGrowth = 1.6f, perLevelValue = 0.08f },
            new UpgradeTrack { key = "entrepot",  label = "Entrepot",  description = "Capacite de caisses",      maxLevel = 5, baseCost = 70, costGrowth = 1.5f, perLevelValue = 1f },
        };

        public event Action OnUpgraded;

        private EconomyManager economy;

        public void Init(EconomyManager economyManager)
        {
            Instance = this;
            economy = economyManager;
        }

        public int GetLevel(string key)
        {
            var t = Find(key);
            return t != null ? t.level : 0;
        }

        public void SetLevel(string key, int level)
        {
            var t = Find(key);
            if (t != null) t.level = Mathf.Clamp(level, 0, t.maxLevel);
        }

        public bool TryUpgrade(UpgradeTrack track)
        {
            if (track.level >= track.maxLevel) return false;
            if (!economy.TrySpend(track.CostForNextLevel())) return false;

            track.level++;
            OnUpgraded?.Invoke();
            return true;
        }

        private UpgradeTrack Find(string key)
        {
            foreach (var t in character) if (t.key == key) return t;
            foreach (var t in camp) if (t.key == key) return t;
            return null;
        }

        public float SprintBonus => Find("endurance").level * Find("endurance").perLevelValue;
        public float ForceBonusPercent => Find("force").level * Find("force").perLevelValue;
        public float BoxChanceBonus => Find("chance").level * Find("chance").perLevelValue;
        public float MoneyMultiplier => 1f + Find("scierie").level * Find("scierie").perLevelValue;
        public float RegrowTimeMultiplier => Mathf.Max(0.35f, 1f - Find("pepiniere").level * Find("pepiniere").perLevelValue);
        public int BoxCapacity => 5 + Mathf.RoundToInt(Find("entrepot").level * Find("entrepot").perLevelValue);
    }
}
