using System.Collections.Generic;
using System.Linq;
using MagicGirls.Core.Enums;
using MagicGirls.Core.Models;
using MagicGirls.Core.Systems;
using MagicGirls.Data;

namespace MagicGirls.Core.Services
{
    public class MilestoneResult
    {
        public MilestoneSO Milestone { get; set; }
        public bool Success { get; set; }
        public List<string> ConditionResults { get; set; } = new();
        public int GoldReward { get; set; }
        public int ExperienceReward { get; set; }
        public int MoralePenalty { get; set; }
    }

    public class MilestoneEvaluator
    {
        private readonly TeamSystem _team;
        private readonly EconomySystem _economy;

        // Track activity completions for milestone conditions
        private readonly Dictionary<ActivityType, int> _activityCounts = new();
        private bool _bossDefeatedThisMonth;
        private float _lastBattleMinHPPercent = 100f;

        public MilestoneEvaluator(TeamSystem team, EconomySystem economy)
        {
            _team = team;
            _economy = economy;
        }

        public void RecordActivityCompletion(ActivityType activity)
        {
            if (!_activityCounts.ContainsKey(activity))
                _activityCounts[activity] = 0;
            _activityCounts[activity]++;
        }

        public void RecordBattleResult(BattleState battle)
        {
            if (battle.Result == BattleResult.Victory)
            {
                // Check if any enemy was a boss-tier (HP > 50)
                bool wasBoss = battle.EnemyTeam.Any(e => e.MaxHP >= 50);
                if (wasBoss)
                    _bossDefeatedThisMonth = true;

                // Track min HP percent across team after battle
                float minHP = battle.PlayerTeam
                    .Where(f => f.IsAlive)
                    .Select(f => (float)f.CurrentHP / f.MaxHP * 100f)
                    .DefaultIfEmpty(0f)
                    .Min();
                _lastBattleMinHPPercent = minHP;
            }
        }

        public MilestoneResult Evaluate(MilestoneSO milestone)
        {
            var result = new MilestoneResult { Milestone = milestone };
            bool allMet = true;

            foreach (var condition in milestone.conditions)
            {
                bool met = EvaluateCondition(condition);
                string desc = DescribeCondition(condition, met);
                result.ConditionResults.Add(desc);
                if (!met) allMet = false;
            }

            result.Success = allMet;

            if (allMet)
            {
                result.GoldReward = milestone.goldReward;
                result.ExperienceReward = milestone.experienceReward;
            }
            else
            {
                result.MoralePenalty = milestone.moralePenalty;
            }

            return result;
        }

        public void ApplyResult(MilestoneResult result)
        {
            if (result.Success)
            {
                if (result.GoldReward > 0)
                    _economy.AddGold(result.GoldReward);

                if (result.ExperienceReward > 0)
                {
                    float xpPerChar = result.ExperienceReward / (float)_team.ActiveTeam.Count;
                    foreach (var character in _team.ActiveTeam)
                        character.Experience += xpPerChar;
                }
            }
            else
            {
                if (result.MoralePenalty > 0)
                {
                    foreach (var character in _team.ActiveTeam)
                        character.ModifyMagicalStat(MagicalStat.Morale, -result.MoralePenalty);
                }
            }
        }

        public void ResetMonth()
        {
            _activityCounts.Clear();
            _bossDefeatedThisMonth = false;
            _lastBattleMinHPPercent = 100f;
        }

        private bool EvaluateCondition(MilestoneCondition condition)
        {
            switch (condition.conditionType)
            {
                case MilestoneConditionType.DefeatBoss:
                    return _bossDefeatedThisMonth;

                case MilestoneConditionType.ReachStatValue:
                    return _team.ActiveTeam.Any(c =>
                        c.GetBaseStat(condition.targetBaseStat) >= condition.threshold);

                case MilestoneConditionType.ReachLevel:
                    return _team.ActiveTeam.Any(c => c.Level >= condition.threshold);

                case MilestoneConditionType.EarnGold:
                    return _economy.Gold >= condition.threshold;

                case MilestoneConditionType.WinBattleWithMinHP:
                    return _bossDefeatedThisMonth && _lastBattleMinHPPercent >= condition.minHPPercent;

                case MilestoneConditionType.CompleteActivities:
                    _activityCounts.TryGetValue(condition.targetActivity, out int count);
                    return count >= condition.activityCount;

                default:
                    return false;
            }
        }

        private string DescribeCondition(MilestoneCondition condition, bool met)
        {
            string status = met ? "[DONE]" : "[FAIL]";
            switch (condition.conditionType)
            {
                case MilestoneConditionType.DefeatBoss:
                    return $"{status} Defeat the monthly boss";
                case MilestoneConditionType.ReachStatValue:
                    return $"{status} Reach {condition.threshold} {condition.targetBaseStat} on any character";
                case MilestoneConditionType.ReachLevel:
                    return $"{status} Reach level {condition.threshold}";
                case MilestoneConditionType.EarnGold:
                    return $"{status} Have {condition.threshold} gold";
                case MilestoneConditionType.WinBattleWithMinHP:
                    return $"{status} Win boss with all allies above {condition.minHPPercent}% HP";
                case MilestoneConditionType.CompleteActivities:
                    _activityCounts.TryGetValue(condition.targetActivity, out int count);
                    return $"{status} Complete {condition.targetActivity} {count}/{condition.activityCount} times";
                default:
                    return $"{status} Unknown condition";
            }
        }
    }
}
