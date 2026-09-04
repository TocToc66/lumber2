using System;
using UnityEngine;

namespace Lumber.Economy
{
    public class ExperienceManager : MonoBehaviour
    {
        public static ExperienceManager Instance { get; private set; }

        /// xp, xpToNextLevel, level
        public event Action<int, int, int> OnXpChanged;
        public event Action<int> OnLevelUp;

        public int Level { get; private set; } = 1;
        public int Xp { get; private set; }

        public int XpToNextLevel => XpRequiredFor(Level);

        public void Init(int level, int xp)
        {
            Instance = this;
            Level = Mathf.Max(1, level);
            Xp = Mathf.Max(0, xp);
        }

        public void AddXp(int amount)
        {
            if (amount <= 0) return;

            Xp += amount;
            while (Xp >= XpRequiredFor(Level))
            {
                Xp -= XpRequiredFor(Level);
                Level++;
                OnLevelUp?.Invoke(Level);
            }

            OnXpChanged?.Invoke(Xp, XpRequiredFor(Level), Level);
        }

        private static int XpRequiredFor(int level)
        {
            return Mathf.RoundToInt(50f * Mathf.Pow(level, 1.35f)) + 50;
        }
    }
}
