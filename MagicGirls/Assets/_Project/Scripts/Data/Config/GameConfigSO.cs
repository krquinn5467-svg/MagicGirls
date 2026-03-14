using UnityEngine;

namespace MagicGirls.Data
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "MagicGirls/Game Config")]
    public class GameConfigSO : ScriptableObject
    {
        [Header("Time")]
        public int segmentsPerDay = 6;
        public int daysPerWeek = 7;
        public int weeksPerMonth = 4;

        [Header("Economy")]
        public int startingGold = 100;

        [Header("Team")]
        public int maxTeamSize = 3;

        [Header("Difficulty")]
        public float difficultyScalingPerWeek = 0.1f;

        [Header("Leveling")]
        public float baseExperienceToLevel = 100f;
        public float experienceScalingPerLevel = 1.5f;
    }
}
