using System;
using System.Collections.Generic;
using System.Linq;
using MagicGirls.Core.Enums;
using MagicGirls.Core.Models;
using MagicGirls.Data;

namespace MagicGirls.Core.Services
{
    public class BattleResolver
    {
        private readonly DiceService _dice;

        public BattleResolver(DiceService dice)
        {
            _dice = dice;
        }

        /// <summary>
        /// Initialize a battle from player team and encounter data.
        /// </summary>
        public BattleState InitBattle(List<CharacterState> team, MonsterEncounterSO encounter, float difficultyScale = 1f)
        {
            var state = new BattleState();

            foreach (var character in team)
            {
                if (character.CurrentHP > 0)
                    state.PlayerTeam.Add(BattleFighter.FromCharacter(character));
            }

            foreach (var monster in encounter.monsters)
            {
                state.EnemyTeam.Add(BattleFighter.FromMonster(monster, difficultyScale));
            }

            // Calculate total rewards
            state.TotalExperienceReward = encounter.monsters.Sum(m => m.experienceReward);
            state.TotalGoldReward = encounter.monsters.Sum(m => m.goldReward);

            // Roll initiative for everyone
            RollInitiative(state);

            state.Log.Add(new BattleLogEntry
            {
                Round = 0,
                NarrativeText = $"Battle begins! {encounter.encounterName}!"
            });

            return state;
        }

        /// <summary>
        /// Execute one full round of combat. Returns the updated state.
        /// </summary>
        public BattleState ExecuteRound(BattleState state)
        {
            if (state.Result != BattleResult.InProgress)
                return state;

            state.CurrentRound++;

            // Get turn order by initiative (highest first)
            var turnOrder = state.AllFighters
                .Where(f => f.IsAlive)
                .OrderByDescending(f => f.Initiative)
                .ToList();

            state.Log.Add(new BattleLogEntry
            {
                Round = state.CurrentRound,
                NarrativeText = $"--- Round {state.CurrentRound} ---"
            });

            foreach (var fighter in turnOrder)
            {
                if (!fighter.IsAlive) continue;
                if (IsBattleOver(state)) break;

                var action = SelectAction(fighter, state);
                var target = SelectTarget(fighter, state);

                if (target == null) continue;

                var logEntry = ResolveAction(fighter, target, action, state.CurrentRound);
                state.Log.Add(logEntry);

                // Check if target defeated
                if (!target.IsAlive)
                {
                    state.Log.Add(new BattleLogEntry
                    {
                        Round = state.CurrentRound,
                        ActorName = target.Name,
                        NarrativeText = $"{target.Name} has been defeated!"
                    });
                }
            }

            // Check battle end
            CheckBattleEnd(state);

            return state;
        }

        public bool IsBattleOver(BattleState state)
        {
            return state.Result != BattleResult.InProgress;
        }

        private void RollInitiative(BattleState state)
        {
            foreach (var fighter in state.AllFighters)
            {
                int dexMod = StatCalculator.GetModifier(fighter.Dexterity);
                var roll = _dice.Roll(DieType.D20, 1, dexMod);
                fighter.Initiative = roll.Total;
            }
        }

        private BattleActionType SelectAction(BattleFighter fighter, BattleState state)
        {
            if (fighter.Type == FighterType.Enemy)
                return BattleActionType.Attack;

            // Player characters choose based on personality
            if (fighter.Traits.Contains(PersonalityTrait.Brave) || fighter.Traits.Contains(PersonalityTrait.Reckless))
                return BattleActionType.Attack;

            if (fighter.Traits.Contains(PersonalityTrait.Studious) || fighter.Traits.Contains(PersonalityTrait.Aloof))
            {
                // Magic users prefer spells if they have good magic power
                if (fighter.MagicPower >= 14)
                    return BattleActionType.Spell;
            }

            if (fighter.Traits.Contains(PersonalityTrait.Timid))
            {
                // Timid characters defend if HP is low
                float hpPct = (float)fighter.CurrentHP / fighter.MaxHP;
                if (hpPct < 0.3f)
                    return BattleActionType.Defend;
            }

            // Default to attack
            return BattleActionType.Attack;
        }

        private BattleFighter SelectTarget(BattleFighter fighter, BattleState state)
        {
            var enemies = fighter.Type == FighterType.Player
                ? state.EnemyTeam.Where(e => e.IsAlive).ToList()
                : state.PlayerTeam.Where(p => p.IsAlive).ToList();

            if (enemies.Count == 0) return null;

            // Simple targeting: attack the one with lowest HP (focus fire)
            if (fighter.Traits.Contains(PersonalityTrait.Serious) || fighter.Traits.Contains(PersonalityTrait.Studious))
                return enemies.OrderBy(e => e.CurrentHP).First();

            // Reckless targets highest HP enemy
            if (fighter.Traits.Contains(PersonalityTrait.Reckless))
                return enemies.OrderByDescending(e => e.CurrentHP).First();

            // Default: random target
            return enemies[new Random().Next(enemies.Count)];
        }

        private BattleLogEntry ResolveAction(BattleFighter actor, BattleFighter target, BattleActionType action, int round)
        {
            return action switch
            {
                BattleActionType.Attack => ResolveAttack(actor, target, round),
                BattleActionType.Spell => ResolveSpell(actor, target, round),
                BattleActionType.Defend => ResolveDefend(actor, round),
                _ => ResolveAttack(actor, target, round)
            };
        }

        private BattleLogEntry ResolveAttack(BattleFighter attacker, BattleFighter defender, int round)
        {
            int strMod = StatCalculator.GetModifier(attacker.Strength);
            var attackRoll = _dice.Roll(DieType.D20, 1, strMod + attacker.AttackBonus);

            var entry = new BattleLogEntry
            {
                Round = round,
                ActorName = attacker.Name,
                ActorType = attacker.Type,
                ActionType = BattleActionType.Attack,
                TargetName = defender.Name,
                AttackRoll = attackRoll,
                IsCritical = attackRoll.IsCritical
            };

            bool hit = attackRoll.Total >= defender.ArmorClass || attackRoll.IsCritical;
            bool fumble = attackRoll.IsFumble;

            entry.Hit = hit && !fumble;

            if (fumble)
            {
                entry.DamageDealt = 0;
                entry.NarrativeText = $"{attacker.Name} attacks {defender.Name} but fumbles! ({attackRoll})";
            }
            else if (hit)
            {
                int diceCount = attackRoll.IsCritical ? attacker.AttackDiceCount * 2 : attacker.AttackDiceCount;
                var damageRoll = _dice.Roll(attacker.AttackDie, diceCount, strMod);
                int damage = Math.Max(1, damageRoll.Total);

                // Apply weakness/resistance
                damage = ApplyDamageModifiers(damage, attacker.AttackType, defender);

                defender.CurrentHP = Math.Max(0, defender.CurrentHP - damage);

                entry.DamageRoll = damageRoll;
                entry.DamageDealt = damage;
                entry.TargetDefeated = !defender.IsAlive;

                string critText = attackRoll.IsCritical ? " CRITICAL HIT!" : "";
                entry.NarrativeText = $"{attacker.Name} attacks {defender.Name}!{critText} ({attackRoll}) for {damage} damage!";

                if (!defender.IsAlive)
                    entry.NarrativeText += $" {defender.Name} falls!";
            }
            else
            {
                entry.DamageDealt = 0;
                entry.NarrativeText = $"{attacker.Name} attacks {defender.Name} but misses! ({attackRoll} vs AC {defender.ArmorClass})";
            }

            return entry;
        }

        private BattleLogEntry ResolveSpell(BattleFighter caster, BattleFighter target, int round)
        {
            int magicMod = StatCalculator.GetModifier(caster.MagicPower);
            int spellDC = 10 + StatCalculator.GetModifier(caster.MagicAffinity);
            var attackRoll = _dice.Roll(DieType.D20, 1, magicMod);

            var entry = new BattleLogEntry
            {
                Round = round,
                ActorName = caster.Name,
                ActorType = caster.Type,
                ActionType = BattleActionType.Spell,
                TargetName = target.Name,
                AttackRoll = attackRoll,
                IsCritical = attackRoll.IsCritical
            };

            bool hit = attackRoll.Total >= target.ArmorClass || attackRoll.IsCritical;
            entry.Hit = hit && !attackRoll.IsFumble;

            if (attackRoll.IsFumble)
            {
                entry.DamageDealt = 0;
                entry.NarrativeText = $"{caster.Name} casts a spell at {target.Name} but it fizzles! ({attackRoll})";
            }
            else if (hit)
            {
                // Spells use d8 base, scaled by magic affinity
                int diceCount = attackRoll.IsCritical ? 2 : 1;
                var damageRoll = _dice.Roll(DieType.D8, diceCount, magicMod);
                int damage = Math.Max(1, damageRoll.Total);

                // Magic attacks use Holy damage type for weakness/resistance
                damage = ApplyDamageModifiers(damage, DamageType.Holy, target);

                target.CurrentHP = Math.Max(0, target.CurrentHP - damage);

                entry.DamageRoll = damageRoll;
                entry.DamageDealt = damage;
                entry.TargetDefeated = !target.IsAlive;

                string critText = attackRoll.IsCritical ? " SURGE!" : "";
                entry.NarrativeText = $"{caster.Name} casts a spell on {target.Name}!{critText} ({attackRoll}) for {damage} magical damage!";

                if (!target.IsAlive)
                    entry.NarrativeText += $" {target.Name} is vanquished!";
            }
            else
            {
                entry.DamageDealt = 0;
                entry.NarrativeText = $"{caster.Name} casts at {target.Name} but the spell is resisted! ({attackRoll} vs AC {target.ArmorClass})";
            }

            return entry;
        }

        private BattleLogEntry ResolveDefend(BattleFighter defender, int round)
        {
            // Defending heals a small amount
            var healRoll = _dice.Roll(DieType.D4, 1);
            int healAmount = Math.Max(1, healRoll.Total);
            defender.CurrentHP = Math.Min(defender.MaxHP, defender.CurrentHP + healAmount);

            return new BattleLogEntry
            {
                Round = round,
                ActorName = defender.Name,
                ActorType = defender.Type,
                ActionType = BattleActionType.Defend,
                DamageDealt = -healAmount,
                NarrativeText = $"{defender.Name} takes a defensive stance and recovers {healAmount} HP!"
            };
        }

        private int ApplyDamageModifiers(int baseDamage, DamageType damageType, BattleFighter target)
        {
            if (target.Weaknesses.Contains(damageType))
                return (int)(baseDamage * 1.5f);

            if (target.Resistances.Contains(damageType))
                return (int)(baseDamage * 0.5f);

            return baseDamage;
        }

        private void CheckBattleEnd(BattleState state)
        {
            bool allEnemiesDead = state.EnemyTeam.All(e => !e.IsAlive);
            bool allPlayersDead = state.PlayerTeam.All(p => !p.IsAlive);

            if (allEnemiesDead)
            {
                state.Result = BattleResult.Victory;
                state.Log.Add(new BattleLogEntry
                {
                    Round = state.CurrentRound,
                    NarrativeText = $"Victory! Earned {state.TotalExperienceReward} XP and {state.TotalGoldReward} gold!"
                });
            }
            else if (allPlayersDead)
            {
                state.Result = BattleResult.Defeat;
                state.Log.Add(new BattleLogEntry
                {
                    Round = state.CurrentRound,
                    NarrativeText = "Defeat... The magical girls have fallen."
                });
            }
        }
    }
}
