using System;
using UnityEngine;
using UnityEngine.UIElements;
using MagicGirls.Core.Services;

namespace MagicGirls.UI.Screens
{
    public class MilestoneScreen : BaseScreen
    {
        private Action _onDismiss;
        private VisualElement _panel;
        private Label _titleLabel;
        private Label _descLabel;
        private Label _resultLabel;
        private VisualElement _conditionsContainer;
        private VisualElement _rewardsContainer;
        private Button _btnDismiss;

        public MilestoneScreen(VisualTreeAsset template) : base(template)
        {
            BuildUI();
        }

        private void BuildUI()
        {
            //todo: this will have to be fixed to work with a template
            Root.AddToClassList("overlay");

            _panel = new VisualElement();
            _panel.AddToClassList("overlay-panel");
            _panel.style.minWidth = 450;
            _panel.style.maxWidth = 600;
            _panel.style.maxHeight = 500;
            _panel.style.paddingTop = 20;
            _panel.style.paddingBottom = 20;
            _panel.style.paddingLeft = 24;
            _panel.style.paddingRight = 24;

            _titleLabel = new Label("Monthly Milestone");
            _titleLabel.AddToClassList("header-text");
            _titleLabel.style.fontSize = 22;
            _titleLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            _panel.Add(_titleLabel);

            _descLabel = new Label();
            _descLabel.style.fontSize = 14;
            _descLabel.style.color = new Color(0.85f, 0.85f, 0.85f);
            _descLabel.style.whiteSpace = WhiteSpace.Normal;
            _descLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            _descLabel.style.marginTop = 8;
            _descLabel.style.marginBottom = 16;
            _panel.Add(_descLabel);

            _resultLabel = new Label();
            _resultLabel.style.fontSize = 20;
            _resultLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            _resultLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            _resultLabel.style.marginBottom = 12;
            _panel.Add(_resultLabel);

            // Conditions section
            var condHeader = new Label("Conditions");
            condHeader.style.fontSize = 16;
            condHeader.style.unityFontStyleAndWeight = FontStyle.Bold;
            condHeader.style.color = new Color(0.75f, 0.6f, 0.9f);
            condHeader.style.marginBottom = 6;
            _panel.Add(condHeader);

            _conditionsContainer = new VisualElement();
            _conditionsContainer.style.marginBottom = 16;
            _panel.Add(_conditionsContainer);

            // Rewards/penalty section
            _rewardsContainer = new VisualElement();
            _rewardsContainer.style.marginBottom = 16;
            _panel.Add(_rewardsContainer);

            _btnDismiss = new Button();
            _btnDismiss.text = "Continue";
            _btnDismiss.AddToClassList("btn");
            _btnDismiss.AddToClassList("btn-primary");
            _btnDismiss.style.alignSelf = Align.Center;
            _btnDismiss.style.width = 160;
            _btnDismiss.style.height = 40;
            _btnDismiss.style.marginTop = 8;
            _btnDismiss.clicked += () => _onDismiss?.Invoke();
            _panel.Add(_btnDismiss);

            Root.Add(_panel);
        }

        public void ShowResult(MilestoneResult result, Action onDismiss)
        {
            _onDismiss = onDismiss;
            _titleLabel.text = result.Milestone.milestoneName;
            _descLabel.text = result.Milestone.description;

            if (result.Success)
            {
                _resultLabel.text = "MILESTONE COMPLETE!";
                _resultLabel.style.color = new Color(0.13f, 0.77f, 0.22f);
            }
            else
            {
                _resultLabel.text = "MILESTONE FAILED";
                _resultLabel.style.color = new Color(0.94f, 0.27f, 0.27f);
            }

            // Conditions
            _conditionsContainer.Clear();
            foreach (var condDesc in result.ConditionResults)
            {
                var label = new Label(condDesc);
                label.style.fontSize = 13;
                label.style.marginBottom = 4;
                label.style.color = condDesc.StartsWith("[DONE]")
                    ? new Color(0.13f, 0.77f, 0.22f)
                    : new Color(0.94f, 0.27f, 0.27f);
                _conditionsContainer.Add(label);
            }

            // Rewards or penalty
            _rewardsContainer.Clear();
            if (result.Success)
            {
                var header = new Label("Rewards");
                header.style.fontSize = 16;
                header.style.unityFontStyleAndWeight = FontStyle.Bold;
                header.style.color = new Color(1f, 0.84f, 0f);
                header.style.marginBottom = 6;
                _rewardsContainer.Add(header);

                if (result.GoldReward > 0)
                    AddRewardLine($"+{result.GoldReward} Gold", "#FFD700");
                if (result.ExperienceReward > 0)
                    AddRewardLine($"+{result.ExperienceReward} XP (shared)", "#F59E0B");
                if (!string.IsNullOrEmpty(result.Milestone.rewardDescription))
                    AddRewardLine(result.Milestone.rewardDescription, "#FFFFFF");
            }
            else
            {
                var header = new Label("Penalty");
                header.style.fontSize = 16;
                header.style.unityFontStyleAndWeight = FontStyle.Bold;
                header.style.color = new Color(0.94f, 0.27f, 0.27f);
                header.style.marginBottom = 6;
                _rewardsContainer.Add(header);

                if (result.MoralePenalty > 0)
                    AddRewardLine($"-{result.MoralePenalty} Morale (all characters)", "#F04444");
                if (!string.IsNullOrEmpty(result.Milestone.penaltyDescription))
                    AddRewardLine(result.Milestone.penaltyDescription, "#CCCCCC");
            }
        }

        private void AddRewardLine(string text, string hexColor)
        {
            var label = new Label(text);
            label.style.fontSize = 13;
            label.style.marginBottom = 2;
            if (ColorUtility.TryParseHtmlString(hexColor, out var color))
                label.style.color = color;
            _rewardsContainer.Add(label);
        }

        public override void Show() { }
        public override void Hide() { }
        public override void BindData() { }
    }
}
