using System;
using System.Collections.Generic;
using UnityEngine;
using MagicGirls.Core.Enums;
using MagicGirls.Core.Models;

namespace MagicGirls.Data
{
    [CreateAssetMenu(fileName = "NewCharacter", menuName = "MagicGirls/Character Template")]
    public class CharacterTemplateSO : ScriptableObject
    {
        [Header("Identity")]
        public string characterName = "New Magical Girl";
        [TextArea(2, 4)]
        public string backstory = "";
        public Sprite portrait;
        public Sprite battleSprite;

        [Header("Starting Base Stats (DnD)")]
        public int startingSTR = 10;
        public int startingDEX = 10;
        public int startingCON = 10;
        public int startingINT = 10;
        public int startingWIS = 10;
        public int startingCHA = 10;

        [Header("Starting Magical Stats")]
        public int startingMagicPower = 10;
        public int startingMagicAffinity = 10;
        public int startingCorruption = 0;
        public int startingMorale = 15;

        [Header("Vitals")]
        public int startingHP = 20;
        public int startingStamina = 10;

        [Header("Personality")]
        public List<PersonalityTrait> defaultTraits = new();

        public CharacterState CreateCharacterState()
        {
            var state = new CharacterState
            {
                Id = Guid.NewGuid().ToString(),
                Name = characterName,
                Level = 1,
                Experience = 0,
                ExperienceToNextLevel = 100f,
                MaxHP = startingHP,
                CurrentHP = startingHP,
                MaxStamina = startingStamina,
                CurrentStamina = startingStamina,
                Traits = new List<PersonalityTrait>(defaultTraits)
            };

            state.BaseStats[BaseStat.STR] = startingSTR;
            state.BaseStats[BaseStat.DEX] = startingDEX;
            state.BaseStats[BaseStat.CON] = startingCON;
            state.BaseStats[BaseStat.INT] = startingINT;
            state.BaseStats[BaseStat.WIS] = startingWIS;
            state.BaseStats[BaseStat.CHA] = startingCHA;

            state.MagicalStats[MagicalStat.MagicPower] = startingMagicPower;
            state.MagicalStats[MagicalStat.MagicAffinity] = startingMagicAffinity;
            state.MagicalStats[MagicalStat.Corruption] = startingCorruption;
            state.MagicalStats[MagicalStat.Morale] = startingMorale;

            return state;
        }
    }
}
