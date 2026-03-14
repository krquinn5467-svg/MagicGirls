using System;
using System.Collections.Generic;
using System.Linq;
using MagicGirls.Core.Enums;
using MagicGirls.Core.Models;
using MagicGirls.Data;

namespace MagicGirls.Core.Systems
{
    public class InventoryItem
    {
        public ItemDefinitionSO Definition { get; set; }
        public int Quantity { get; set; }
    }

    public class InventorySystem
    {
        private readonly List<InventoryItem> _items = new();

        public IReadOnlyList<InventoryItem> Items => _items;
        public event Action OnInventoryChanged;

        public void AddItem(ItemDefinitionSO item, int quantity = 1)
        {
            var existing = _items.FirstOrDefault(i => i.Definition == item);
            if (existing != null)
            {
                existing.Quantity += quantity;
            }
            else
            {
                _items.Add(new InventoryItem { Definition = item, Quantity = quantity });
            }
            OnInventoryChanged?.Invoke();
        }

        public bool RemoveItem(ItemDefinitionSO item, int quantity = 1)
        {
            var existing = _items.FirstOrDefault(i => i.Definition == item);
            if (existing == null || existing.Quantity < quantity) return false;

            existing.Quantity -= quantity;
            if (existing.Quantity <= 0)
                _items.Remove(existing);

            OnInventoryChanged?.Invoke();
            return true;
        }

        public bool HasItem(ItemDefinitionSO item, int quantity = 1)
        {
            var existing = _items.FirstOrDefault(i => i.Definition == item);
            return existing != null && existing.Quantity >= quantity;
        }

        public bool Equip(CharacterState character, ItemDefinitionSO item)
        {
            if (item.isConsumable) return false;
            if (!HasItem(item)) return false;

            var slot = item.equipSlot;

            // Unequip current item in that slot
            if (character.Equipment.TryGetValue(slot, out var current) && current != null)
                Unequip(character, slot);

            RemoveItem(item);
            character.Equipment[slot] = item;
            ApplyEquipBonuses(character, item, 1);

            OnInventoryChanged?.Invoke();
            return true;
        }

        public bool Unequip(CharacterState character, EquipSlot slot)
        {
            if (!character.Equipment.TryGetValue(slot, out var item) || item == null)
                return false;

            ApplyEquipBonuses(character, item, -1);
            character.Equipment[slot] = null;
            AddItem(item);

            OnInventoryChanged?.Invoke();
            return true;
        }

        public bool UseConsumable(CharacterState character, ItemDefinitionSO item)
        {
            if (!item.isConsumable) return false;
            if (!HasItem(item)) return false;

            if (item.healAmount > 0)
                character.CurrentHP = Math.Min(character.MaxHP, character.CurrentHP + item.healAmount);

            if (item.staminaRestore > 0)
                character.CurrentStamina = Math.Min(character.MaxStamina, character.CurrentStamina + item.staminaRestore);

            RemoveItem(item);
            return true;
        }

        public List<LootResult> RollLoot(LootTableSO table, System.Random rng)
        {
            var results = new List<LootResult>();

            foreach (var entry in table.entries)
            {
                if (rng.NextDouble() <= entry.dropChance)
                {
                    int qty = rng.Next(entry.minQuantity, entry.maxQuantity + 1);
                    AddItem(entry.item, qty);
                    results.Add(new LootResult { Item = entry.item, Quantity = qty });
                }
            }

            return results;
        }

        private void ApplyEquipBonuses(CharacterState character, ItemDefinitionSO item, int multiplier)
        {
            foreach (var bonus in item.statBonuses)
            {
                int amount = bonus.bonus * multiplier;
                if (bonus.targetType == StatTargetType.Base)
                    character.ModifyBaseStat(bonus.baseStat, amount);
                else
                    character.ModifyMagicalStat(bonus.magicalStat, amount);
            }
        }
    }

    public class LootResult
    {
        public ItemDefinitionSO Item { get; set; }
        public int Quantity { get; set; }
    }
}
