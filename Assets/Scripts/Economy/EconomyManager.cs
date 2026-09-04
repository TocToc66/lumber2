using System;
using UnityEngine;

namespace Lumber.Economy
{
    public class EconomyManager : MonoBehaviour
    {
        public static EconomyManager Instance { get; private set; }

        public event Action<int> OnMoneyChanged;

        public int Money { get; private set; }

        public void Init(int startingMoney)
        {
            Instance = this;
            Money = startingMoney;
        }

        public void Add(int amount)
        {
            if (amount <= 0) return;
            Money += amount;
            OnMoneyChanged?.Invoke(Money);
        }

        public bool TrySpend(int amount)
        {
            if (amount < 0 || Money < amount) return false;
            Money -= amount;
            OnMoneyChanged?.Invoke(Money);
            return true;
        }
    }
}
