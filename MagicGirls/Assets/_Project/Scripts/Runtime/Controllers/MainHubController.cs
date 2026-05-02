using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using MagicGirls.Core.Models;
using MagicGirls.Core.Services;
using MagicGirls.Data;
using MagicGirls.Runtime;
using MagicGirls.UI.Screens;
using MagicGirls.Runtime.Screens;
using System.Threading.Tasks;
using Mono.Cecil.Cil;

namespace MagicGirls.Runtime.Controllers
{
    [RequireComponent(typeof(UIDocument))]
    public class MainHubController : MonoBehaviour
    {
        [Header("UI Templates")]
        [SerializeField] private VisualTreeAsset hubTemplate;
        [SerializeField] private VisualTreeAsset resultsTemplate;
        [SerializeField] private VisualTreeAsset dayResultsTemplate;
        [SerializeField] private VisualTreeAsset characterTemplate;
        [SerializeField] private VisualTreeAsset calendarTemplate;
        [SerializeField] private VisualTreeAsset battleTemplate;
        [SerializeField] private VisualTreeAsset schedulerTemplate;
        [SerializeField] private VisualTreeAsset modalTemplate;
        [SerializeField] private ScreenRegistryAsset screenRegistryAsset;

        private ScreenManager _screenManager;
        public GameManager _gameManager;
        private BattleController _battleController;

        private void Start()
        {
            _gameManager = FindAnyObjectByType<GameManager>();
            if (_gameManager == null)
            {
                var go = new GameObject("GameManager");
                _gameManager = go.AddComponent<GameManager>();
                Debug.LogWarning("[MainHub] GameManager not found, created fallback. Run from Boot scene for full setup.");
            }

            // Create BattleController
            _battleController = gameObject.AddComponent<BattleController>();

            SetupUI();
            BindEvents();
        }

        private void SetupUI()
        {

            //FIX: this code should be somewhere generic. it's not specific to the main hub scene.
            var uiDoc = GetComponent<UIDocument>();

            _screenManager = new ScreenManager(uiDoc.rootVisualElement);
            var screenRegistry = new ScreenRegistry(screenRegistryAsset);

            var screenFactory = new ReferencedScreenFactory
            (
                _screenManager,
                _gameManager,
                _battleController
            );
            
            _screenManager.Factory = screenFactory;
            _screenManager.Registry = screenRegistry;

            _ = _screenManager.ShowScreenAsync<HubScreen>();
        }

        private void BindEvents()
        {
            _gameManager.OnBattleTriggered += OnBattleTriggered;
            _gameManager.OnMilestoneEvaluated += OnMilestoneEvaluated;
        }

        private async void OnBattleTriggered(MonsterEncounterSO encounter)
        {
            Debug.Log($"[MainHub] Battle triggered: {encounter.encounterName}");

            // Switch to battle screen
            var screen = await _screenManager.ShowScreenAsync<BattleScreen>();

            // Start the battle
            float difficulty = _gameManager.GetDifficultyScale();
            var team = _gameManager.Team.ActiveTeam.ToList();
            _battleController.StartBattle(team, encounter, difficulty);
        }

        private void OnBattleDismissed()
        {
            // Apply battle results
            var battleState = _battleController.CurrentBattle;
            if (battleState != null)
            {
                _gameManager.ApplyBattleResults(battleState);
            }

            // Return to hub
            _ = _screenManager.ShowScreenAsync<HubScreen>();
        }

        private async void OnMilestoneEvaluated(MilestoneResult result)
        {
            Debug.Log($"[MainHub] Milestone evaluated: {result.Milestone.milestoneName} - {(result.Success ? "SUCCESS" : "FAILED")}");

            var milestoneScreen = await _screenManager.GetScreen<MilestoneScreen>() as MilestoneScreen;
            if (milestoneScreen != null)
            {
                milestoneScreen.ShowResult(result, OnMilestoneDismissed);
                _screenManager.ShowOverlay(milestoneScreen);
            }
        }

        private void OnMilestoneDismissed()
        {
            _gameManager.DismissMilestone();
            _screenManager.HideOverlay<MilestoneScreen>();
        }

        private void OnDestroy()
        {
            if (_gameManager != null)
            {
                _gameManager.OnBattleTriggered -= OnBattleTriggered;
                _gameManager.OnMilestoneEvaluated -= OnMilestoneEvaluated;
            }
        }

        private void OnGUI()
        {
            if (GUI.Button(new Rect(10, 10, 150, 50), "Screen Test"))
            {
                print("start screen test!" + _screenManager);
                _ = ShowTestScreen();
            }
        }

        private async Task ShowTestScreen()
        {
            print("ShowTestScreen start");
            var screen = await _screenManager.ShowScreenAsync<CalendarScreen>();
            print("ShowTestScreen end");
        }
    }
}
