using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using MagicGirls.Core.Enums;
using MagicGirls.Core.Models;
using MagicGirls.Runtime;
using MagicGirls.Runtime.Controllers;

namespace MagicGirls.UI.Screens
{
    public class BattleScreen : BaseScreen
    {
        private GameManager _gameManager;
        private UIManager _uiManager;
        private BattleController _battleController;

        private Label _battleTitle;
        private Label _roundLabel;
        private VisualElement _playerFighters;
        private VisualElement _enemyFighters;
        private VisualElement _battleLog;
        private ScrollView _battleLogScroll;
        private Button _btnSkip;
        private Button _btnContinue;

        private readonly Dictionary<string, FighterUI> _fighterUIs = new();

        private Action _onBattleDismissed;

        public BattleScreen(VisualTreeAsset template) : base(template)
        {
        }

        public void Initialize(GameManager gameManager, UIManager uiManager, BattleController battleController)
        {
            _gameManager = gameManager;
            _uiManager = uiManager;
            _battleController = battleController;

            _battleTitle = QL("battle-title");
            _roundLabel = QL("round-label");
            _playerFighters = Q("player-fighters");
            _enemyFighters = Q("enemy-fighters");
            _battleLog = Q("battle-log");
            _battleLogScroll = Root.Q<ScrollView>("battle-log-scroll");
            _btnSkip = QB("btn-skip");
            _btnContinue = QB("btn-continue");

            _btnSkip.clicked += OnSkipClicked;
            _btnContinue.clicked += OnContinueClicked;

            // Bind battle events
            _battleController.OnBattleStarted += OnBattleStarted;
            _battleController.OnActionResolved += OnActionResolved;
            _battleController.OnFighterHPChanged += OnFighterHPChanged;
            _battleController.OnRoundStarted += OnRoundStarted;
            _battleController.OnBattleEnded += OnBattleEnded;
        }

        public void SetDismissCallback(Action callback)
        {
            _onBattleDismissed = callback;
        }

        private void OnBattleStarted(BattleState state)
        {
            _battleLog.Clear();
            _fighterUIs.Clear();
            _btnSkip.style.display = DisplayStyle.Flex;
            _btnContinue.style.display = DisplayStyle.None;
            _roundLabel.text = "Round 0";

            // Build fighter portraits
            _playerFighters.Clear();
            foreach (var fighter in state.PlayerTeam)
            {
                var ui = CreateFighterUI(fighter, true);
                _playerFighters.Add(ui.Root);
                _fighterUIs[fighter.Id] = ui;
            }

            _enemyFighters.Clear();
            foreach (var fighter in state.EnemyTeam)
            {
                var ui = CreateFighterUI(fighter, false);
                _enemyFighters.Add(ui.Root);
                _fighterUIs[fighter.Id] = ui;
            }

            AddLogEntry("Battle begins!", "log-entry-round", "log-text-round");
        }

        private void OnRoundStarted(int round)
        {
            _roundLabel.text = $"Round {round}";
        }

        private void OnActionResolved(BattleLogEntry entry)
        {
            string entryClass = "log-entry";
            string textClass = "log-text";

            if (entry.NarrativeText.Contains("---"))
            {
                entryClass = "log-entry-round";
                textClass = "log-text-round";
            }
            else if (entry.IsCritical)
            {
                entryClass = "log-entry-crit";
                textClass = "log-text-crit";
            }
            else if (entry.TargetDefeated)
            {
                entryClass = "log-entry-defeat";
                textClass = "log-text-defeat";
            }
            else if (entry.Hit)
            {
                entryClass = "log-entry-hit";
            }
            else if (entry.AttackRoll != null && !entry.Hit)
            {
                entryClass = "log-entry-miss";
            }

            AddLogEntry(entry.NarrativeText, entryClass, textClass);
        }

        private void OnFighterHPChanged(BattleFighter fighter)
        {
            if (_fighterUIs.TryGetValue(fighter.Id, out var ui))
            {
                ui.UpdateHP(fighter.CurrentHP, fighter.MaxHP);
                if (!fighter.IsAlive)
                    ui.MarkDefeated();
            }
        }

        private void OnBattleEnded(BattleState state)
        {
            _btnSkip.style.display = DisplayStyle.None;
            _btnContinue.style.display = DisplayStyle.Flex;

            bool victory = state.Result == BattleResult.Victory;
            string resultText = victory
                ? $"VICTORY! +{state.TotalExperienceReward} XP, +{state.TotalGoldReward} Gold"
                : "DEFEAT... The girls retreat to recover.";

            string textClass = victory ? "log-text-crit" : "log-text-defeat";
            AddLogEntry(resultText, "log-entry-round", textClass);

            _battleTitle.text = victory ? "Victory!" : "Defeat...";
            _battleTitle.style.color = victory
                ? new Color(1f, 0.84f, 0f)
                : new Color(0.94f, 0.27f, 0.27f);
        }

        private void OnSkipClicked()
        {
            _battleController.SkipToEnd();
        }

        private void OnContinueClicked()
        {
            _onBattleDismissed?.Invoke();
        }

        private void AddLogEntry(string text, string entryClass, string textClass)
        {
            var entry = new VisualElement();
            entry.AddToClassList("log-entry");
            entry.AddToClassList(entryClass);

            var label = new Label(text);
            label.AddToClassList("log-text");
            label.AddToClassList(textClass);
            label.style.whiteSpace = WhiteSpace.Normal;
            entry.Add(label);

            _battleLog.Add(entry);

            // Auto-scroll to bottom
            _battleLogScroll.schedule.Execute(() =>
            {
                _battleLogScroll.scrollOffset = new Vector2(0, float.MaxValue);
            });
        }

        private FighterUI CreateFighterUI(BattleFighter fighter, bool isPlayer)
        {
            var card = new VisualElement();
            card.AddToClassList("fighter-card");

            var nameRow = new VisualElement();
            nameRow.AddToClassList("fighter-name-row");

            var nameLabel = new Label(fighter.Name);
            nameLabel.AddToClassList("fighter-name");
            nameLabel.AddToClassList(isPlayer ? "player-fighter-name" : "enemy-fighter-name");
            nameRow.Add(nameLabel);

            var hpText = new Label($"{fighter.CurrentHP}/{fighter.MaxHP}");
            hpText.AddToClassList("fighter-hp-text");
            nameRow.Add(hpText);

            card.Add(nameRow);

            // HP bar
            var hpBar = new VisualElement();
            hpBar.AddToClassList("fighter-hp-bar");

            var hpFill = new VisualElement();
            hpFill.AddToClassList("fighter-hp-fill");
            hpFill.AddToClassList(isPlayer ? "hp-fill-player" : "hp-fill-enemy");
            hpFill.style.width = new Length(100, LengthUnit.Percent);
            hpBar.Add(hpFill);

            card.Add(hpBar);

            // Stats row
            var statsRow = new VisualElement();
            statsRow.AddToClassList("fighter-stats-row");

            AddStatTag(statsRow, "AC", fighter.ArmorClass.ToString());
            AddStatTag(statsRow, "STR", fighter.Strength.ToString());
            AddStatTag(statsRow, "DEX", fighter.Dexterity.ToString());
            if (isPlayer)
                AddStatTag(statsRow, "MP", fighter.MagicPower.ToString());

            card.Add(statsRow);

            return new FighterUI
            {
                Root = card,
                HPText = hpText,
                HPFill = hpFill,
                IsPlayer = isPlayer
            };
        }

        private void AddStatTag(VisualElement container, string label, string value)
        {
            var tag = new Label($"{label}:{value}");
            tag.AddToClassList("fighter-stat");
            container.Add(tag);
        }

        public override void Show()
        {
            _battleTitle.text = "Monster Battle!";
            _battleTitle.style.color = new Color(1f, 0.41f, 0.71f);
        }

        public override void Hide() { }
        public override void BindData() { }

        private class FighterUI
        {
            public VisualElement Root;
            public Label HPText;
            public VisualElement HPFill;
            public bool IsPlayer;

            public void UpdateHP(int current, int max)
            {
                float pct = max > 0 ? (float)current / max * 100f : 0;
                HPFill.style.width = new Length(pct, LengthUnit.Percent);
                HPText.text = $"{current}/{max}";

                // Color based on HP percentage
                HPFill.RemoveFromClassList("hp-fill-low");
                HPFill.RemoveFromClassList("hp-fill-critical");

                if (pct <= 25f)
                    HPFill.AddToClassList("hp-fill-critical");
                else if (pct <= 50f)
                    HPFill.AddToClassList("hp-fill-low");
            }

            public void MarkDefeated()
            {
                Root.AddToClassList("fighter-card-defeated");
            }
        }
    }
}
