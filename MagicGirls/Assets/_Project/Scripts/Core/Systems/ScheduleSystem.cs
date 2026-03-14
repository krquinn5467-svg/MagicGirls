using System;
using System.Collections.Generic;
using System.Linq;
using MagicGirls.Core.Enums;
using MagicGirls.Core.Models;

namespace MagicGirls.Core.Systems
{
    public class ScheduleSystem
    {
        // characterId -> segment index -> activity
        private readonly Dictionary<string, ActivityType[]> _daySchedule = new();

        public event Action OnScheduleChanged;

        public void InitializeForCharacter(string characterId)
        {
            _daySchedule[characterId] = new ActivityType[GameCalendar.SegmentsPerDay];
            // Default everything to Rest
            for (int i = 0; i < GameCalendar.SegmentsPerDay; i++)
                _daySchedule[characterId][i] = ActivityType.Rest;

            OnScheduleChanged?.Invoke();
        }

        public void SetActivity(string characterId, int segmentIndex, ActivityType activity)
        {
            if (!_daySchedule.ContainsKey(characterId))
                InitializeForCharacter(characterId);

            if (segmentIndex < 0 || segmentIndex >= GameCalendar.SegmentsPerDay)
                return;

            // Validate night-only
            if (activity == ActivityType.DungeonCrawl && !IsNightSegment(segmentIndex))
                return;

            _daySchedule[characterId][segmentIndex] = activity;
            OnScheduleChanged?.Invoke();
        }

        public ActivityType GetActivity(string characterId, int segmentIndex)
        {
            if (!_daySchedule.ContainsKey(characterId))
                return ActivityType.Rest;

            if (segmentIndex < 0 || segmentIndex >= GameCalendar.SegmentsPerDay)
                return ActivityType.Rest;

            return _daySchedule[characterId][segmentIndex];
        }

        public ActivityType[] GetDaySchedule(string characterId)
        {
            if (!_daySchedule.ContainsKey(characterId))
            {
                InitializeForCharacter(characterId);
            }
            return _daySchedule[characterId];
        }

        public List<ActivityType> GetAvailableActivities(int segmentIndex)
        {
            var activities = new List<ActivityType>
            {
                ActivityType.Rest,
                ActivityType.Train,
                ActivityType.School,
                ActivityType.Recreation,
                ActivityType.Work
            };

            if (IsNightSegment(segmentIndex))
                activities.Add(ActivityType.DungeonCrawl);

            return activities;
        }

        public bool ValidateSchedule(string characterId)
        {
            if (!_daySchedule.ContainsKey(characterId))
                return true;

            var schedule = _daySchedule[characterId];
            for (int i = 0; i < schedule.Length; i++)
            {
                if (schedule[i] == ActivityType.DungeonCrawl && !IsNightSegment(i))
                    return false;
            }
            return true;
        }

        public void ResetDay()
        {
            foreach (var charId in _daySchedule.Keys.ToList())
            {
                for (int i = 0; i < GameCalendar.SegmentsPerDay; i++)
                    _daySchedule[charId][i] = ActivityType.Rest;
            }
            OnScheduleChanged?.Invoke();
        }

        public static bool IsNightSegment(int segmentIndex)
        {
            return segmentIndex >= (int)TimeSegment.Night1;
        }

        public static string GetSegmentLabel(int segmentIndex)
        {
            return (TimeSegment)segmentIndex switch
            {
                TimeSegment.Morning1 => "AM 1",
                TimeSegment.Morning2 => "AM 2",
                TimeSegment.Day1 => "PM 1",
                TimeSegment.Day2 => "PM 2",
                TimeSegment.Night1 => "EVE",
                TimeSegment.Night2 => "NIGHT",
                _ => "???"
            };
        }
    }
}
