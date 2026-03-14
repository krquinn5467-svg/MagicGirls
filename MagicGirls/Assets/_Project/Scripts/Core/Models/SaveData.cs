using System;
using System.Collections.Generic;
using MagicGirls.Core.Enums;

namespace MagicGirls.Core.Models
{
    [Serializable]
    public class SaveData
    {
        public string Version = "1.0";
        public string SaveDate;

        // Time
        public int CurrentSegmentIndex;
        public int CurrentDay;
        public int CurrentWeek;
        public int CurrentMonth;
        public int TotalWeeksCompleted;

        // Economy
        public int Gold;

        // Characters
        public List<CharacterSaveData> Characters = new();

        // Inventory (item names + quantities, resolved at load via registry)
        public List<InventoryItemSaveData> InventoryItems = new();
    }

    [Serializable]
    public class CharacterSaveData
    {
        public string Id;
        public string Name;
        public int Level;
        public float Experience;
        public float ExperienceToNextLevel;

        public Dictionary<BaseStat, int> BaseStats = new();
        public Dictionary<MagicalStat, int> MagicalStats = new();

        public int CurrentHP;
        public int MaxHP;
        public int CurrentStamina;
        public int MaxStamina;

        public List<PersonalityTrait> Traits = new();

        // Equipment by slot name -> item name (null if empty)
        public string WeaponName;
        public string ArmorName;
        public string AccessoryName;

        public static CharacterSaveData FromCharacter(CharacterState character)
        {
            return new CharacterSaveData
            {
                Id = character.Id,
                Name = character.Name,
                Level = character.Level,
                Experience = character.Experience,
                ExperienceToNextLevel = character.ExperienceToNextLevel,
                BaseStats = new Dictionary<BaseStat, int>(character.BaseStats),
                MagicalStats = new Dictionary<MagicalStat, int>(character.MagicalStats),
                CurrentHP = character.CurrentHP,
                MaxHP = character.MaxHP,
                CurrentStamina = character.CurrentStamina,
                MaxStamina = character.MaxStamina,
                Traits = new List<PersonalityTrait>(character.Traits),
                WeaponName = character.Equipment.TryGetValue(Data.EquipSlot.Weapon, out var w) ? w?.itemName : null,
                ArmorName = character.Equipment.TryGetValue(Data.EquipSlot.Armor, out var a) ? a?.itemName : null,
                AccessoryName = character.Equipment.TryGetValue(Data.EquipSlot.Accessory, out var acc) ? acc?.itemName : null
            };
        }

        public void ApplyTo(CharacterState character)
        {
            character.Id = Id;
            character.Name = Name;
            character.Level = Level;
            character.Experience = Experience;
            character.ExperienceToNextLevel = ExperienceToNextLevel;
            character.BaseStats = new Dictionary<BaseStat, int>(BaseStats);
            character.MagicalStats = new Dictionary<MagicalStat, int>(MagicalStats);
            character.CurrentHP = CurrentHP;
            character.MaxHP = MaxHP;
            character.CurrentStamina = CurrentStamina;
            character.MaxStamina = MaxStamina;
            character.Traits = new List<PersonalityTrait>(Traits);
        }
    }

    [Serializable]
    public class InventoryItemSaveData
    {
        public string ItemName;
        public int Quantity;
    }
}
