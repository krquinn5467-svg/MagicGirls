using System;
using MagicGirls.Core.Enums;
using MagicGirls.Core.Models;

namespace MagicGirls.Core.Systems
{
    public class TimeSystem
    {
        public GameCalendar Calendar { get; private set; } = new();

        public event Action<TimeSegment> OnSegmentAdvanced;
        public event Action<int> OnDayAdvanced;
        public event Action<int> OnWeekCompleted;
        public event Action<int> OnMonthCompleted;

        public bool IsNight => Calendar.IsNight;
        public TimeSegment CurrentSegment => Calendar.CurrentSegment;

        public void AdvanceSegment()
        {
            Calendar.CurrentSegmentIndex++;

            if (Calendar.CurrentSegmentIndex >= GameCalendar.SegmentsPerDay)
            {
                // Day is over
                Calendar.CurrentSegmentIndex = 0;
                AdvanceDay();
            }
            else
            {
                OnSegmentAdvanced?.Invoke(Calendar.CurrentSegment);
            }
        }

        private void AdvanceDay()
        {
            Calendar.CurrentDay++;
            OnDayAdvanced?.Invoke(Calendar.CurrentDay);

            if (Calendar.CurrentDay > GameCalendar.DaysPerWeek)
            {
                // Week is over
                Calendar.CurrentDay = 1;
                OnWeekCompleted?.Invoke(Calendar.CurrentWeek);
                AdvanceWeek();
            }
        }

        private void AdvanceWeek()
        {
            Calendar.CurrentWeek++;

            if (Calendar.CurrentWeek > GameCalendar.WeeksPerMonth)
            {
                // Month is over
                Calendar.CurrentWeek = 1;
                Calendar.CurrentMonth++;
                OnMonthCompleted?.Invoke(Calendar.CurrentMonth - 1);
            }
        }

        public bool IsActivityAvailable(ActivityType activity)
        {
            if (activity == ActivityType.DungeonCrawl)
                return IsNight;
            return true;
        }

        public string GetStatusText()
        {
            return Calendar.ToString();
        }
    }
}
