using System;
using System.Collections.Generic;
using MagicGirls.Core.Enums;
using MagicGirls.Data;

namespace MagicGirls.Core.Models
{
    public enum BattleResult
    {
        InProgress,
        Victory,
        Defeat
    }

    public enum FighterType
    {
        Player,
        Enemy
    }

    public class BattleState
    {
        public List<BattleFighter> PlayerTeam { get; set; } = new();
        public List<BattleFighter> EnemyTeam { get; set; } = new();
        public List<BattleLogEntry> Log { get; set; } = new();
        public int CurrentRound { get; set; } = 0;
        public BattleResult Result { get; set; } = BattleResult.InProgress;
        public int TotalExperienceReward { get; set; }
        public int TotalGoldReward { get; set; }

        public List<BattleFighter> AllFighters
        {
            get
            {
                var all = new List<BattleFighter>(PlayerTeam);
                all.AddRange(EnemyTeam);
                return all;
            }
        }
    }

    public class BattleFighter
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public FighterType Type { get; set; }
        public int CurrentHP { get; set; }
        public int MaxHP { get; set; }
        public int ArmorClass { get; set; }
        public int Initiative { get; set; }
        public bool IsAlive => CurrentHP > 0;

        // Stats for combat
        public int Strength { get; set; }
        public int Dexterity { get; set; }
        public int Constitution { get; set; }
        public int MagicPower { get; set; }
        public int MagicAffinity { get; set; }

        // Combat config
        public DamageType AttackType { get; set; } = DamageType.Physical;
        public DieType AttackDie { get; set; } = DieType.D6;
        public int AttackDiceCount { get; set; } = 1;
        public int AttackBonus { get; set; }

        public List<DamageType> Weaknesses { get; set; } = new();
        public List<DamageType> Resistances { get; set; } = new();

        // Personality (player chars only)
        public List<PersonalityTrait> Traits { get; set; } = new();

        // Back-reference for applying results
        public string CharacterId { get; set; }

        public static BattleFighter FromCharacter(CharacterState character)
        {
            return new BattleFighter
            {
                Id = Guid.NewGuid().ToString(),
                CharacterId = character.Id,
                Name = character.Name,
                Type = FighterType.Player,
                CurrentHP = character.CurrentHP,
                MaxHP = character.MaxHP,
                ArmorClass = 10 + Services.StatCalculator.GetModifier(character.GetBaseStat(BaseStat.DEX)),
                Strength = character.GetBaseStat(BaseStat.STR),
                Dexterity = character.GetBaseStat(BaseStat.DEX),
                Constitution = character.GetBaseStat(BaseStat.CON),
                MagicPower = character.GetMagicalStat(MagicalStat.MagicPower),
                MagicAffinity = character.GetMagicalStat(MagicalStat.MagicAffinity),
                AttackType = DamageType.Physical,
                AttackDie = DieType.D8,
                AttackDiceCount = 1,
                AttackBonus = 0,
                Traits = new List<PersonalityTrait>(character.Traits)
            };
        }

        public static BattleFighter FromMonster(MonsterTemplateSO monster, float difficultyScale = 1f)
        {
            int scaledHP = (int)(monster.baseHP * difficultyScale);
            return new BattleFighter
            {
                Id = Guid.NewGuid().ToString(),
                Name = monster.monsterName,
                Type = FighterType.Enemy,
                CurrentHP = scaledHP,
                MaxHP = scaledHP,
                ArmorClass = monster.armorClass,
                Strength = (int)(monster.strength * difficultyScale),
                Dexterity = (int)(monster.dexterity * difficultyScale),
                Constitution = (int)(monster.constitution * difficultyScale),
                MagicPower = (int)(monster.magicPower * difficultyScale),
                AttackType = monster.attackType,
                AttackDie = monster.attackDie,
                AttackDiceCount = monster.attackDiceCount,
                AttackBonus = monster.attackBonus,
                Weaknesses = new List<DamageType>(monster.weaknesses),
                Resistances = new List<DamageType>(monster.resistances)
            };
        }
    }

    public class BattleLogEntry
    {
        public int Round { get; set; }
        public string ActorName { get; set; }
        public FighterType ActorType { get; set; }
        public BattleActionType ActionType { get; set; }
        public string TargetName { get; set; }
        public DiceRollResult AttackRoll { get; set; }
        public DiceRollResult DamageRoll { get; set; }
        public int DamageDealt { get; set; }
        public bool Hit { get; set; }
        public bool IsCritical { get; set; }
        public bool TargetDefeated { get; set; }
        public string NarrativeText { get; set; }

        public override string ToString() => NarrativeText;
    }
}
