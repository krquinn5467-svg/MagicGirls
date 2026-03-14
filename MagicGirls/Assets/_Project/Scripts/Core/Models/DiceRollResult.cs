using MagicGirls.Core.Enums;

namespace MagicGirls.Core.Models
{
    public class DiceRollResult
    {
        public DieType Die { get; set; }
        public int[] Rolls { get; set; }
        public int Modifier { get; set; }
        public int Total { get; set; }
        public bool IsCritical { get; set; }
        public bool IsFumble { get; set; }

        public override string ToString()
        {
            string rollsStr = string.Join(", ", Rolls);
            string modStr = Modifier >= 0 ? $"+{Modifier}" : Modifier.ToString();
            string extra = IsCritical ? " CRIT!" : IsFumble ? " FUMBLE!" : "";
            return $"{Rolls.Length}d{(int)Die} [{rollsStr}] {modStr} = {Total}{extra}";
        }
    }
}
