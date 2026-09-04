using System;
using System.Collections.Generic;
using UnityEngine;
using Lumber.Player;

namespace Lumber.Loot
{
    /// Owns the player's axe collection and unopened boxes. Boxes are earned by chopping
    /// (see GameManager) and opened here for a randomly rolled axe (rarity + stat roll).
    /// The best-scoring axe is auto-equipped whenever a better one is obtained.
    public class InventoryManager : MonoBehaviour
    {
        public static InventoryManager Instance { get; private set; }

        public event Action OnInventoryChanged;
        public event Action<AxeInstance> OnBoxOpened;
        public event Action<int> OnBoxCountChanged;

        private readonly List<AxeInstance> axes = new List<AxeInstance>();
        private readonly System.Random rng = new System.Random();
        private AxeTool axeTool;
        private string equippedId;
        private int boxCount;
        private int maxBoxCapacity = 5;
        private float forceBonusPercent;

        public IReadOnlyList<AxeInstance> Axes => axes;
        public int BoxCount => boxCount;
        public int MaxBoxCapacity => maxBoxCapacity;
        public string EquippedId => equippedId;

        public AxeInstance Equipped
        {
            get
            {
                foreach (var a in axes)
                    if (a.id == equippedId) return a;
                return null;
            }
        }

        public void Init(AxeTool tool, int savedBoxCount, List<AxeInstance> savedAxes, string savedEquippedId, int capacity)
        {
            Instance = this;
            axeTool = tool;
            maxBoxCapacity = capacity;
            boxCount = Mathf.Clamp(savedBoxCount, 0, maxBoxCapacity);

            axes.Clear();
            if (savedAxes != null) axes.AddRange(savedAxes);
            if (axes.Count == 0)
                axes.Add(CreateStarterAxe());

            equippedId = savedEquippedId;
            if (string.IsNullOrEmpty(equippedId) || FindAxe(equippedId) == null)
                equippedId = axes[0].id;

            ApplyEquipped();
        }

        private static AxeInstance CreateStarterAxe()
        {
            return new AxeInstance
            {
                id = Guid.NewGuid().ToString(),
                axeName = "Hache commune",
                rarity = AxeRarity.Common,
                damage = 10,
                cooldown = 0.45f
            };
        }

        public void SetMaxBoxCapacity(int capacity)
        {
            maxBoxCapacity = Mathf.Max(1, capacity);
            if (boxCount > maxBoxCapacity)
            {
                boxCount = maxBoxCapacity;
                OnBoxCountChanged?.Invoke(boxCount);
            }
        }

        public void SetForceBonusPercent(float percent)
        {
            forceBonusPercent = percent;
            ApplyEquipped();
        }

        public bool AddBox()
        {
            if (boxCount >= maxBoxCapacity) return false;
            boxCount++;
            OnBoxCountChanged?.Invoke(boxCount);
            return true;
        }

        public AxeInstance OpenBox()
        {
            if (boxCount <= 0) return null;

            boxCount--;
            OnBoxCountChanged?.Invoke(boxCount);

            var rarity = RollRarity();
            var def = RarityInfo.Table[rarity];
            int damage = Mathf.RoundToInt(Mathf.Lerp(def.minDamage, def.maxDamage, (float)rng.NextDouble()));
            float cooldown = Mathf.Lerp(def.minCooldown, def.maxCooldown, (float)rng.NextDouble());

            var instance = new AxeInstance
            {
                id = Guid.NewGuid().ToString(),
                axeName = "Hache " + def.label.ToLowerInvariant(),
                rarity = rarity,
                damage = damage,
                cooldown = cooldown
            };
            axes.Add(instance);

            var current = Equipped;
            if (current == null || Score(instance) > Score(current))
            {
                equippedId = instance.id;
                ApplyEquipped();
            }

            OnBoxOpened?.Invoke(instance);
            OnInventoryChanged?.Invoke();
            return instance;
        }

        public void Equip(string id)
        {
            if (FindAxe(id) == null) return;
            equippedId = id;
            ApplyEquipped();
            OnInventoryChanged?.Invoke();
        }

        public static float Score(AxeInstance a)
        {
            return a.damage / Mathf.Max(0.05f, a.cooldown);
        }

        private AxeInstance FindAxe(string id)
        {
            foreach (var a in axes)
                if (a.id == id) return a;
            return null;
        }

        private void ApplyEquipped()
        {
            var eq = Equipped;
            if (eq == null || axeTool == null) return;
            int effectiveDamage = Mathf.RoundToInt(eq.damage * (1f + forceBonusPercent));
            axeTool.SetStats(effectiveDamage, eq.cooldown);
        }

        private AxeRarity RollRarity()
        {
            float roll = (float)rng.NextDouble();
            float cumulative = 0f;
            foreach (var r in RarityInfo.RarestFirst)
            {
                cumulative += RarityInfo.Table[r].weight;
                if (roll <= cumulative) return r;
            }
            return AxeRarity.Common;
        }
    }
}
