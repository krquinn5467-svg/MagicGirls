using System;

namespace MagicGirls.Core.Systems
{
    public class EconomySystem
    {
        public int Gold { get; private set; }
        public event Action<int> OnGoldChanged;

        public void Initialize(int startingGold)
        {
            Gold = startingGold;
            OnGoldChanged?.Invoke(Gold);
        }

        public void AddGold(int amount)
        {
            if (amount <= 0) return;
            Gold += amount;
            OnGoldChanged?.Invoke(Gold);
        }

        public bool SpendGold(int amount)
        {
            if (amount <= 0 || Gold < amount) return false;
            Gold -= amount;
            OnGoldChanged?.Invoke(Gold);
            return true;
        }
    }
}
