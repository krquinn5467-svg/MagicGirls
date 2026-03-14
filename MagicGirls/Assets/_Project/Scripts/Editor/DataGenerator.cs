#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using MagicGirls.Core.Enums;
using MagicGirls.Data;
using System.Collections.Generic;

namespace MagicGirls.Editor
{
    public static class DataGenerator
    {
        [MenuItem("MagicGirls/Generate Starter Data")]
        public static void GenerateStarterData()
        {
            CreateDirectories();
            CreateGameConfig();
            CreateCharacterTemplates();
            CreateActivityDefinitions();
            CreateMonsterTemplates();
            CreateEncounters();
            CreateItems();
            CreateLootTables();
            CreateMilestones();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[MagicGirls] Starter data generated successfully!");
        }

        private static void CreateDirectories()
        {
            EnsureFolder("Assets/_Project/Data");
            EnsureFolder("Assets/_Project/Data/Characters");
            EnsureFolder("Assets/_Project/Data/Activities");
            EnsureFolder("Assets/_Project/Data/Config");
            EnsureFolder("Assets/_Project/Data/Monsters");
            EnsureFolder("Assets/_Project/Data/Items");
            EnsureFolder("Assets/_Project/Data/LootTables");
            EnsureFolder("Assets/_Project/Data/Milestones");
        }

        private static void EnsureFolder(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                var parts = path.Split('/');
                var current = parts[0];
                for (int i = 1; i < parts.Length; i++)
                {
                    var next = current + "/" + parts[i];
                    if (!AssetDatabase.IsValidFolder(next))
                        AssetDatabase.CreateFolder(current, parts[i]);
                    current = next;
                }
            }
        }

        private static void CreateGameConfig()
        {
            var config = ScriptableObject.CreateInstance<GameConfigSO>();
            config.segmentsPerDay = 6;
            config.daysPerWeek = 7;
            config.weeksPerMonth = 4;
            config.startingGold = 100;
            config.maxTeamSize = 3;
            config.difficultyScalingPerWeek = 0.1f;
            config.baseExperienceToLevel = 100f;
            config.experienceScalingPerLevel = 1.5f;

            AssetDatabase.CreateAsset(config, "Assets/_Project/Data/Config/GameConfig.asset");
        }

        private static void CreateCharacterTemplates()
        {
            // Character 1: Sakura - Brave & Cheerful - Physical fighter
            var sakura = ScriptableObject.CreateInstance<CharacterTemplateSO>();
            sakura.characterName = "Sakura";
            sakura.backstory = "A spirited girl who faces every challenge with a smile. Her bravery is matched only by her kindness.";
            sakura.startingSTR = 14;
            sakura.startingDEX = 12;
            sakura.startingCON = 13;
            sakura.startingINT = 9;
            sakura.startingWIS = 10;
            sakura.startingCHA = 14;
            sakura.startingMagicPower = 11;
            sakura.startingMagicAffinity = 10;
            sakura.startingCorruption = 0;
            sakura.startingMorale = 18;
            sakura.startingHP = 24;
            sakura.startingStamina = 12;
            sakura.defaultTraits = new List<PersonalityTrait> { PersonalityTrait.Brave, PersonalityTrait.Cheerful };

            AssetDatabase.CreateAsset(sakura, "Assets/_Project/Data/Characters/Sakura.asset");

            // Character 2: Luna - Studious & Aloof - Magic user
            var luna = ScriptableObject.CreateInstance<CharacterTemplateSO>();
            luna.characterName = "Luna";
            luna.backstory = "A quiet, brilliant girl whose mastery of magic is unmatched. She prefers books to people.";
            luna.startingSTR = 8;
            luna.startingDEX = 10;
            luna.startingCON = 10;
            luna.startingINT = 16;
            luna.startingWIS = 14;
            luna.startingCHA = 8;
            luna.startingMagicPower = 16;
            luna.startingMagicAffinity = 14;
            luna.startingCorruption = 2;
            luna.startingMorale = 12;
            luna.startingHP = 18;
            luna.startingStamina = 10;
            luna.defaultTraits = new List<PersonalityTrait> { PersonalityTrait.Studious, PersonalityTrait.Aloof };

            AssetDatabase.CreateAsset(luna, "Assets/_Project/Data/Characters/Luna.asset");

            // Character 3: Hana - Kind & Diligent - Support/balanced
            var hana = ScriptableObject.CreateInstance<CharacterTemplateSO>();
            hana.characterName = "Hana";
            hana.backstory = "A hardworking girl with a heart of gold. She always puts others before herself.";
            hana.startingSTR = 11;
            hana.startingDEX = 11;
            hana.startingCON = 12;
            hana.startingINT = 12;
            hana.startingWIS = 13;
            hana.startingCHA = 13;
            hana.startingMagicPower = 12;
            hana.startingMagicAffinity = 12;
            hana.startingCorruption = 0;
            hana.startingMorale = 16;
            hana.startingHP = 22;
            hana.startingStamina = 12;
            hana.defaultTraits = new List<PersonalityTrait> { PersonalityTrait.Kind, PersonalityTrait.Diligent };

            AssetDatabase.CreateAsset(hana, "Assets/_Project/Data/Characters/Hana.asset");
        }

        private static void CreateActivityDefinitions()
        {
            // REST
            var rest = ScriptableObject.CreateInstance<ActivityDefinitionSO>();
            rest.activityType = ActivityType.Rest;
            rest.displayName = "Rest";
            rest.description = "Take a break and recover stamina. A well-rested girl fights better!";
            rest.nightOnly = false;
            rest.staminaCost = 0;
            rest.moraleChange = 2;
            rest.goldGain = 0;
            rest.experienceGain = 0;
            // Rest has no stat modifiers - special handling in ActivityResolver recovers stamina
            AssetDatabase.CreateAsset(rest, "Assets/_Project/Data/Activities/Activity_Rest.asset");

            // TRAIN
            var train = ScriptableObject.CreateInstance<ActivityDefinitionSO>();
            train.activityType = ActivityType.Train;
            train.displayName = "Train";
            train.description = "Physical and combat training. Builds strength, dexterity, and constitution.";
            train.nightOnly = false;
            train.staminaCost = 2;
            train.moraleChange = -1;
            train.goldGain = 0;
            train.experienceGain = 10;
            train.statModifiers = new List<StatModifier>
            {
                new StatModifier { targetType = StatTargetType.Base, baseStat = BaseStat.STR, dieType = DieType.D4, diceCount = 1, flatModifier = 0 },
                new StatModifier { targetType = StatTargetType.Base, baseStat = BaseStat.DEX, dieType = DieType.D4, diceCount = 1, flatModifier = 0 },
                new StatModifier { targetType = StatTargetType.Base, baseStat = BaseStat.CON, dieType = DieType.D4, diceCount = 1, flatModifier = -1 }
            };
            AssetDatabase.CreateAsset(train, "Assets/_Project/Data/Activities/Activity_Train.asset");

            // SCHOOL
            var school = ScriptableObject.CreateInstance<ActivityDefinitionSO>();
            school.activityType = ActivityType.School;
            school.displayName = "School";
            school.description = "Study magic theory and general knowledge. Sharpens the mind.";
            school.nightOnly = false;
            school.staminaCost = 1;
            school.moraleChange = -1;
            school.goldGain = 0;
            school.experienceGain = 10;
            school.statModifiers = new List<StatModifier>
            {
                new StatModifier { targetType = StatTargetType.Base, baseStat = BaseStat.INT, dieType = DieType.D6, diceCount = 1, flatModifier = 0 },
                new StatModifier { targetType = StatTargetType.Base, baseStat = BaseStat.WIS, dieType = DieType.D4, diceCount = 1, flatModifier = 0 },
                new StatModifier { targetType = StatTargetType.Magical, magicalStat = MagicalStat.MagicAffinity, dieType = DieType.D4, diceCount = 1, flatModifier = -1 }
            };
            AssetDatabase.CreateAsset(school, "Assets/_Project/Data/Activities/Activity_School.asset");

            // DUNGEON CRAWL
            var dungeon = ScriptableObject.CreateInstance<ActivityDefinitionSO>();
            dungeon.activityType = ActivityType.DungeonCrawl;
            dungeon.displayName = "Dungeon Crawl";
            dungeon.description = "Explore dangerous dungeons at night. High risk, high reward!";
            dungeon.nightOnly = true;
            dungeon.staminaCost = 3;
            dungeon.moraleChange = -2;
            dungeon.goldGain = 0;
            dungeon.experienceGain = 25;
            dungeon.injuryDC = 12;
            dungeon.statModifiers = new List<StatModifier>
            {
                new StatModifier { targetType = StatTargetType.Magical, magicalStat = MagicalStat.MagicPower, dieType = DieType.D6, diceCount = 1, flatModifier = 0 },
                new StatModifier { targetType = StatTargetType.Base, baseStat = BaseStat.CON, dieType = DieType.D4, diceCount = 1, flatModifier = 0 },
                new StatModifier { targetType = StatTargetType.Base, baseStat = BaseStat.STR, dieType = DieType.D4, diceCount = 1, flatModifier = 0 }
            };
            AssetDatabase.CreateAsset(dungeon, "Assets/_Project/Data/Activities/Activity_DungeonCrawl.asset");

            // RECREATION
            var rec = ScriptableObject.CreateInstance<ActivityDefinitionSO>();
            rec.activityType = ActivityType.Recreation;
            rec.displayName = "Recreation";
            rec.description = "Have fun and socialize. Boosts morale and charm.";
            rec.nightOnly = false;
            rec.staminaCost = 0;
            rec.moraleChange = 3;
            rec.goldGain = 0;
            rec.experienceGain = 5;
            rec.statModifiers = new List<StatModifier>
            {
                new StatModifier { targetType = StatTargetType.Base, baseStat = BaseStat.CHA, dieType = DieType.D4, diceCount = 1, flatModifier = 1 }
            };
            AssetDatabase.CreateAsset(rec, "Assets/_Project/Data/Activities/Activity_Recreation.asset");

            // WORK
            var work = ScriptableObject.CreateInstance<ActivityDefinitionSO>();
            work.activityType = ActivityType.Work;
            work.displayName = "Work";
            work.description = "Earn gold through part-time jobs. Builds character too!";
            work.nightOnly = false;
            work.staminaCost = 2;
            work.moraleChange = -1;
            work.goldGain = 10;
            work.experienceGain = 5;
            work.statModifiers = new List<StatModifier>
            {
                new StatModifier { targetType = StatTargetType.Base, baseStat = BaseStat.WIS, dieType = DieType.D4, diceCount = 1, flatModifier = -1 }
            };
            AssetDatabase.CreateAsset(work, "Assets/_Project/Data/Activities/Activity_Work.asset");
        }

        private static void CreateMonsterTemplates()
        {
            // Slime - Week 1 easy monster
            var slime = ScriptableObject.CreateInstance<MonsterTemplateSO>();
            slime.monsterName = "Shadow Slime";
            slime.description = "A dark, gelatinous creature that lurks in shadows.";
            slime.baseHP = 15;
            slime.strength = 8;
            slime.dexterity = 6;
            slime.constitution = 12;
            slime.magicPower = 4;
            slime.attackType = DamageType.Physical;
            slime.attackDie = DieType.D4;
            slime.attackDiceCount = 1;
            slime.attackBonus = 0;
            slime.armorClass = 8;
            slime.weaknesses = new List<DamageType> { DamageType.Fire };
            slime.difficultyRating = 1;
            slime.experienceReward = 30;
            slime.goldReward = 10;
            AssetDatabase.CreateAsset(slime, "Assets/_Project/Data/Monsters/Monster_Slime.asset");

            // Dark Imp - Week 1-2 monster
            var imp = ScriptableObject.CreateInstance<MonsterTemplateSO>();
            imp.monsterName = "Dark Imp";
            imp.description = "A small, mischievous demon with sharp claws.";
            imp.baseHP = 18;
            imp.strength = 10;
            imp.dexterity = 14;
            imp.constitution = 10;
            imp.magicPower = 8;
            imp.attackType = DamageType.Dark;
            imp.attackDie = DieType.D6;
            imp.attackDiceCount = 1;
            imp.attackBonus = 1;
            imp.armorClass = 12;
            imp.weaknesses = new List<DamageType> { DamageType.Holy };
            imp.resistances = new List<DamageType> { DamageType.Dark };
            imp.difficultyRating = 2;
            imp.experienceReward = 50;
            imp.goldReward = 15;
            AssetDatabase.CreateAsset(imp, "Assets/_Project/Data/Monsters/Monster_Imp.asset");

            // Corrupted Wolf - Week 2-3 monster
            var wolf = ScriptableObject.CreateInstance<MonsterTemplateSO>();
            wolf.monsterName = "Corrupted Wolf";
            wolf.description = "A once-noble wolf twisted by dark magic.";
            wolf.baseHP = 25;
            wolf.strength = 14;
            wolf.dexterity = 12;
            wolf.constitution = 12;
            wolf.magicPower = 6;
            wolf.attackType = DamageType.Physical;
            wolf.attackDie = DieType.D6;
            wolf.attackDiceCount = 2;
            wolf.attackBonus = 2;
            wolf.armorClass = 11;
            wolf.weaknesses = new List<DamageType> { DamageType.Holy, DamageType.Fire };
            wolf.difficultyRating = 3;
            wolf.experienceReward = 70;
            wolf.goldReward = 20;
            AssetDatabase.CreateAsset(wolf, "Assets/_Project/Data/Monsters/Monster_Wolf.asset");

            // Shadow Knight - Week 3-4 monster
            var knight = ScriptableObject.CreateInstance<MonsterTemplateSO>();
            knight.monsterName = "Shadow Knight";
            knight.description = "An armored specter wielding a dark blade.";
            knight.baseHP = 35;
            knight.strength = 16;
            knight.dexterity = 10;
            knight.constitution = 14;
            knight.magicPower = 12;
            knight.attackType = DamageType.Dark;
            knight.attackDie = DieType.D8;
            knight.attackDiceCount = 1;
            knight.attackBonus = 3;
            knight.armorClass = 15;
            knight.weaknesses = new List<DamageType> { DamageType.Holy };
            knight.resistances = new List<DamageType> { DamageType.Physical, DamageType.Dark };
            knight.difficultyRating = 4;
            knight.experienceReward = 100;
            knight.goldReward = 30;
            AssetDatabase.CreateAsset(knight, "Assets/_Project/Data/Monsters/Monster_Knight.asset");

            // Boss: Dark Queen - Monthly boss
            var queen = ScriptableObject.CreateInstance<MonsterTemplateSO>();
            queen.monsterName = "Dark Queen Lilith";
            queen.description = "The sinister queen who commands the shadow forces.";
            queen.baseHP = 60;
            queen.strength = 14;
            queen.dexterity = 12;
            queen.constitution = 16;
            queen.magicPower = 18;
            queen.attackType = DamageType.Dark;
            queen.attackDie = DieType.D10;
            queen.attackDiceCount = 1;
            queen.attackBonus = 4;
            queen.armorClass = 16;
            queen.weaknesses = new List<DamageType> { DamageType.Holy };
            queen.resistances = new List<DamageType> { DamageType.Dark, DamageType.Ice };
            queen.difficultyRating = 6;
            queen.experienceReward = 200;
            queen.goldReward = 100;
            AssetDatabase.CreateAsset(queen, "Assets/_Project/Data/Monsters/Monster_DarkQueen.asset");
        }

        private static void CreateEncounters()
        {
            // Load monster assets
            var slime = AssetDatabase.LoadAssetAtPath<MonsterTemplateSO>("Assets/_Project/Data/Monsters/Monster_Slime.asset");
            var imp = AssetDatabase.LoadAssetAtPath<MonsterTemplateSO>("Assets/_Project/Data/Monsters/Monster_Imp.asset");
            var wolf = AssetDatabase.LoadAssetAtPath<MonsterTemplateSO>("Assets/_Project/Data/Monsters/Monster_Wolf.asset");
            var knight = AssetDatabase.LoadAssetAtPath<MonsterTemplateSO>("Assets/_Project/Data/Monsters/Monster_Knight.asset");
            var queen = AssetDatabase.LoadAssetAtPath<MonsterTemplateSO>("Assets/_Project/Data/Monsters/Monster_DarkQueen.asset");

            // Week 1: Slime x2
            var enc1 = ScriptableObject.CreateInstance<MonsterEncounterSO>();
            enc1.encounterName = "Shadow Slime Ambush";
            enc1.description = "A pair of shadow slimes block the path!";
            enc1.monsters = new List<MonsterTemplateSO> { slime, slime };
            enc1.minWeek = 1;
            enc1.maxWeek = 2;
            AssetDatabase.CreateAsset(enc1, "Assets/_Project/Data/Monsters/Encounter_Week1.asset");

            // Week 2: Imp + Slime
            var enc2 = ScriptableObject.CreateInstance<MonsterEncounterSO>();
            enc2.encounterName = "Dark Imp Raid";
            enc2.description = "A dark imp leads a slime into battle!";
            enc2.monsters = new List<MonsterTemplateSO> { imp, slime };
            enc2.minWeek = 2;
            enc2.maxWeek = 3;
            AssetDatabase.CreateAsset(enc2, "Assets/_Project/Data/Monsters/Encounter_Week2.asset");

            // Week 3: Wolf + Imp
            var enc3 = ScriptableObject.CreateInstance<MonsterEncounterSO>();
            enc3.encounterName = "Corrupted Pack";
            enc3.description = "A corrupted wolf and its imp handler attack!";
            enc3.monsters = new List<MonsterTemplateSO> { wolf, imp };
            enc3.minWeek = 3;
            enc3.maxWeek = 0;
            AssetDatabase.CreateAsset(enc3, "Assets/_Project/Data/Monsters/Encounter_Week3.asset");

            // Week 4+: Knight + Wolf
            var enc4 = ScriptableObject.CreateInstance<MonsterEncounterSO>();
            enc4.encounterName = "Shadow Vanguard";
            enc4.description = "A shadow knight commands a corrupted wolf!";
            enc4.monsters = new List<MonsterTemplateSO> { knight, wolf };
            enc4.minWeek = 4;
            enc4.maxWeek = 0;
            AssetDatabase.CreateAsset(enc4, "Assets/_Project/Data/Monsters/Encounter_Week4.asset");

            // Boss: Dark Queen
            var boss = ScriptableObject.CreateInstance<MonsterEncounterSO>();
            boss.encounterName = "Dark Queen's Throne";
            boss.description = "The Dark Queen awaits at the end of the month!";
            boss.monsters = new List<MonsterTemplateSO> { queen };
            boss.isBoss = true;
            boss.milestoneDescription = "Defeat the Dark Queen to complete the month!";
            boss.minWeek = 1;
            boss.maxWeek = 0;
            AssetDatabase.CreateAsset(boss, "Assets/_Project/Data/Monsters/Encounter_Boss.asset");
        }

        private static void CreateItems()
        {
            // Weapon: Wooden Sword
            var sword = ScriptableObject.CreateInstance<ItemDefinitionSO>();
            sword.itemName = "Wooden Sword";
            sword.description = "A basic training sword. Better than bare fists.";
            sword.itemType = ItemType.Weapon;
            sword.equipSlot = EquipSlot.Weapon;
            sword.statBonuses = new List<ItemStatBonus>
            {
                new ItemStatBonus { targetType = StatTargetType.Base, baseStat = BaseStat.STR, bonus = 2 }
            };
            sword.attackBonus = 1;
            sword.buyPrice = 50;
            sword.sellPrice = 25;
            AssetDatabase.CreateAsset(sword, "Assets/_Project/Data/Items/Item_WoodenSword.asset");

            // Weapon: Magic Wand
            var wand = ScriptableObject.CreateInstance<ItemDefinitionSO>();
            wand.itemName = "Magic Wand";
            wand.description = "A wand that channels magical energy.";
            wand.itemType = ItemType.Weapon;
            wand.equipSlot = EquipSlot.Weapon;
            wand.statBonuses = new List<ItemStatBonus>
            {
                new ItemStatBonus { targetType = StatTargetType.Magical, magicalStat = MagicalStat.MagicPower, bonus = 3 }
            };
            wand.attackBonus = 1;
            wand.buyPrice = 75;
            wand.sellPrice = 35;
            AssetDatabase.CreateAsset(wand, "Assets/_Project/Data/Items/Item_MagicWand.asset");

            // Armor: Leather Vest
            var vest = ScriptableObject.CreateInstance<ItemDefinitionSO>();
            vest.itemName = "Leather Vest";
            vest.description = "Light armor that doesn't restrict movement.";
            vest.itemType = ItemType.Armor;
            vest.equipSlot = EquipSlot.Armor;
            vest.armorClassBonus = 2;
            vest.hpBonus = 5;
            vest.buyPrice = 60;
            vest.sellPrice = 30;
            AssetDatabase.CreateAsset(vest, "Assets/_Project/Data/Items/Item_LeatherVest.asset");

            // Armor: Mystic Robe
            var robe = ScriptableObject.CreateInstance<ItemDefinitionSO>();
            robe.itemName = "Mystic Robe";
            robe.description = "A robe woven with protective enchantments.";
            robe.itemType = ItemType.Armor;
            robe.equipSlot = EquipSlot.Armor;
            robe.armorClassBonus = 1;
            robe.statBonuses = new List<ItemStatBonus>
            {
                new ItemStatBonus { targetType = StatTargetType.Magical, magicalStat = MagicalStat.MagicAffinity, bonus = 2 }
            };
            robe.buyPrice = 80;
            robe.sellPrice = 40;
            AssetDatabase.CreateAsset(robe, "Assets/_Project/Data/Items/Item_MysticRobe.asset");

            // Accessory: Lucky Charm
            var charm = ScriptableObject.CreateInstance<ItemDefinitionSO>();
            charm.itemName = "Lucky Charm";
            charm.description = "A trinket that brings good fortune.";
            charm.itemType = ItemType.Accessory;
            charm.equipSlot = EquipSlot.Accessory;
            charm.statBonuses = new List<ItemStatBonus>
            {
                new ItemStatBonus { targetType = StatTargetType.Base, baseStat = BaseStat.CHA, bonus = 2 },
                new ItemStatBonus { targetType = StatTargetType.Base, baseStat = BaseStat.WIS, bonus = 1 }
            };
            charm.buyPrice = 40;
            charm.sellPrice = 20;
            AssetDatabase.CreateAsset(charm, "Assets/_Project/Data/Items/Item_LuckyCharm.asset");

            // Consumable: Healing Potion
            var potion = ScriptableObject.CreateInstance<ItemDefinitionSO>();
            potion.itemName = "Healing Potion";
            potion.description = "Restores a moderate amount of health.";
            potion.itemType = ItemType.Consumable;
            potion.isConsumable = true;
            potion.healAmount = 15;
            potion.buyPrice = 25;
            potion.sellPrice = 12;
            AssetDatabase.CreateAsset(potion, "Assets/_Project/Data/Items/Item_HealingPotion.asset");

            // Consumable: Stamina Tonic
            var tonic = ScriptableObject.CreateInstance<ItemDefinitionSO>();
            tonic.itemName = "Stamina Tonic";
            tonic.description = "Restores stamina for another day of hard work.";
            tonic.itemType = ItemType.Consumable;
            tonic.isConsumable = true;
            tonic.staminaRestore = 5;
            tonic.buyPrice = 20;
            tonic.sellPrice = 10;
            AssetDatabase.CreateAsset(tonic, "Assets/_Project/Data/Items/Item_StaminaTonic.asset");
        }

        private static void CreateLootTables()
        {
            var sword = AssetDatabase.LoadAssetAtPath<ItemDefinitionSO>("Assets/_Project/Data/Items/Item_WoodenSword.asset");
            var wand = AssetDatabase.LoadAssetAtPath<ItemDefinitionSO>("Assets/_Project/Data/Items/Item_MagicWand.asset");
            var vest = AssetDatabase.LoadAssetAtPath<ItemDefinitionSO>("Assets/_Project/Data/Items/Item_LeatherVest.asset");
            var robe = AssetDatabase.LoadAssetAtPath<ItemDefinitionSO>("Assets/_Project/Data/Items/Item_MysticRobe.asset");
            var charm = AssetDatabase.LoadAssetAtPath<ItemDefinitionSO>("Assets/_Project/Data/Items/Item_LuckyCharm.asset");
            var potion = AssetDatabase.LoadAssetAtPath<ItemDefinitionSO>("Assets/_Project/Data/Items/Item_HealingPotion.asset");
            var tonic = AssetDatabase.LoadAssetAtPath<ItemDefinitionSO>("Assets/_Project/Data/Items/Item_StaminaTonic.asset");

            // Dungeon loot
            var dungeonLoot = ScriptableObject.CreateInstance<LootTableSO>();
            dungeonLoot.tableName = "Dungeon Loot";
            dungeonLoot.minGold = 5;
            dungeonLoot.maxGold = 20;
            dungeonLoot.entries = new List<LootEntry>
            {
                new LootEntry { item = potion, dropChance = 0.4f, minQuantity = 1, maxQuantity = 2 },
                new LootEntry { item = tonic, dropChance = 0.3f, minQuantity = 1, maxQuantity = 1 },
                new LootEntry { item = charm, dropChance = 0.1f, minQuantity = 1, maxQuantity = 1 }
            };
            AssetDatabase.CreateAsset(dungeonLoot, "Assets/_Project/Data/LootTables/LootTable_Dungeon.asset");

            // Battle loot
            var battleLoot = ScriptableObject.CreateInstance<LootTableSO>();
            battleLoot.tableName = "Battle Loot";
            battleLoot.minGold = 10;
            battleLoot.maxGold = 30;
            battleLoot.entries = new List<LootEntry>
            {
                new LootEntry { item = potion, dropChance = 0.5f, minQuantity = 1, maxQuantity = 2 },
                new LootEntry { item = sword, dropChance = 0.15f, minQuantity = 1, maxQuantity = 1 },
                new LootEntry { item = vest, dropChance = 0.1f, minQuantity = 1, maxQuantity = 1 }
            };
            AssetDatabase.CreateAsset(battleLoot, "Assets/_Project/Data/LootTables/LootTable_Battle.asset");

            // Boss loot
            var bossLoot = ScriptableObject.CreateInstance<LootTableSO>();
            bossLoot.tableName = "Boss Loot";
            bossLoot.minGold = 50;
            bossLoot.maxGold = 100;
            bossLoot.entries = new List<LootEntry>
            {
                new LootEntry { item = wand, dropChance = 0.3f, minQuantity = 1, maxQuantity = 1 },
                new LootEntry { item = robe, dropChance = 0.3f, minQuantity = 1, maxQuantity = 1 },
                new LootEntry { item = charm, dropChance = 0.25f, minQuantity = 1, maxQuantity = 1 },
                new LootEntry { item = potion, dropChance = 0.8f, minQuantity = 2, maxQuantity = 3 }
            };
            AssetDatabase.CreateAsset(bossLoot, "Assets/_Project/Data/LootTables/LootTable_Boss.asset");
        }

        private static void CreateMilestones()
        {
            // Month 1 milestone: Defeat the boss + have any char reach 15 STR or INT
            var m1 = ScriptableObject.CreateInstance<MilestoneSO>();
            m1.milestoneName = "First Steps";
            m1.description = "Prove your team's worth by defeating the Dark Queen and growing stronger.";
            m1.conditions = new List<MilestoneCondition>
            {
                new MilestoneCondition
                {
                    conditionType = MilestoneConditionType.DefeatBoss
                },
                new MilestoneCondition
                {
                    conditionType = MilestoneConditionType.ReachStatValue,
                    targetBaseStat = BaseStat.STR,
                    threshold = 15
                }
            };
            m1.goldReward = 200;
            m1.experienceReward = 150;
            m1.rewardDescription = "The girls grow in confidence!";
            m1.moralePenalty = 5;
            m1.penaltyDescription = "The team feels discouraged...";
            m1.targetMonth = 1;
            AssetDatabase.CreateAsset(m1, "Assets/_Project/Data/Milestones/Milestone_Month1.asset");

            // Month 2 milestone: Boss + level 3
            var m2 = ScriptableObject.CreateInstance<MilestoneSO>();
            m2.milestoneName = "Rising Stars";
            m2.description = "Push beyond your limits and show real growth.";
            m2.conditions = new List<MilestoneCondition>
            {
                new MilestoneCondition
                {
                    conditionType = MilestoneConditionType.DefeatBoss
                },
                new MilestoneCondition
                {
                    conditionType = MilestoneConditionType.ReachLevel,
                    threshold = 3
                }
            };
            m2.goldReward = 300;
            m2.experienceReward = 250;
            m2.rewardDescription = "Word of the girls' deeds spreads across the land!";
            m2.moralePenalty = 8;
            m2.penaltyDescription = "Doubt creeps into the team's hearts...";
            m2.targetMonth = 2;
            AssetDatabase.CreateAsset(m2, "Assets/_Project/Data/Milestones/Milestone_Month2.asset");

            // Generic repeating milestone: Boss + 500 gold
            var mGeneric = ScriptableObject.CreateInstance<MilestoneSO>();
            mGeneric.milestoneName = "Monthly Trial";
            mGeneric.description = "Each month brings new challenges. Defeat the boss and maintain your treasury.";
            mGeneric.conditions = new List<MilestoneCondition>
            {
                new MilestoneCondition
                {
                    conditionType = MilestoneConditionType.DefeatBoss
                },
                new MilestoneCondition
                {
                    conditionType = MilestoneConditionType.EarnGold,
                    threshold = 500
                }
            };
            mGeneric.goldReward = 250;
            mGeneric.experienceReward = 200;
            mGeneric.rewardDescription = "Another month conquered!";
            mGeneric.moralePenalty = 5;
            mGeneric.penaltyDescription = "The team struggles to keep up...";
            mGeneric.targetMonth = 0; // Any month (fallback)
            AssetDatabase.CreateAsset(mGeneric, "Assets/_Project/Data/Milestones/Milestone_Generic.asset");
        }
    }
}
#endif
