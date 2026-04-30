using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using MagicGirls.Core.Enums;
using MagicGirls.Core.Models;
using MagicGirls.Core.Services;
using MagicGirls.Runtime;
using MagicGirls.UI.Widgets;

namespace MagicGirls.UI.Screens
{
    public class CharacterScreen : BaseScreen
    {
        private GameManager _gameManager;
        private UIManager _uiManager;
        private string _selectedCharId;

        private VisualElement _charTabs;
        private Label _charName;
        private Label _charLevel;
        private Label _charTraits;
        private Label _charBackstory;
        private VisualElement _vitalsContainer;
        private VisualElement _baseStatsContainer;
        private VisualElement _magicStatsContainer;
        private VisualElement _xpContainer;
        private Button _btnBack;

        private readonly Dictionary<string, Button> _tabButtons = new();

        public CharacterScreen(VisualTreeAsset template) : base(template)
        {
        }

        public void Initialize(GameManager gameManager, UIManager uiManager)
        {
            _gameManager = gameManager;
            _uiManager = uiManager;

            _charTabs = Q("char-tabs");
            _charName = QL("char-name");
            _charLevel = QL("char-level");
            _charTraits = QL("char-traits");
            _charBackstory = QL("char-backstory");
            _vitalsContainer = Q("vitals-container");
            _baseStatsContainer = Q("base-stats-container");
            _magicStatsContainer = Q("magic-stats-container");
            _xpContainer = Q("xp-container");
            _btnBack = QB("btn-back");

            _btnBack.clicked += () => _uiManager.ShowScreen("Hub");
        }

        public override void Show()
        {
            BuildTabs();

            // Select first character or previously selected
            if (string.IsNullOrEmpty(_selectedCharId) && _gameManager.Team.ActiveTeam.Count > 0)
                _selectedCharId = _gameManager.Team.ActiveTeam[0].Id;

            SelectCharacter(_selectedCharId);
        }

        public override void Hide() { }
        public override void BindData() { }

        private void BuildTabs()
        {
            _charTabs.Clear();
            _tabButtons.Clear();

            foreach (var character in _gameManager.Team.ActiveTeam)
            {
                var tab = new Button();
                tab.text = character.Name;
                tab.AddToClassList("char-tab");
                var charId = character.Id;
                tab.clicked += () => SelectCharacter(charId);
                _charTabs.Add(tab);
                _tabButtons[character.Id] = tab;
            }
        }

        private void SelectCharacter(string charId)
        {
            _selectedCharId = charId;

            // Update tab styling
            foreach (var kvp in _tabButtons)
            {
                kvp.Value.RemoveFromClassList("char-tab-active");
                if (kvp.Key == charId)
                    kvp.Value.AddToClassList("char-tab-active");
            }

            var character = _gameManager.Team.GetCharacter(charId);
            if (character == null) return;

            PopulateCharacterDetails(character);
        }

        private void PopulateCharacterDetails(CharacterState character)
        {
            _charName.text = character.Name;
            _charLevel.text = $"Level {character.Level}";
            _charTraits.text = character.Traits.Count > 0
                ? string.Join(", ", character.Traits)
                : "No personality traits";
            _charBackstory.text = "";

            // Vitals
            _vitalsContainer.Clear();
            _vitalsContainer.Add(new StatBarWidget("HP", character.CurrentHP, character.MaxHP, "#EF4444"));
            _vitalsContainer.Add(new StatBarWidget("Stamina", character.CurrentStamina, character.MaxStamina, "#22C55E"));
            var moraleBar = new StatBarWidget("Morale", character.GetMagicalStat(MagicalStat.Morale), 30, "#3B82F6");
            _vitalsContainer.Add(moraleBar);

            // Base stats
            _baseStatsContainer.Clear();
            foreach (BaseStat stat in Enum.GetValues(typeof(BaseStat)))
            {
                int value = character.GetBaseStat(stat);
                int mod = StatCalculator.GetModifier(value);
                string modStr = mod >= 0 ? $"+{mod}" : mod.ToString();

                var row = new VisualElement();
                row.style.flexDirection = FlexDirection.Row;
                row.style.justifyContent = Justify.SpaceBetween;
                row.style.paddingTop = 4;
                row.style.paddingBottom = 4;
                row.style.borderBottomWidth = 1;
                row.style.borderBottomColor = new Color(1, 1, 1, 0.05f);

                var nameLabel = new Label(stat.ToString());
                nameLabel.style.fontSize = 15;
                nameLabel.style.color = Color.white;
                nameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                nameLabel.style.width = 60;
                row.Add(nameLabel);

                var valueLabel = new Label(value.ToString());
                valueLabel.style.fontSize = 18;
                valueLabel.style.color = GetStatColor(value);
                valueLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                row.Add(valueLabel);

                var modLabel = new Label($"({modStr})");
                modLabel.style.fontSize = 14;
                modLabel.style.color = mod >= 0 ? new Color(0.13f, 0.77f, 0.22f) : new Color(0.94f, 0.27f, 0.27f);
                row.Add(modLabel);

                _baseStatsContainer.Add(row);
            }

            // Magical stats
            _magicStatsContainer.Clear();
            foreach (MagicalStat stat in Enum.GetValues(typeof(MagicalStat)))
            {
                int value = character.GetMagicalStat(stat);
                string color = stat == MagicalStat.Corruption ? "#EF4444" : "#A855F7";

                var bar = new StatBarWidget(FormatMagicalStatName(stat), value, 30, color);
                _magicStatsContainer.Add(bar);
            }

            // XP
            _xpContainer.Clear();
            var xpBar = new StatBarWidget("XP", (int)character.Experience, (int)character.ExperienceToNextLevel, "#F59E0B");
            _xpContainer.Add(xpBar);

            var xpLabel = new Label($"{(int)character.Experience} / {(int)character.ExperienceToNextLevel} to next level");
            xpLabel.style.fontSize = 12;
            xpLabel.style.color = new Color(0.63f, 0.63f, 0.69f);
            xpLabel.style.marginTop = 4;
            _xpContainer.Add(xpLabel);
        }

        private Color GetStatColor(int value)
        {
            if (value >= 16) return new Color(0.13f, 0.77f, 0.22f); // green
            if (value >= 12) return new Color(0.23f, 0.51f, 0.96f); // blue
            if (value >= 10) return Color.white;
            return new Color(0.94f, 0.27f, 0.27f); // red
        }

        private string FormatMagicalStatName(MagicalStat stat)
        {
            return stat switch
            {
                MagicalStat.MagicPower => "Magic Power",
                MagicalStat.MagicAffinity => "Magic Affinity",
                MagicalStat.Corruption => "Corruption",
                MagicalStat.Morale => "Morale",
                _ => stat.ToString()
            };
        }
    }
}
