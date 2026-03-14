using UnityEngine;
using UnityEngine.SceneManagement;
using MagicGirls.Core.Services;
using MagicGirls.Core.Systems;
using MagicGirls.Data;

namespace MagicGirls.Runtime
{
    [DefaultExecutionOrder(-100)]
    public class GameBootstrapper : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private GameConfigSO gameConfig;
        [SerializeField] private CharacterTemplateSO[] starterCharacters;
        [SerializeField] private ActivityDefinitionSO[] activityDefinitions;
        [SerializeField] private MonsterEncounterSO[] encounters;
        [SerializeField] private MilestoneSO[] milestones;

        [Header("Scenes")]
        [SerializeField] private string mainHubSceneName = "MainHub";

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            InitializeSystems();
            LoadMainHub();
        }

        private void InitializeSystems()
        {
            // Core services
            var diceService = new DiceService();
            var activityResolver = new ActivityResolver(diceService);

            // Core systems
            var timeSystem = new TimeSystem();
            var teamSystem = new TeamSystem();
            var economySystem = new EconomySystem();
            var scheduleSystem = new ScheduleSystem();

            economySystem.Initialize(gameConfig.startingGold);

            // Register all
            ServiceLocator.Register(gameConfig);
            ServiceLocator.Register(diceService);
            ServiceLocator.Register(activityResolver);
            ServiceLocator.Register(timeSystem);
            ServiceLocator.Register(teamSystem);
            ServiceLocator.Register(economySystem);
            ServiceLocator.Register(scheduleSystem);

            // Store activity definitions for lookup
            var activityRegistry = new ActivityRegistry(activityDefinitions);
            ServiceLocator.Register(activityRegistry);

            // Store encounters for battle system
            var encounterRegistry = new EncounterRegistry(encounters);
            ServiceLocator.Register(encounterRegistry);

            // Milestone registry
            var milestoneRegistry = new MilestoneRegistry(milestones);
            ServiceLocator.Register(milestoneRegistry);

            // Milestone evaluator
            var milestoneEvaluator = new MilestoneEvaluator(teamSystem, economySystem);
            ServiceLocator.Register(milestoneEvaluator);

            // Inventory system
            var inventorySystem = new InventorySystem();
            ServiceLocator.Register(inventorySystem);

            // Save system
            var saveSystem = new SaveSystem(timeSystem, teamSystem, economySystem, inventorySystem);
            ServiceLocator.Register(saveSystem);

            // Create starter characters
            foreach (var template in starterCharacters)
            {
                teamSystem.AddCharacter(template);
            }

            Debug.Log($"[MagicGirls] Systems initialized. Team size: {teamSystem.ActiveTeam.Count}, Gold: {economySystem.Gold}");
        }

        private void LoadMainHub()
        {
            SceneManager.LoadScene(mainHubSceneName, LoadSceneMode.Additive);
        }

        private void OnDestroy()
        {
            ServiceLocator.Clear();
        }
    }
}
