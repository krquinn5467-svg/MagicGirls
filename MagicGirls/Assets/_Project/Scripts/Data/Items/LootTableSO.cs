using System.Collections.Generic;
using UnityEngine;

namespace MagicGirls.Data
{
    [System.Serializable]
    public class LootEntry
    {
        public ItemDefinitionSO item;
        [Range(0f, 1f)]
        public float dropChance = 0.5f;
        public int minQuantity = 1;
        public int maxQuantity = 1;
    }

    [CreateAssetMenu(fileName = "NewLootTable", menuName = "MagicGirls/Loot Table")]
    public class LootTableSO : ScriptableObject
    {
        public string tableName = "Loot Table";
        public List<LootEntry> entries = new();

        [Header("Guaranteed Gold")]
        public int minGold;
        public int maxGold;
    }
}
