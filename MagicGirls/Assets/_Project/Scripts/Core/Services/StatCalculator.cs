using System;
using MagicGirls.Core.Enums;
using MagicGirls.Core.Models;

namespace MagicGirls.Core.Services
{
    public static class StatCalculator
    {
        /// <summary>
        /// DnD-style ability modifier: (stat - 10) / 2, rounded down.
        /// </summary>
        public static int GetModifier(int statValue)
        {
            return (int)Math.Floor((statValue - 10) / 2.0);
        }

        public static int GetBaseStatModifier(CharacterState character, BaseStat stat)
        {
            return GetModifier(character.GetBaseStat(stat));
        }

        public static int GetMagicalStatModifier(CharacterState character, MagicalStat stat)
        {
            return GetModifier(character.GetMagicalStat(stat));
        }

        public static int CalculateMaxHP(CharacterState character)
        {
            int conMod = GetBaseStatModifier(character, BaseStat.CON);
            return 10 + (character.Level * (6 + conMod));
        }

        public static int CalculateMaxStamina(CharacterState character)
        {
            int conMod = GetBaseStatModifier(character, BaseStat.CON);
            int wisMod = GetBaseStatModifier(character, BaseStat.WIS);
            return 8 + conMod + wisMod + character.Level;
        }

        public static int CalculateArmorClass(CharacterState character)
        {
            return 10 + GetBaseStatModifier(character, BaseStat.DEX);
        }

        public static int CalculateInitiative(CharacterState character)
        {
            return GetBaseStatModifier(character, BaseStat.DEX);
        }

        /// <summary>
        /// Personality traits give a multiplier to stat gains from activities.
        /// Returns a multiplier (1.0 = normal, 1.5 = bonus, 0.5 = penalty).
        /// </summary>
        public static float GetPersonalityMultiplier(CharacterState character, ActivityType activity)
        {
            float multiplier = 1.0f;

            foreach (var trait in character.Traits)
            {
                multiplier *= GetTraitActivityMultiplier(trait, activity);
            }

            return multiplier;
        }

        private static float GetTraitActivityMultiplier(PersonalityTrait trait, ActivityType activity)
        {
            return (trait, activity) switch
            {
                (PersonalityTrait.Brave, ActivityType.DungeonCrawl) => 1.3f,
                (PersonalityTrait.Brave, ActivityType.Train) => 1.2f,
                (PersonalityTrait.Timid, ActivityType.DungeonCrawl) => 0.7f,
                (PersonalityTrait.Timid, ActivityType.School) => 1.2f,
                (PersonalityTrait.Studious, ActivityType.School) => 1.5f,
                (PersonalityTrait.Studious, ActivityType.Train) => 0.8f,
                (PersonalityTrait.Reckless, ActivityType.DungeonCrawl) => 1.4f,
                (PersonalityTrait.Reckless, ActivityType.Rest) => 0.7f,
                (PersonalityTrait.Kind, ActivityType.Recreation) => 1.3f,
                (PersonalityTrait.Kind, ActivityType.Work) => 1.2f,
                (PersonalityTrait.Aloof, ActivityType.School) => 1.2f,
                (PersonalityTrait.Aloof, ActivityType.Recreation) => 0.7f,
                (PersonalityTrait.Cheerful, ActivityType.Recreation) => 1.4f,
                (PersonalityTrait.Cheerful, ActivityType.Work) => 1.1f,
                (PersonalityTrait.Serious, ActivityType.Train) => 1.3f,
                (PersonalityTrait.Serious, ActivityType.Recreation) => 0.8f,
                (PersonalityTrait.Lazy, ActivityType.Rest) => 1.5f,
                (PersonalityTrait.Lazy, ActivityType.Train) => 0.6f,
                (PersonalityTrait.Lazy, ActivityType.Work) => 0.6f,
                (PersonalityTrait.Diligent, ActivityType.Train) => 1.3f,
                (PersonalityTrait.Diligent, ActivityType.Work) => 1.3f,
                (PersonalityTrait.Diligent, ActivityType.School) => 1.2f,
                _ => 1.0f
            };
        }
    }
}
