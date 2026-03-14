using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MagicGirls.Core.Enums;
using MagicGirls.Core.Models;
using MagicGirls.Core.Services;
using MagicGirls.Core.Systems;
using MagicGirls.Data;

namespace MagicGirls.Runtime.Controllers
{
    public class BattleController : MonoBehaviour
    {
        [Header("Timing")]
        [SerializeField] private float roundDelay = 1.5f;
        [SerializeField] private float actionDelay = 0.8f;
        [SerializeField] private int maxRounds = 20;

        private BattleResolver _resolver;
        private BattleState _battleState;
        private bool _battleRunning;

        public BattleState CurrentBattle => _battleState;
        public bool IsBattleRunning => _battleRunning;

        // Events for UI binding
        public event Action<BattleState> OnBattleStarted;
        public event Action<BattleLogEntry> OnActionResolved;
        public event Action<BattleFighter> OnFighterHPChanged;
        public event Action<int> OnRoundStarted;
        public event Action<BattleState> OnBattleEnded;

        private void Start()
        {
            var diceService = ServiceLocator.Get<DiceService>();
            _resolver = new BattleResolver(diceService);
        }

        public void StartBattle(List<CharacterState> team, MonsterEncounterSO encounter, float difficultyScale = 1f)
        {
            if (_battleRunning) return;

            _battleState = _resolver.InitBattle(team, encounter, difficultyScale);
            OnBattleStarted?.Invoke(_battleState);

            StartCoroutine(RunBattle());
        }

        private IEnumerator RunBattle()
        {
            _battleRunning = true;

            yield return new WaitForSeconds(1f);

            while (!_resolver.IsBattleOver(_battleState) && _battleState.CurrentRound < maxRounds)
            {
                int prevLogCount = _battleState.Log.Count;

                _resolver.ExecuteRound(_battleState);

                OnRoundStarted?.Invoke(_battleState.CurrentRound);

                // Fire events for each new log entry with delays
                for (int i = prevLogCount; i < _battleState.Log.Count; i++)
                {
                    var entry = _battleState.Log[i];
                    OnActionResolved?.Invoke(entry);

                    // Notify HP changes for fighters
                    if (entry.Hit && entry.TargetName != null)
                    {
                        var target = _battleState.AllFighters.FirstOrDefault(f => f.Name == entry.TargetName);
                        if (target != null)
                            OnFighterHPChanged?.Invoke(target);
                    }

                    yield return new WaitForSeconds(actionDelay);
                }

                yield return new WaitForSeconds(roundDelay - actionDelay);
            }

            // Force end if max rounds reached
            if (_battleState.Result == BattleResult.InProgress)
            {
                _battleState.Result = BattleResult.Defeat;
                _battleState.Log.Add(new BattleLogEntry
                {
                    Round = _battleState.CurrentRound,
                    NarrativeText = "The battle dragged on too long... The girls retreat."
                });
            }

            _battleRunning = false;
            OnBattleEnded?.Invoke(_battleState);
        }

        public void SkipToEnd()
        {
            if (!_battleRunning || _battleState == null) return;

            StopAllCoroutines();

            while (!_resolver.IsBattleOver(_battleState) && _battleState.CurrentRound < maxRounds)
            {
                _resolver.ExecuteRound(_battleState);
            }

            if (_battleState.Result == BattleResult.InProgress)
                _battleState.Result = BattleResult.Defeat;

            _battleRunning = false;
            OnBattleEnded?.Invoke(_battleState);
        }
    }
}
