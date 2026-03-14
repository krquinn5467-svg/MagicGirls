using System.Collections.Generic;
using UnityEngine;
using MagicGirls.Core.Enums;

namespace MagicGirls.Data
{
    [CreateAssetMenu(fileName = "NewMonster", menuName = "MagicGirls/Monster Template")]
    public class MonsterTemplateSO : ScriptableObject
    {
        [Header("Identity")]
        public string monsterName = "Monster";
        [TextArea(2, 3)]
        public string description = "";
        public Sprite sprite;

        [Header("Base Stats")]
        public int baseHP = 20;
        public int strength = 10;
        public int dexterity = 10;
        public int constitution = 10;
        public int magicPower = 10;

        [Header("Combat")]
        public DamageType attackType = DamageType.Physical;
        public DieType attackDie = DieType.D6;
        public int attackDiceCount = 1;
        public int attackBonus = 0;
        public int armorClass = 10;

        [Header("Resistances")]
        public List<DamageType> weaknesses = new();
        public List<DamageType> resistances = new();

        [Header("Difficulty")]
        public int difficultyRating = 1;
        public int experienceReward = 50;
        public int goldReward = 20;
    }
}
