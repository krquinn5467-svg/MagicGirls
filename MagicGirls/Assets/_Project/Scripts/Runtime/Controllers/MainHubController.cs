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


        private UIManager _uiManager;       //to be replaced by screenmanager
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

            var uiDoc = GetComponent<UIDocument>();
            var screenFactory = new ReferencedScreenFactory();
            var screenRegistry = new ScreenRegistry(screenRegistryAsset);

            //FIX: this code should be somewhere generic. it's not specific to the main hub scene.
            _screenManager = new ScreenManager(screenRegistry, screenFactory, uiDoc.rootVisualElement);
            
            _uiManager = gameObject.AddComponent<UIManager>();

            SetUIDocumentOnManager(_uiManager, uiDoc);

            // Hub screen
            var hubScreen = new HubScreen(hubTemplate);
            hubScreen.Initialize(_gameManager, _uiManager);
            _uiManager.RegisterScreen("Hub", hubScreen);

            var schedulerScreen = new SchedulerScreen(schedulerTemplate);
            schedulerScreen.Initialize(_gameManager, _uiManager);
            _uiManager.RegisterScreen("Scheduler", schedulerScreen);

            // Results overlay
            var resultsScreen = new ResultsScreen(resultsTemplate);
            resultsScreen.Initialize(_uiManager);
            _uiManager.RegisterScreen("Results", resultsScreen);

            // Day results overlay
            var dayResultsScreen = new DayResultsScreen(dayResultsTemplate);
            dayResultsScreen.Initialize(_uiManager);
            _uiManager.RegisterScreen("DayResults", dayResultsScreen);

            // Character detail screen
            if (characterTemplate != null)
            {
                var characterScreen = new CharacterScreen(characterTemplate);
                characterScreen.Initialize(_gameManager, _uiManager);
                _uiManager.RegisterScreen("Character", characterScreen);
            }

            // Calendar screen
            if (calendarTemplate != null)
            {
                var calendarScreen = new CalendarScreen(calendarTemplate);
                calendarScreen.Initialize(_gameManager, _uiManager);
                _uiManager.RegisterScreen("Calendar", calendarScreen);
            }

            // Battle screen
            if (battleTemplate != null)
            {
                var battleScreen = new BattleScreen(battleTemplate);
                battleScreen.Initialize(_gameManager, _uiManager, _battleController);
                battleScreen.SetDismissCallback(OnBattleDismissed);
                _uiManager.RegisterScreen("Battle", battleScreen);
            }

            if(modalTemplate != null)
            {  
            // Milestone screen (programmatic, no template)
            var milestoneScreen = new MilestoneScreen(modalTemplate);
            _uiManager.RegisterScreen("Milestone", milestoneScreen);
            }

            _uiManager.ShowScreen("Hub");
        }

        private void BindEvents()
        {
            _gameManager.OnBattleTriggered += OnBattleTriggered;
            _gameManager.OnMilestoneEvaluated += OnMilestoneEvaluated;
        }

        private void OnBattleTriggered(MonsterEncounterSO encounter)
        {
            Debug.Log($"[MainHub] Battle triggered: {encounter.encounterName}");

            // Switch to battle screen
            _uiManager.ShowScreen("Battle");

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
            _uiManager.ShowScreen("Hub");
        }

        private void OnMilestoneEvaluated(MilestoneResult result)
        {
            Debug.Log($"[MainHub] Milestone evaluated: {result.Milestone.milestoneName} - {(result.Success ? "SUCCESS" : "FAILED")}");

            var milestoneScreen = _uiManager.GetScreen<MilestoneScreen>("Milestone");
            if (milestoneScreen != null)
            {
                milestoneScreen.ShowResult(result, OnMilestoneDismissed);
                _uiManager.ShowOverlay("Milestone");
            }
        }

        private void OnMilestoneDismissed()
        {
            _gameManager.DismissMilestone();
            _uiManager.HideOverlay("Milestone");
        }

        private void SetUIDocumentOnManager(UIManager manager, UIDocument doc)
        {
            var field = typeof(UIManager).GetField("uiDocument",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
                field.SetValue(manager, doc);
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
