using System;
using MagicGirls.Core.Enums;
using MagicGirls.Core.Models;

namespace MagicGirls.Core.Services
{
    public class DiceService
    {
        private readonly Random _random;

        public DiceService(int? seed = null)
        {
            _random = seed.HasValue ? new Random(seed.Value) : new Random();
        }

        public DiceRollResult Roll(DieType die, int count = 1, int modifier = 0)
        {
            int dieSides = (int)die;
            int[] rolls = new int[count];
            int sum = 0;

            for (int i = 0; i < count; i++)
            {
                rolls[i] = _random.Next(1, dieSides + 1);
                sum += rolls[i];
            }

            int total = sum + modifier;

            bool isCritical = die == DieType.D20 && count == 1 && rolls[0] == 20;
            bool isFumble = die == DieType.D20 && count == 1 && rolls[0] == 1;

            return new DiceRollResult
            {
                Die = die,
                Rolls = rolls,
                Modifier = modifier,
                Total = total,
                IsCritical = isCritical,
                IsFumble = isFumble
            };
        }

        public DiceRollResult RollWithAdvantage(DieType die, int modifier = 0)
        {
            var roll1 = Roll(die, 1, 0);
            var roll2 = Roll(die, 1, 0);
            var best = roll1.Rolls[0] >= roll2.Rolls[0] ? roll1 : roll2;
            best.Modifier = modifier;
            best.Total = best.Rolls[0] + modifier;
            return best;
        }

        public DiceRollResult RollWithDisadvantage(DieType die, int modifier = 0)
        {
            var roll1 = Roll(die, 1, 0);
            var roll2 = Roll(die, 1, 0);
            var worst = roll1.Rolls[0] <= roll2.Rolls[0] ? roll1 : roll2;
            worst.Modifier = modifier;
            worst.Total = worst.Rolls[0] + modifier;
            return worst;
        }

        public bool CheckDC(DiceRollResult roll, int difficultyClass)
        {
            return roll.Total >= difficultyClass;
        }
    }
}
