using System.Collections.Generic;
using UnityEngine;
using MagicGirls.Core.Enums;

namespace MagicGirls.Data
{
    public enum MilestoneConditionType
    {
        DefeatBoss,
        ReachStatValue,
        ReachLevel,
        EarnGold,
        WinBattleWithMinHP,
        CompleteActivities
    }

    [System.Serializable]
    public class MilestoneCondition
    {
        public MilestoneConditionType conditionType;

        [Tooltip("Target stat for ReachStatValue condition")]
        public BaseStat targetBaseStat;

        [Tooltip("Threshold value for stat/level/gold conditions")]
        public int threshold;

        [Tooltip("Min HP percentage for WinBattleWithMinHP (0-100)")]
        [Range(0, 100)]
        public int minHPPercent = 50;

        [Tooltip("Activity type for CompleteActivities condition")]
        public ActivityType targetActivity;

        [Tooltip("Number of times activity must be completed")]
        public int activityCount;
    }

    [CreateAssetMenu(fileName = "NewMilestone", menuName = "MagicGirls/Milestone")]
    public class MilestoneSO : ScriptableObject
    {
        [Header("Milestone Info")]
        public string milestoneName = "Monthly Goal";
        [TextArea(2, 4)]
        public string description = "";

        [Header("Conditions (all must be met)")]
        public List<MilestoneCondition> conditions = new();

        [Header("Rewards")]
        public int goldReward;
        public int experienceReward;
        [TextArea(1, 2)]
        public string rewardDescription = "";

        [Header("Penalty on Failure")]
        public int moralePenalty;
        [TextArea(1, 2)]
        public string penaltyDescription = "";

        [Header("Availability")]
        [Tooltip("Which month this milestone applies to. 0 = any month")]
        public int targetMonth;
    }
}
