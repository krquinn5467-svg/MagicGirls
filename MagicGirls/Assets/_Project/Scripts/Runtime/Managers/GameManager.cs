using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MagicGirls.Core.Enums;
using MagicGirls.Core.Models;
using MagicGirls.Core.Services;
using MagicGirls.Core.Systems;
using MagicGirls.Data;
using MagicGirls.Runtime.Controllers;

namespace MagicGirls.Runtime
{
    public class GameManager : MonoBehaviour
    {
        private TimeSystem _time;
        private TeamSystem _team;
        private EconomySystem _economy;
        private ScheduleSystem _schedule;
        private ActivityResolver _activityResolver;
        private ActivityRegistry _activityRegistry;
        private EncounterRegistry _encounterRegistry;
        private MilestoneRegistry _milestoneRegistry;
        private MilestoneEvaluator _milestoneEvaluator;
        private InventorySystem _inventory;
        private SaveSystem _saveSystem;
        private GameConfigSO _config;

        public event Action<ActivityResult, CharacterState> OnActivityCompleted;
        public event Action OnSegmentChanged;
        public event Action OnDayStarted;
        public event Action<int> OnWeekEnded;
        public event Action<int> OnMonthEnded;
        public event Action<List<SegmentResults>> OnDayResultsReady;
        public event Action<MonsterEncounterSO> OnBattleTriggered;
        public event Action<MilestoneResult> OnMilestoneEvaluated;

        public TimeSystem Time => _time;
        public TeamSystem Team => _team;
        public EconomySystem Economy => _economy;
        public ScheduleSystem Schedule => _schedule;
        public ActivityRegistry Activities => _activityRegistry;
        public EncounterRegistry Encounters => _encounterRegistry;
        public InventorySystem Inventory => _inventory;
        public SaveSystem SaveSystem => _saveSystem;

        public bool IsDayInProgress { get; private set; }
        public bool IsBattlePending { get; private set; }
        public bool IsMilestonePending { get; private set; }

        private int _totalWeeksCompleted;

        private void Awake()
        {
            _time = ServiceLocator.Get<TimeSystem>();
            _team = ServiceLocator.Get<TeamSystem>();
            _economy = ServiceLocator.Get<EconomySystem>();
            _schedule = ServiceLocator.Get<ScheduleSystem>();
            _activityResolver = ServiceLocator.Get<ActivityResolver>();
            _activityRegistry = ServiceLocator.Get<ActivityRegistry>();
            _encounterRegistry = ServiceLocator.Get<EncounterRegistry>();
            _milestoneRegistry = ServiceLocator.Get<MilestoneRegistry>();
            _milestoneEvaluator = ServiceLocator.Get<MilestoneEvaluator>();
            _inventory = ServiceLocator.Get<InventorySystem>();
            _saveSystem = ServiceLocator.Get<SaveSystem>();
            _config = ServiceLocator.Get<GameConfigSO>();

            _time.OnWeekCompleted += HandleWeekCompleted;
            _time.OnMonthCompleted += HandleMonthCompleted;
        }

        private void Start()
        {
            // Initialize schedule slots for all characters
            foreach (var character in _team.ActiveTeam)
                _schedule.InitializeForCharacter(character.Id);

            Debug.Log("[MagicGirls] GameManager started. " + _time.GetStatusText());
        }

        public bool CanPerformActivity(ActivityType activityType)
        {
            return _time.IsActivityAvailable(activityType);
        }

        /// <summary>
        /// Perform a single activity for a character in the current segment (immediate mode).
        /// </summary>
        public ActivityResult PerformActivity(string characterId, ActivityType activityType)
        {
            var activity = _activityRegistry.Get(activityType);
            if (activity == null)
            {
                Debug.LogError($"Activity {activityType} not found in registry!");
                return null;
            }

            if (!_time.IsActivityAvailable(activityType))
            {
                Debug.LogWarning($"Activity {activityType} is not available during {_time.CurrentSegment}");
                return null;
            }

            var character = _team.GetCharacter(characterId);
            if (character == null)
            {
                Debug.LogError($"Character {characterId} not found!");
                return null;
            }

            var result = _activityResolver.Resolve(character, activity);
            _team.ApplyActivityResult(characterId, result);

            if (result.GoldChange > 0)
                _economy.AddGold(result.GoldChange);

            OnActivityCompleted?.Invoke(result, character);
            return result;
        }

        /// <summary>
        /// Execute the full day based on the current schedule.
        /// Processes all 6 segments for all characters, collecting results.
        /// </summary>
        public List<SegmentResults> ExecuteDay()
        {
            if (IsDayInProgress) return null;
            IsDayInProgress = true;

            var allResults = new List<SegmentResults>();

            for (int seg = 0; seg < GameCalendar.SegmentsPerDay; seg++)
            {
                var segResults = new SegmentResults
                {
                    SegmentIndex = seg,
                    Segment = (TimeSegment)seg
                };

                foreach (var character in _team.ActiveTeam)
                {
                    var activityType = _schedule.GetActivity(character.Id, seg);
                    var activityDef = _activityRegistry.Get(activityType);

                    if (activityDef == null) continue;

                    // Validate night-only fallback
                    if (activityDef.nightOnly && !ScheduleSystem.IsNightSegment(seg))
                    {
                        activityType = ActivityType.Rest;
                        activityDef = _activityRegistry.Get(ActivityType.Rest);
                    }

                    var result = _activityResolver.Resolve(character, activityDef);
                    _team.ApplyActivityResult(character.Id, result);

                    if (result.GoldChange > 0)
                        _economy.AddGold(result.GoldChange);

                    segResults.Results.Add(new CharacterActivityResult
                    {
                        CharacterId = character.Id,
                        CharacterName = character.Name,
                        Activity = activityType,
                        Result = result
                    });

                    // Track for milestone evaluation
                    _milestoneEvaluator.RecordActivityCompletion(activityType);

                    OnActivityCompleted?.Invoke(result, character);
                }

                allResults.Add(segResults);
            }

            // Advance the full day (6 segments)
            for (int i = 0; i < GameCalendar.SegmentsPerDay; i++)
                _time.AdvanceSegment();

            // Reset schedule for next day
            _schedule.ResetDay();

            IsDayInProgress = false;
            OnSegmentChanged?.Invoke();
            OnDayResultsReady?.Invoke(allResults);
            OnDayStarted?.Invoke();

            Debug.Log($"[MagicGirls] Day executed. Now: {_time.GetStatusText()}");

            return allResults;
        }

        public void AdvanceSegment()
        {
            _time.AdvanceSegment();
            OnSegmentChanged?.Invoke();
            Debug.Log($"[MagicGirls] {_time.GetStatusText()}");
        }

        private void HandleWeekCompleted(int weekNumber)
        {
            _totalWeeksCompleted++;
            Debug.Log($"[MagicGirls] Week {weekNumber} completed! Battle time!");

            // Determine encounter
            var encounter = _encounterRegistry.GetEncounterForWeek(_totalWeeksCompleted);
            if (encounter != null)
            {
                IsBattlePending = true;
                OnBattleTriggered?.Invoke(encounter);
            }

            OnWeekEnded?.Invoke(weekNumber);
        }

        public float GetDifficultyScale()
        {
            return 1f + (_totalWeeksCompleted * _config.difficultyScalingPerWeek);
        }

        public void ApplyBattleResults(BattleState battleState)
        {
            IsBattlePending = false;

            // Track for milestone evaluation
            _milestoneEvaluator.RecordBattleResult(battleState);

            if (battleState.Result == BattleResult.Victory)
            {
                _economy.AddGold(battleState.TotalGoldReward);

                // Distribute XP to living characters
                float xpPerChar = battleState.TotalExperienceReward /
                    (float)_team.ActiveTeam.Count(c => c.CurrentHP > 0);

                foreach (var character in _team.ActiveTeam)
                {
                    if (character.CurrentHP <= 0) continue;
                    character.Experience += xpPerChar;
                }

                Debug.Log($"[MagicGirls] Battle won! +{battleState.TotalGoldReward}g, +{battleState.TotalExperienceReward}xp");
            }
            else
            {
                // Defeat: characters recover to 1 HP if knocked out
                foreach (var character in _team.ActiveTeam)
                {
                    if (character.CurrentHP <= 0)
                        character.CurrentHP = 1;
                }
                Debug.Log("[MagicGirls] Battle lost. Team recovers to fight another day.");
            }

            // Sync HP from battle fighters back to character state
            foreach (var fighter in battleState.PlayerTeam)
            {
                var character = _team.GetCharacter(fighter.CharacterId);
                if (character != null)
                {
                    character.CurrentHP = Math.Max(1, fighter.CurrentHP);
                }
            }
        }

        private void HandleMonthCompleted(int monthNumber)
        {
            Debug.Log($"[MagicGirls] Month {monthNumber} completed! Milestone check!");

            // Evaluate milestone
            var milestone = _milestoneRegistry.GetMilestoneForMonth(monthNumber);
            if (milestone != null)
            {
                IsMilestonePending = true;
                var result = _milestoneEvaluator.Evaluate(milestone);
                _milestoneEvaluator.ApplyResult(result);
                OnMilestoneEvaluated?.Invoke(result);
                Debug.Log($"[MagicGirls] Milestone '{milestone.milestoneName}': {(result.Success ? "COMPLETE" : "FAILED")}");
            }

            // Reset milestone tracking for next month
            _milestoneEvaluator.ResetMonth();

            // Auto-save at end of month
            _saveSystem.Save(_totalWeeksCompleted);

            OnMonthEnded?.Invoke(monthNumber);
        }

        public void DismissMilestone()
        {
            IsMilestonePending = false;
        }

        public void SaveGame()
        {
            _saveSystem.Save(_totalWeeksCompleted);
        }

        private void OnDestroy()
        {
            if (_time != null)
            {
                _time.OnWeekCompleted -= HandleWeekCompleted;
                _time.OnMonthCompleted -= HandleMonthCompleted;
            }
        }
    }

    /// <summary>
    /// Results for a single time segment across all characters.
    /// </summary>
    public class SegmentResults
    {
        public int SegmentIndex { get; set; }
        public TimeSegment Segment { get; set; }
        public List<CharacterActivityResult> Results { get; set; } = new();
    }

    public class CharacterActivityResult
    {
        public string CharacterId { get; set; }
        public string CharacterName { get; set; }
        public ActivityType Activity { get; set; }
        public ActivityResult Result { get; set; }
    }
}
