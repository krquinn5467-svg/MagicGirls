using System.Collections.Generic;
using System.Linq;
using MagicGirls.Data;

namespace MagicGirls.Runtime
{
    public class EncounterRegistry
    {
        private readonly List<MonsterEncounterSO> _encounters;

        public EncounterRegistry(MonsterEncounterSO[] encounters)
        {
            _encounters = new List<MonsterEncounterSO>(encounters);
        }

        public MonsterEncounterSO GetEncounterForWeek(int weekNumber)
        {
            // Find encounters valid for this week
            var valid = _encounters.Where(e =>
                !e.isBoss &&
                weekNumber >= e.minWeek &&
                (e.maxWeek == 0 || weekNumber <= e.maxWeek)
            ).ToList();

            if (valid.Count == 0) return _encounters.FirstOrDefault(e => !e.isBoss);

            // Pick based on week as a simple deterministic selection
            return valid[weekNumber % valid.Count];
        }

        public MonsterEncounterSO GetBossForMonth(int monthNumber)
        {
            var bosses = _encounters.Where(e => e.isBoss).ToList();
            if (bosses.Count == 0) return null;
            return bosses[(monthNumber - 1) % bosses.Count];
        }

        public IEnumerable<MonsterEncounterSO> GetAll() => _encounters;
    }
}
