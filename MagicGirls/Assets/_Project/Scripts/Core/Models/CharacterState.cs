using System;
using System.Collections.Generic;
using MagicGirls.Core.Enums;
using MagicGirls.Data;

namespace MagicGirls.Core.Models
{
    [Serializable]
    public class CharacterState
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Level { get; set; } = 1;
        public float Experience { get; set; }
        public float ExperienceToNextLevel { get; set; } = 100f;

        public Dictionary<BaseStat, int> BaseStats { get; set; } = new();
        public Dictionary<MagicalStat, int> MagicalStats { get; set; } = new();

        public int CurrentHP { get; set; }
        public int MaxHP { get; set; }
        public int CurrentStamina { get; set; }
        public int MaxStamina { get; set; }

        public List<PersonalityTrait> Traits { get; set; } = new();

        // Equipment slots
        public Dictionary<EquipSlot, ItemDefinitionSO> Equipment { get; set; } = new()
        {
            { EquipSlot.Weapon, null },
            { EquipSlot.Armor, null },
            { EquipSlot.Accessory, null }
        };

        public static CharacterState CreateDefault(string name)
        {
            var state = new CharacterState
            {
                Id = Guid.NewGuid().ToString(),
                Name = name,
                Level = 1,
                Experience = 0,
                ExperienceToNextLevel = 100f,
                MaxHP = 20,
                MaxStamina = 10
            };

            foreach (BaseStat stat in Enum.GetValues(typeof(BaseStat)))
                state.BaseStats[stat] = 10;

            foreach (MagicalStat stat in Enum.GetValues(typeof(MagicalStat)))
                state.MagicalStats[stat] = 10;

            state.CurrentHP = state.MaxHP;
            state.CurrentStamina = state.MaxStamina;

            return state;
        }

        public int GetBaseStat(BaseStat stat) =>
            BaseStats.TryGetValue(stat, out int val) ? val : 10;

        public int GetMagicalStat(MagicalStat stat) =>
            MagicalStats.TryGetValue(stat, out int val) ? val : 10;

        public void ModifyBaseStat(BaseStat stat, int amount)
        {
            if (!BaseStats.ContainsKey(stat)) BaseStats[stat] = 10;
            BaseStats[stat] = Math.Max(1, BaseStats[stat] + amount);
        }

        public void ModifyMagicalStat(MagicalStat stat, int amount)
        {
            if (!MagicalStats.ContainsKey(stat)) MagicalStats[stat] = 10;
            MagicalStats[stat] = Math.Max(0, MagicalStats[stat] + amount);
        }
    }
}
