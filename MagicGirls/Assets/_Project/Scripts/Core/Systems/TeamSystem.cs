using System;
using System.Collections.Generic;
using System.Linq;
using MagicGirls.Core.Enums;
using MagicGirls.Core.Models;
using MagicGirls.Core.Services;
using MagicGirls.Data;

namespace MagicGirls.Core.Systems
{
    public class TeamSystem
    {
        private readonly List<CharacterState> _team = new();

        public IReadOnlyList<CharacterState> ActiveTeam => _team;
        public event Action<CharacterState> OnCharacterAdded;
        public event Action<CharacterState> OnCharacterUpdated;

        public CharacterState AddCharacter(CharacterTemplateSO template)
        {
            var character = template.CreateCharacterState();
            _team.Add(character);
            OnCharacterAdded?.Invoke(character);
            return character;
        }

        public CharacterState GetCharacter(string id)
        {
            return _team.FirstOrDefault(c => c.Id == id);
        }

        public void ApplyActivityResult(string characterId, ActivityResult result)
        {
            var character = GetCharacter(characterId);
            if (character == null) return;

            // Apply stat changes
            foreach (var change in result.StatChanges)
            {
                if (change.TargetType == StatTargetType.Base)
                    character.ModifyBaseStat(change.BaseStat, change.Amount);
                else
                    character.ModifyMagicalStat(change.MagicalStat, change.Amount);
            }

            // Apply stamina change
            character.CurrentStamina = Math.Clamp(
                character.CurrentStamina + result.StaminaChange,
                0, character.MaxStamina);

            // Apply morale change
            character.ModifyMagicalStat(MagicalStat.Morale, result.MoraleChange);

            // Apply experience
            character.Experience += result.ExperienceGain;
            CheckLevelUp(character);

            // Apply injury (HP loss from dungeon)
            if (result.InjuryOccurred)
            {
                character.CurrentHP = Math.Max(1, character.CurrentHP - 5);
            }

            // Recalculate derived stats
            character.MaxHP = StatCalculator.CalculateMaxHP(character);
            character.MaxStamina = StatCalculator.CalculateMaxStamina(character);

            OnCharacterUpdated?.Invoke(character);
        }

        private void CheckLevelUp(CharacterState character)
        {
            while (character.Experience >= character.ExperienceToNextLevel)
            {
                character.Experience -= character.ExperienceToNextLevel;
                character.Level++;
                character.ExperienceToNextLevel = 100f * (float)Math.Pow(1.5, character.Level - 1);

                // Heal on level up
                character.MaxHP = StatCalculator.CalculateMaxHP(character);
                character.MaxStamina = StatCalculator.CalculateMaxStamina(character);
                character.CurrentHP = character.MaxHP;
                character.CurrentStamina = character.MaxStamina;
            }
        }
    }
}
