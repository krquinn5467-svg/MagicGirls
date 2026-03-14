using System.Collections.Generic;
using System.Linq;
using MagicGirls.Core.Enums;
using MagicGirls.Data;

namespace MagicGirls.Runtime
{
    public class ActivityRegistry
    {
        private readonly Dictionary<ActivityType, ActivityDefinitionSO> _activities = new();

        public ActivityRegistry(ActivityDefinitionSO[] definitions)
        {
            foreach (var def in definitions)
            {
                _activities[def.activityType] = def;
            }
        }

        public ActivityDefinitionSO Get(ActivityType type)
        {
            return _activities.TryGetValue(type, out var def) ? def : null;
        }

        public IEnumerable<ActivityDefinitionSO> GetAll()
        {
            return _activities.Values;
        }

        public IEnumerable<ActivityDefinitionSO> GetAvailable(bool isNight)
        {
            return _activities.Values.Where(a => !a.nightOnly || isNight);
        }
    }
}
