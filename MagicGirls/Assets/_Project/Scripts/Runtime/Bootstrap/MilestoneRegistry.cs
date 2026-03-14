using System.Linq;
using MagicGirls.Data;

namespace MagicGirls.Runtime
{
    public class MilestoneRegistry
    {
        private readonly MilestoneSO[] _milestones;

        public MilestoneRegistry(MilestoneSO[] milestones)
        {
            _milestones = milestones;
        }

        public MilestoneSO GetMilestoneForMonth(int monthNumber)
        {
            // First try specific month milestone
            var specific = _milestones.FirstOrDefault(m => m.targetMonth == monthNumber);
            if (specific != null) return specific;

            // Fallback to generic (targetMonth == 0)
            return _milestones.FirstOrDefault(m => m.targetMonth == 0);
        }
    }
}
