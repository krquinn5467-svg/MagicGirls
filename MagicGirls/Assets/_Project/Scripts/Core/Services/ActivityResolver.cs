using System;
using System.Collections.Generic;
using MagicGirls.Core.Enums;
using MagicGirls.Core.Models;
using MagicGirls.Data;

namespace MagicGirls.Core.Services
{
    public class ActivityResolver
    {
        private readonly DiceService _dice;

        public ActivityResolver(DiceService dice)
        {
            _dice = dice;
        }

        public ActivityResult Resolve(CharacterState character, ActivityDefinitionSO activity)
        {
            var result = new ActivityResult
            {
                Activity = activity.activityType
            };

            float personalityMult = StatCalculator.GetPersonalityMultiplier(character, activity.activityType);

            // Roll for each stat modifier defined on the activity
            foreach (var mod in activity.statModifiers)
            {
                var roll = _dice.Roll(mod.dieType, mod.diceCount, mod.flatModifier);
                result.AllRolls.Add(roll);

                int rawGain = Math.Max(0, roll.Total);
                int scaledGain = (int)Math.Round(rawGain * personalityMult);

                // Ensure at least 1 point gain if roll was positive
                if (rawGain > 0 && scaledGain == 0) scaledGain = 1;

                var statChange = new StatChange
                {
                    TargetType = mod.targetType,
                    BaseStat = mod.baseStat,
                    MagicalStat = mod.magicalStat,
                    Amount = scaledGain,
                    Roll = roll
                };

                result.StatChanges.Add(statChange);
            }

            // Apply costs and rewards
            result.StaminaChange = -activity.staminaCost;
            result.MoraleChange = activity.moraleChange;
            result.GoldChange = activity.goldGain;
            result.ExperienceGain = activity.experienceGain;

            // Special handling for specific activities
            switch (activity.activityType)
            {
                case ActivityType.Rest:
                    ResolveRest(character, result);
                    break;
                case ActivityType.DungeonCrawl:
                    ResolveDungeonCrawl(character, activity, result);
                    break;
                case ActivityType.Work:
                    ResolveWork(character, result);
                    break;
            }

            result.NarrativeText = GenerateNarrative(character, activity, result);

            return result;
        }

        private void ResolveRest(CharacterState character, ActivityResult result)
        {
            // Rest recovers stamina based on CON
            int conMod = StatCalculator.GetBaseStatModifier(character, BaseStat.CON);
            var recoveryRoll = _dice.Roll(DieType.D6, 1, conMod);
            result.AllRolls.Add(recoveryRoll);
            result.StaminaChange = Math.Max(1, recoveryRoll.Total);

            // Also recover some morale
            result.MoraleChange = 2;
        }

        private void ResolveDungeonCrawl(CharacterState character, ActivityDefinitionSO activity, ActivityResult result)
        {
            // Injury check: CON save vs DC
            int conMod = StatCalculator.GetBaseStatModifier(character, BaseStat.CON);
            var saveRoll = _dice.Roll(DieType.D20, 1, conMod);
            result.AllRolls.Add(saveRoll);

            if (!_dice.CheckDC(saveRoll, activity.injuryDC))
            {
                result.InjuryOccurred = true;
                // Lose some HP on failed save
                var damageRoll = _dice.Roll(DieType.D6, 1);
                result.AllRolls.Add(damageRoll);
                result.StaminaChange -= damageRoll.Total;
            }

            // Bonus XP from dungeon crawling
            result.ExperienceGain += 25f;
        }

        private void ResolveWork(CharacterState character, ActivityResult result)
        {
            // Gold from work scales with CHA
            int chaMod = StatCalculator.GetBaseStatModifier(character, BaseStat.CHA);
            var goldRoll = _dice.Roll(DieType.D6, 1, chaMod);
            result.AllRolls.Add(goldRoll);
            result.GoldChange += Math.Max(1, goldRoll.Total) * 5;
        }

        private string GenerateNarrative(CharacterState character, ActivityDefinitionSO activity, ActivityResult result)
        {
            string name = character.Name;

            return activity.activityType switch
            {
                ActivityType.Rest =>
                    $"{name} rested and recovered {result.StaminaChange} stamina.",
                ActivityType.Train =>
                    $"{name} trained hard! {FormatStatGains(result)}",
                ActivityType.School =>
                    $"{name} studied diligently. {FormatStatGains(result)}",
                ActivityType.DungeonCrawl =>
                    result.InjuryOccurred
                        ? $"{name} explored the dungeon but got hurt! {FormatStatGains(result)}"
                        : $"{name} cleared the dungeon successfully! {FormatStatGains(result)}",
                ActivityType.Recreation =>
                    $"{name} had fun and relaxed. {FormatStatGains(result)}",
                ActivityType.Work =>
                    $"{name} worked and earned {result.GoldChange} gold. {FormatStatGains(result)}",
                _ => $"{name} completed {activity.displayName}."
            };
        }

        private string FormatStatGains(ActivityResult result)
        {
            if (result.StatChanges.Count == 0) return "";

            var parts = new List<string>();
            foreach (var change in result.StatChanges)
            {
                if (change.Amount != 0)
                    parts.Add($"{change.StatName} +{change.Amount}");
            }

            return parts.Count > 0 ? string.Join(", ", parts) : "No stat changes.";
        }
    }
}
