using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using MagicGirls.Core.Enums;
using MagicGirls.Core.Models;
using MagicGirls.Runtime;
using MagicGirls.UI.Widgets;

namespace MagicGirls.UI.Screens
{
    public class SchedulerScreen : BaseScreen
    {
        private GameManager _gameManager;
        private UIManager _uiManager;
        private string _selectedCharacterId;
        private readonly Dictionary<string, VisualElement> _characterCards = new();
        private readonly List<ScheduleSlotWidget> _scheduleSlots = new();

        // UI references
        private Label _timeLabel;
        private Label _goldLabel;
        // private VisualElement _characterList;
        private VisualElement _scheduleGrid;
        private Button _btnExecuteDay;
        private Button _backBtn;
        private Button _btnCalendar;

        private int _dayResultIndex;
        private List<SegmentResults> _pendingDayResults;

        public SchedulerScreen(VisualTreeAsset template) : base(template)
        {
        }

        public void Initialize(GameManager gameManager, UIManager uiManager)
        {
            _gameManager = gameManager;
            _uiManager = uiManager;

            CacheUIReferences();
            BindButtons();
            BindEvents();
            BuildScheduleGrid();
        }

        private void CacheUIReferences()
        {
            _timeLabel = QL("time-label");
            _goldLabel = QL("gold-label");
            // _characterList = Q("character-list");
            _scheduleGrid = Q("schedule-grid");
            _btnExecuteDay = QB("btn-execute-day");
            _btnCalendar = QB("btn-calendar");
            _backBtn = QB("btn-back");
        }

        private void BindButtons()
        {
            _btnExecuteDay.clicked += OnExecuteDayClicked;

            if (_btnCalendar != null)
                _btnCalendar.clicked += () => _uiManager.ShowScreen("Calendar");

            if(_backBtn != null)
                _backBtn.clicked += () => _uiManager.ShowScreen("Hub");
        }

        private void BindEvents()
        {
            _gameManager.OnSegmentChanged += RefreshAll;
            _gameManager.Economy.OnGoldChanged += gold => _goldLabel.text = $"Gold: {gold}";
            // _gameManager.Team.OnCharacterUpdated += _ => RefreshCharacterCards();
        }

        private void BuildScheduleGrid()
        {
            _scheduleGrid.Clear();
            _scheduleSlots.Clear();

            foreach (var character in _gameManager.Team.ActiveTeam)
            {
                var row = new VisualElement();
                row.AddToClassList("schedule-row");

                var nameLabel = new Label(character.Name);
                nameLabel.AddToClassList("schedule-char-name");
                row.Add(nameLabel);

                for (int seg = 0; seg < GameCalendar.SegmentsPerDay; seg++)
                {
                    var available = _gameManager.Schedule.GetAvailableActivities(seg);
                    var slot = new ScheduleSlotWidget(character.Id, seg, available);
                    slot.OnActivityChanged += OnScheduleSlotChanged;
                    _scheduleSlots.Add(slot);
                    row.Add(slot);
                }

                _scheduleGrid.Add(row);
            }
        }

        private void OnScheduleSlotChanged(string characterId, int segmentIndex, ActivityType activity)
        {
            _gameManager.Schedule.SetActivity(characterId, segmentIndex, activity);
        }

        public override void Show()
        {
            RefreshAll();
            BuildScheduleGrid();
        }

        public override void Hide() { }

        public override void BindData()
        {
            RefreshAll();
        }

        private void RefreshAll()
        {
            RefreshTimeDisplay();
            // RefreshCharacterCards();
            _goldLabel.text = $"Gold: {_gameManager.Economy.Gold}";
        }

        private void RefreshTimeDisplay()
        {
            _timeLabel.text = _gameManager.Time.GetStatusText();
        }

        // private void RefreshCharacterCards()
        // {
        //     _characterList.Clear();
        //     _characterCards.Clear();

        //     foreach (var character in _gameManager.Team.ActiveTeam)
        //     {
        //         var card = CreateCharacterCard(character);
        //         _characterCards[character.Id] = card;
        //         _characterList.Add(card);
        //     }
        // }

        // private VisualElement CreateCharacterCard(CharacterState character)
        // {
        //     var card = new VisualElement();
        //     card.AddToClassList("character-card");

        //     var nameLabel = new Label(character.Name);
        //     nameLabel.AddToClassList("character-name");
        //     card.Add(nameLabel);

        //     var levelLabel = new Label($"Level {character.Level} - XP: {(int)character.Experience}/{(int)character.ExperienceToNextLevel}");
        //     levelLabel.AddToClassList("character-level");
        //     card.Add(levelLabel);

        //     if (character.Traits.Count > 0)
        //     {
        //         var traitsLabel = new Label(string.Join(", ", character.Traits));
        //         traitsLabel.AddToClassList("character-traits");
        //         card.Add(traitsLabel);
        //     }

        //     // Vitals row
        //     var vitalsRow = new VisualElement();
        //     vitalsRow.AddToClassList("character-vitals-row");

        //     var hpLabel = new Label($"HP: {character.CurrentHP}/{character.MaxHP}");
        //     hpLabel.AddToClassList("hp-text");
        //     vitalsRow.Add(hpLabel);

        //     var staminaLabel = new Label($"STA: {character.CurrentStamina}/{character.MaxStamina}");
        //     staminaLabel.AddToClassList("stamina-text");
        //     vitalsRow.Add(staminaLabel);

        //     var moraleVal = character.GetMagicalStat(MagicalStat.Morale);
        //     var moraleLabel = new Label($"MOR: {moraleVal}");
        //     moraleLabel.AddToClassList("morale-text");
        //     vitalsRow.Add(moraleLabel);

        //     card.Add(vitalsRow);

        //     // Base stats row
        //     var statsRow = new VisualElement();
        //     statsRow.style.flexDirection = FlexDirection.Row;
        //     statsRow.style.flexWrap = Wrap.Wrap;
        //     statsRow.style.marginTop = 4;

        //     foreach (BaseStat stat in Enum.GetValues(typeof(BaseStat)))
        //     {
        //         var statLabel = new Label($"{stat}: {character.GetBaseStat(stat)}");
        //         statLabel.style.fontSize = 11;
        //         statLabel.style.color = new Color(0.63f, 0.63f, 0.69f);
        //         statLabel.style.marginRight = 8;
        //         statsRow.Add(statLabel);
        //     }
        //     card.Add(statsRow);

        //     // Magical stats row
        //     var magicRow = new VisualElement();
        //     magicRow.style.flexDirection = FlexDirection.Row;
        //     magicRow.style.flexWrap = Wrap.Wrap;
        //     magicRow.style.marginTop = 2;

        //     var mpLabel = new Label($"MP: {character.GetMagicalStat(MagicalStat.MagicPower)}");
        //     mpLabel.style.fontSize = 11;
        //     mpLabel.style.color = new Color(0.66f, 0.33f, 0.97f);
        //     mpLabel.style.marginRight = 8;
        //     magicRow.Add(mpLabel);

        //     var maLabel = new Label($"Aff: {character.GetMagicalStat(MagicalStat.MagicAffinity)}");
        //     maLabel.style.fontSize = 11;
        //     maLabel.style.color = new Color(0.66f, 0.33f, 0.97f);
        //     maLabel.style.marginRight = 8;
        //     magicRow.Add(maLabel);

        //     var corLabel = new Label($"Cor: {character.GetMagicalStat(MagicalStat.Corruption)}");
        //     corLabel.style.fontSize = 11;
        //     corLabel.style.color = new Color(0.94f, 0.27f, 0.27f);
        //     magicRow.Add(corLabel);

        //     card.Add(magicRow);

        //     // Click to view character details
        //     card.RegisterCallback<ClickEvent>(_ => SelectCharacter(character.Id));

        //     return card;
        // }

        // private void SelectCharacter(string characterId)
        // {
        //     if (_selectedCharacterId != null && _characterCards.TryGetValue(_selectedCharacterId, out var oldCard))
        //         oldCard.RemoveFromClassList("character-card-selected");

        //     _selectedCharacterId = characterId;

        //     if (_characterCards.TryGetValue(characterId, out var newCard))
        //         newCard.AddToClassList("character-card-selected");
        // }

        private void OnExecuteDayClicked()
        {
            if (_gameManager.IsDayInProgress) return;

            var results = _gameManager.ExecuteDay();
            if (results == null || results.Count == 0) return;

            // Show results one segment at a time
            _pendingDayResults = results;
            _dayResultIndex = 0;
            ShowNextSegmentResult();
        }

        private void ShowNextSegmentResult()
        {
            if (_pendingDayResults == null || _dayResultIndex >= _pendingDayResults.Count)
            {
                // All results shown, refresh
                _pendingDayResults = null;
                RefreshAll();
                BuildScheduleGrid();
                return;
            }

            var segResult = _pendingDayResults[_dayResultIndex];
            _dayResultIndex++;

            var resultsScreen = _uiManager.GetScreen<DayResultsScreen>("DayResults");
            if (resultsScreen != null)
            {
                resultsScreen.ShowSegmentResults(segResult, () =>
                {
                    _uiManager.HideOverlay(resultsScreen);
                    ShowNextSegmentResult();
                });
                _uiManager.ShowOverlay(resultsScreen);
            }
            else
            {
                // Fallback: skip overlay if DayResults screen not registered
                ShowNextSegmentResult();
            }
        }
    }
}
