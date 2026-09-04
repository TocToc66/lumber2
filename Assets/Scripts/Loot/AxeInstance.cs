using System;

namespace Lumber.Loot
{
    /// One rolled axe owned by the player (from a box). Plain serializable data so it
    /// can be saved to JSON directly.
    [Serializable]
    public class AxeInstance
    {
        public string id;
        public string axeName;
        public AxeRarity rarity;
        public int damage;
        public float cooldown;
    }
}
