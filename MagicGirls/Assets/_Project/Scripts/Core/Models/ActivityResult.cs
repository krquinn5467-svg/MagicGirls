using System.Collections.Generic;
using MagicGirls.Core.Enums;

namespace MagicGirls.Core.Models
{
    public class StatChange
    {
        public StatTargetType TargetType { get; set; }
        public BaseStat BaseStat { get; set; }
        public MagicalStat MagicalStat { get; set; }
        public int Amount { get; set; }
        public DiceRollResult Roll { get; set; }

        public string StatName => TargetType == StatTargetType.Base
            ? BaseStat.ToString()
            : MagicalStat.ToString();
    }

    public class ActivityResult
    {
        public ActivityType Activity { get; set; }
        public List<StatChange> StatChanges { get; set; } = new();
        public int StaminaChange { get; set; }
        public int MoraleChange { get; set; }
        public int GoldChange { get; set; }
        public float ExperienceGain { get; set; }
        public string NarrativeText { get; set; }
        public bool InjuryOccurred { get; set; }
        public List<DiceRollResult> AllRolls { get; set; } = new();
    }
}
