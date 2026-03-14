using System;
using MagicGirls.Core.Enums;

namespace MagicGirls.Core.Models
{
    [Serializable]
    public class GameCalendar
    {
        public int CurrentSegmentIndex { get; set; }
        public int CurrentDay { get; set; } = 1;
        public int CurrentWeek { get; set; } = 1;
        public int CurrentMonth { get; set; } = 1;

        public const int SegmentsPerDay = 6;
        public const int DaysPerWeek = 7;
        public const int WeeksPerMonth = 4;

        public TimeSegment CurrentSegment => (TimeSegment)CurrentSegmentIndex;

        public bool IsNight => CurrentSegmentIndex >= (int)TimeSegment.Night1;

        public int TotalDayInMonth => (CurrentWeek - 1) * DaysPerWeek + CurrentDay;

        public string GetTimeOfDayLabel()
        {
            return CurrentSegment switch
            {
                TimeSegment.Morning1 => "Early Morning",
                TimeSegment.Morning2 => "Late Morning",
                TimeSegment.Day1 => "Early Afternoon",
                TimeSegment.Day2 => "Late Afternoon",
                TimeSegment.Night1 => "Evening",
                TimeSegment.Night2 => "Late Night",
                _ => "Unknown"
            };
        }

        public string GetDayOfWeekLabel()
        {
            return CurrentDay switch
            {
                1 => "Monday",
                2 => "Tuesday",
                3 => "Wednesday",
                4 => "Thursday",
                5 => "Friday",
                6 => "Saturday",
                7 => "Sunday",
                _ => $"Day {CurrentDay}"
            };
        }

        public override string ToString()
        {
            return $"Month {CurrentMonth}, Week {CurrentWeek}, {GetDayOfWeekLabel()} - {GetTimeOfDayLabel()}";
        }
    }
}
