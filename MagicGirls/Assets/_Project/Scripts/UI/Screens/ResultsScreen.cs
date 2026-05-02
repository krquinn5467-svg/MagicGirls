using UnityEngine.UIElements;
using MagicGirls.Core.Enums;
using MagicGirls.Core.Models;
using MagicGirls.Runtime;
using Unity.Android.Gradle.Manifest;
using MagicGirls.Runtime.Managers;

namespace MagicGirls.UI.Screens
{
    public class ResultsScreen : BaseScreen, IUsesScreenManager
    {
        private ScreenManager _screenManager;

        private Label _titleLabel;
        private Label _narrativeLabel;
        private VisualElement _diceContainer;
        private VisualElement _statChangesContainer;
        private VisualElement _otherChangesContainer;
        private Button _btnContinue;

        public ResultsScreen(VisualTreeAsset template) : base(template)
        {
        }

        public override void Initialize()
        {
            _titleLabel = QL("results-title");
            _narrativeLabel = QL("narrative-text");
            _diceContainer = Q("dice-rolls-container");
            _statChangesContainer = Q("stat-changes-container");
            _otherChangesContainer = Q("other-changes-container");
            _btnContinue = QB("btn-continue");

            _btnContinue.clicked += OnContinueClicked;
        }

        public void SetScreenManager(ScreenManager sm)
        {
            _screenManager = sm;
        }

        public void ShowResult(ActivityResult result, CharacterState character)
        {
            _titleLabel.text = $"{result.Activity} Results";
            _narrativeLabel.text = result.NarrativeText;

            PopulateDiceRolls(result);
            PopulateStatChanges(result);
            PopulateOtherChanges(result);
        }

        private void PopulateDiceRolls(ActivityResult result)
        {
            _diceContainer.Clear();

            foreach (var roll in result.AllRolls)
            {
                var row = new VisualElement();
                row.AddToClassList("dice-roll-row");

                var label = new Label($"{roll.Rolls.Length}d{(int)roll.Die}");
                label.AddToClassList("dice-roll-label");
                row.Add(label);

                var value = new Label(roll.ToString());
                value.AddToClassList("dice-roll-value");
                if (roll.IsCritical) value.style.color = new UnityEngine.Color(0.13f, 0.77f, 0.13f);
                if (roll.IsFumble) value.style.color = new UnityEngine.Color(0.94f, 0.27f, 0.27f);
                row.Add(value);

                _diceContainer.Add(row);
            }
        }

        private void PopulateStatChanges(ActivityResult result)
        {
            _statChangesContainer.Clear();

            foreach (var change in result.StatChanges)
            {
                if (change.Amount == 0) continue;

                var row = new VisualElement();
                row.AddToClassList("stat-change-row");

                var label = new Label(change.StatName);
                label.AddToClassList("stat-change-label");
                row.Add(label);

                string sign = change.Amount > 0 ? "+" : "";
                var value = new Label($"{sign}{change.Amount}");
                value.AddToClassList("stat-change-value");
                value.AddToClassList(change.Amount > 0 ? "stat-change-positive" : "stat-change-negative");
                row.Add(value);

                _statChangesContainer.Add(row);
            }
        }

        private void PopulateOtherChanges(ActivityResult result)
        {
            _otherChangesContainer.Clear();

            if (result.StaminaChange != 0)
                AddOtherRow("Stamina", result.StaminaChange);

            if (result.MoraleChange != 0)
                AddOtherRow("Morale", result.MoraleChange);

            if (result.GoldChange != 0)
                AddOtherRow("Gold", result.GoldChange, true);

            if (result.ExperienceGain > 0)
                AddOtherRow("Experience", (int)result.ExperienceGain);

            if (result.InjuryOccurred)
            {
                var warning = new Label("Injury sustained!");
                warning.AddToClassList("injury-warning");
                _otherChangesContainer.Add(warning);
            }
        }

        private void AddOtherRow(string label, int value, bool isGold = false)
        {
            var row = new VisualElement();
            row.AddToClassList("other-change-row");

            var labelEl = new Label(label);
            labelEl.AddToClassList("other-change-label");
            row.Add(labelEl);

            string sign = value > 0 ? "+" : "";
            var valueEl = new Label($"{sign}{value}");
            valueEl.AddToClassList("other-change-value");

            if (isGold)
                valueEl.style.color = new UnityEngine.Color(1f, 0.84f, 0f);
            else if (value > 0)
                valueEl.AddToClassList("stat-change-positive");
            else
                valueEl.AddToClassList("stat-change-negative");

            row.Add(valueEl);
            _otherChangesContainer.Add(row);
        }

        private void OnContinueClicked()
        {
            _screenManager.HideOverlay(this);
        }

        public override void Show() { }
        public override void Hide() { }
        public override void BindData() { }
    }
}
