using System;
using System.Collections.Generic;
using UnityEngine.UIElements;
using MagicGirls.Core.Enums;
using MagicGirls.Core.Systems;

namespace MagicGirls.UI.Widgets
{
    public class ScheduleSlotWidget : VisualElement
    {
        private readonly Button _button;
        private readonly int _segmentIndex;
        private readonly string _characterId;
        private ActivityType _currentActivity = ActivityType.Rest;
        private readonly List<ActivityType> _availableActivities;

        public event Action<string, int, ActivityType> OnActivityChanged;

        public ScheduleSlotWidget(string characterId, int segmentIndex, List<ActivityType> availableActivities)
        {
            _characterId = characterId;
            _segmentIndex = segmentIndex;
            _availableActivities = availableActivities;

            AddToClassList("schedule-slot");

            if (ScheduleSystem.IsNightSegment(segmentIndex))
                AddToClassList("schedule-slot-night");

            _button = new Button();
            _button.text = GetActivityShortName(_currentActivity);
            _button.AddToClassList("schedule-slot-btn");
            _button.clicked += CycleActivity;
            Add(_button);
        }

        private void CycleActivity()
        {
            int currentIndex = _availableActivities.IndexOf(_currentActivity);
            int nextIndex = (currentIndex + 1) % _availableActivities.Count;
            SetActivity(_availableActivities[nextIndex]);
        }

        public void SetActivity(ActivityType activity)
        {
            _currentActivity = activity;
            _button.text = GetActivityShortName(activity);

            // Update styling based on activity
            _button.RemoveFromClassList("slot-rest");
            _button.RemoveFromClassList("slot-train");
            _button.RemoveFromClassList("slot-school");
            _button.RemoveFromClassList("slot-dungeon");
            _button.RemoveFromClassList("slot-recreation");
            _button.RemoveFromClassList("slot-work");

            _button.AddToClassList($"slot-{activity.ToString().ToLower()}");

            OnActivityChanged?.Invoke(_characterId, _segmentIndex, activity);
        }

        public ActivityType CurrentActivity => _currentActivity;

        private static string GetActivityShortName(ActivityType activity)
        {
            return activity switch
            {
                ActivityType.Rest => "REST",
                ActivityType.Train => "TRAIN",
                ActivityType.School => "STUDY",
                ActivityType.DungeonCrawl => "DGNCRWL",
                ActivityType.Recreation => "FUN",
                ActivityType.Work => "WORK",
                _ => "???"
            };
        }
    }
}
