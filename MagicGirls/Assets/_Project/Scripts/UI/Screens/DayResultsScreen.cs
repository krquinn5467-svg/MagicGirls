using System;
using UnityEngine;
using UnityEngine.UIElements;
using MagicGirls.Core.Enums;
using MagicGirls.Core.Models;
using MagicGirls.Core.Systems;
using MagicGirls.Runtime;

namespace MagicGirls.UI.Screens
{
    public class DayResultsScreen : BaseScreen
    {
        private Action _onContinue;

        public DayResultsScreen(VisualTreeAsset template) : base(template)
        {
            BuildUI();
        }

        private VisualElement _panel;
        private Label _titleLabel;
        private VisualElement _resultsContainer;
        private Button _btnContinue;

        private UIManager _uiManager;

        private void BuildUI()
        {
            Root.AddToClassList("overlay");
        }

        public void Initialize(UIManager uiManager)
        {
            _uiManager = uiManager;

            CacheUIReferences();
            BindButtons();
        }

        private void CacheUIReferences()
        {
            _btnContinue = QB("btn-continue");
            _resultsContainer = Q("results-container");
            _titleLabel = QL("results-title");
        }

        private void BindButtons()
        {
            _btnContinue.clicked += () => _onContinue?.Invoke();
        }


        public void ShowSegmentResults(SegmentResults segResults, Action onContinue)
        {
            _onContinue = onContinue;
            _titleLabel.text = $"{ScheduleSystem.GetSegmentLabel(segResults.SegmentIndex)} Results";
            _resultsContainer.Clear();

            foreach (var charResult in segResults.Results)
            {
                var charSection = new VisualElement();
                charSection.style.marginBottom = 12;
                charSection.style.paddingBottom = 8;
                charSection.style.borderBottomWidth = 1;
                charSection.style.borderBottomColor = new Color(1, 1, 1, 0.1f);

                // Character name + activity
                var headerRow = new VisualElement();
                headerRow.style.flexDirection = FlexDirection.Row;
                headerRow.style.justifyContent = Justify.SpaceBetween;

                var nameLabel = new Label(charResult.CharacterName);
                nameLabel.style.fontSize = 16;
                nameLabel.style.color = new Color(1f, 0.41f, 0.71f); // pink
                nameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                headerRow.Add(nameLabel);

                var actLabel = new Label(charResult.Activity.ToString());
                actLabel.style.fontSize = 14;
                actLabel.style.color = GetActivityColor(charResult.Activity);
                actLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                headerRow.Add(actLabel);

                charSection.Add(headerRow);

                // Narrative
                var narrative = new Label(charResult.Result.NarrativeText);
                narrative.style.fontSize = 13;
                narrative.style.color = new Color(0.92f, 0.92f, 0.92f);
                narrative.style.whiteSpace = WhiteSpace.Normal;
                narrative.style.marginTop = 4;
                charSection.Add(narrative);

                // Stat changes on one line
                if (charResult.Result.StatChanges.Count > 0)
                {
                    var statsLine = new VisualElement();
                    statsLine.style.flexDirection = FlexDirection.Row;
                    statsLine.style.flexWrap = Wrap.Wrap;
                    statsLine.style.marginTop = 4;

                    foreach (var change in charResult.Result.StatChanges)
                    {
                        if (change.Amount == 0) continue;
                        string sign = change.Amount > 0 ? "+" : "";
                        var tag = new Label($"{change.StatName} {sign}{change.Amount}");
                        tag.style.fontSize = 12;
                        tag.style.marginRight = 8;
                        tag.style.color = change.Amount > 0
                            ? new Color(0.13f, 0.77f, 0.22f)
                            : new Color(0.94f, 0.27f, 0.27f);
                        tag.style.unityFontStyleAndWeight = FontStyle.Bold;
                        statsLine.Add(tag);
                    }

                    charSection.Add(statsLine);
                }

                // Other effects
                var effectsLine = new VisualElement();
                effectsLine.style.flexDirection = FlexDirection.Row;
                effectsLine.style.flexWrap = Wrap.Wrap;
                effectsLine.style.marginTop = 2;

                if (charResult.Result.StaminaChange != 0)
                    AddEffectTag(effectsLine, "STA", charResult.Result.StaminaChange, "#22C55E");
                if (charResult.Result.MoraleChange != 0)
                    AddEffectTag(effectsLine, "MOR", charResult.Result.MoraleChange, "#3B82F6");
                if (charResult.Result.GoldChange != 0)
                    AddEffectTag(effectsLine, "Gold", charResult.Result.GoldChange, "#FFD700");
                if (charResult.Result.ExperienceGain > 0)
                    AddEffectTag(effectsLine, "XP", (int)charResult.Result.ExperienceGain, "#F59E0B");

                if (effectsLine.childCount > 0)
                    charSection.Add(effectsLine);

                if (charResult.Result.InjuryOccurred)
                {
                    var injury = new Label("Injured!");
                    injury.style.fontSize = 13;
                    injury.style.color = new Color(0.94f, 0.27f, 0.27f);
                    injury.style.unityFontStyleAndWeight = FontStyle.Bold;
                    injury.style.marginTop = 2;
                    charSection.Add(injury);
                }

                _resultsContainer.Add(charSection);
            }
        }

        private void AddEffectTag(VisualElement container, string label, int value, string hexColor)
        {
            string sign = value > 0 ? "+" : "";
            var tag = new Label($"{label} {sign}{value}");
            tag.style.fontSize = 11;
            tag.style.marginRight = 8;
            if (ColorUtility.TryParseHtmlString(hexColor, out var color))
                tag.style.color = color;
            container.Add(tag);
        }

        private Color GetActivityColor(ActivityType activity)
        {
            return activity switch
            {
                ActivityType.Rest => new Color(0.13f, 0.77f, 0.22f),
                ActivityType.Train => new Color(0.94f, 0.27f, 0.27f),
                ActivityType.School => new Color(0.23f, 0.51f, 0.96f),
                ActivityType.DungeonCrawl => new Color(0.66f, 0.33f, 0.97f),
                ActivityType.Recreation => new Color(0.96f, 0.62f, 0.04f),
                ActivityType.Work => new Color(1f, 0.84f, 0f),
                _ => Color.white
            };
        }

        public override void OnShow() { }
        public override void OnHide() { }
        public override void BindData() { }
    }
}
