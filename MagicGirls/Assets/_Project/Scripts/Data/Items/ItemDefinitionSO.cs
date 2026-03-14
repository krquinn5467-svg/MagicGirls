using System.Collections.Generic;
using UnityEngine;
using MagicGirls.Core.Enums;

namespace MagicGirls.Data
{
    public enum ItemType
    {
        Weapon,
        Armor,
        Accessory,
        Consumable
    }

    public enum EquipSlot
    {
        Weapon,
        Armor,
        Accessory
    }

    [System.Serializable]
    public class ItemStatBonus
    {
        public StatTargetType targetType;
        public BaseStat baseStat;
        public MagicalStat magicalStat;
        public int bonus;
    }

    [CreateAssetMenu(fileName = "NewItem", menuName = "MagicGirls/Item")]
    public class ItemDefinitionSO : ScriptableObject
    {
        [Header("Item Info")]
        public string itemName = "New Item";
        [TextArea(2, 3)]
        public string description = "";
        public ItemType itemType;

        [Header("Equipment")]
        public EquipSlot equipSlot;
        public List<ItemStatBonus> statBonuses = new();

        [Header("Combat Bonuses")]
        public int hpBonus;
        public int armorClassBonus;
        public int attackBonus;

        [Header("Consumable")]
        public bool isConsumable;
        public int healAmount;
        public int staminaRestore;

        [Header("Economy")]
        public int buyPrice;
        public int sellPrice;
    }
}
