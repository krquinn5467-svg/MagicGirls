using System;
using System.Collections.Generic;
using UnityEngine;
using MagicGirls.Core.Enums;

namespace MagicGirls.Data
{
    [Serializable]
    public class StatModifier
    {
        public StatTargetType targetType = StatTargetType.Base;
        public BaseStat baseStat;
        public MagicalStat magicalStat;
        public DieType dieType = DieType.D6;
        public int diceCount = 1;
        public int flatModifier = 0;

        public string StatName => targetType == StatTargetType.Base
            ? baseStat.ToString()
            : magicalStat.ToString();
    }

    [CreateAssetMenu(fileName = "NewActivity", menuName = "MagicGirls/Activity Definition")]
    public class ActivityDefinitionSO : ScriptableObject
    {
        [Header("Basic Info")]
        public ActivityType activityType;
        public string displayName = "Activity";
        [TextArea(2, 3)]
        public string description = "";
        public Sprite icon;

        [Header("Availability")]
        public bool nightOnly = false;

        [Header("Stat Effects")]
        public List<StatModifier> statModifiers = new();

        [Header("Costs & Rewards")]
        public int staminaCost = 0;
        public int moraleChange = 0;
        public int goldGain = 0;
        public float experienceGain = 0f;

        [Header("Dungeon Specific")]
        [Tooltip("DC for CON save to avoid injury during dungeon crawl")]
        public int injuryDC = 12;
    }
}
