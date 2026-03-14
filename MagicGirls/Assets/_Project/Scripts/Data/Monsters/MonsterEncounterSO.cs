using System.Collections.Generic;
using UnityEngine;

namespace MagicGirls.Data
{
    [CreateAssetMenu(fileName = "NewEncounter", menuName = "MagicGirls/Monster Encounter")]
    public class MonsterEncounterSO : ScriptableObject
    {
        [Header("Encounter Info")]
        public string encounterName = "Monster Fight";
        [TextArea(2, 3)]
        public string description = "";

        [Header("Monsters")]
        public List<MonsterTemplateSO> monsters = new();

        [Header("Availability")]
        [Tooltip("Minimum week number (inclusive) for this encounter to appear")]
        public int minWeek = 1;
        [Tooltip("Maximum week number (inclusive) for this encounter to appear. 0 = no limit")]
        public int maxWeek = 0;

        [Header("Type")]
        public bool isBoss = false;

        [Header("Monthly Milestone (Boss only)")]
        [TextArea(2, 3)]
        public string milestoneDescription = "";
    }
}
