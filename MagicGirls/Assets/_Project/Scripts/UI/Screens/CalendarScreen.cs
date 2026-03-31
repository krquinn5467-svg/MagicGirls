using UnityEngine;
using UnityEngine.UIElements;
using MagicGirls.Core.Models;
using MagicGirls.Runtime;

namespace MagicGirls.UI.Screens
{
    public class CalendarScreen : BaseScreen
    {
        private GameManager _gameManager;
        private UIManager _uiManager;

        private Label _monthLabel;
        private VisualElement _calGrid;
        private Label _calStatus;
        private Label _calMilestone;
        private Button _btnBack;

        public CalendarScreen(VisualTreeAsset template) : base(template)
        {
        }

        public void Initialize(GameManager gameManager, UIManager uiManager)
        {
            _gameManager = gameManager;
            _uiManager = uiManager;

            _monthLabel = QL("month-label");
            _calGrid = Q("cal-grid");
            _calStatus = QL("cal-status");
            _calMilestone = QL("cal-milestone");
            _btnBack = QB("btn-back");

            _btnBack.clicked += () => _uiManager.ShowScreen("Hub");
        }

        public override void OnShow()
        {
            BuildCalendar();
        }

        public override void OnHide() { }
        public override void BindData() { }

        private void BuildCalendar()
        {
            var cal = _gameManager.Time.Calendar;
            _monthLabel.text = $"Month {cal.CurrentMonth}";

            _calGrid.Clear();

            for (int week = 1; week <= GameCalendar.WeeksPerMonth; week++)
            {
                var row = new VisualElement();
                row.AddToClassList("cal-row");

                var weekLabel = new Label($"Week {week}");
                weekLabel.AddToClassList("cal-week-label");
                row.Add(weekLabel);

                var dayContainer = new VisualElement();
                dayContainer.AddToClassList("cal-day-container");
                row.Add(dayContainer);

                for (int day = 1; day <= GameCalendar.DaysPerWeek; day++)
                {
                    var dayCell = new VisualElement();
                    dayCell.AddToClassList("cal-day");

                    int dayNumber = (week - 1) * GameCalendar.DaysPerWeek + day;
                    var dayLabel = new Label(dayNumber.ToString());
                    dayLabel.AddToClassList("cal-day-date-label");
                    dayLabel.AddToClassList("cal-day-label");
                    dayCell.Add(dayLabel);

                    // Highlight current day
                    bool isCurrent = week == cal.CurrentWeek && day == cal.CurrentDay;
                    bool isPast = week < cal.CurrentWeek || (week == cal.CurrentWeek && day < cal.CurrentDay);
                    bool isBattleDay = day == GameCalendar.DaysPerWeek; // Sunday = battle day
                    bool isMilestone = week == GameCalendar.WeeksPerMonth && day == GameCalendar.DaysPerWeek;

                    if (isCurrent)
                        dayCell.AddToClassList("cal-day-current");
                    else if (isPast)
                        dayCell.AddToClassList("cal-day-past");

                    if (isBattleDay && !isPast)
                    {
                        dayCell.AddToClassList("cal-day-battle");
                        var battleTag = new Label("BATTLE");
                        battleTag.AddToClassList("cal-day-battle-label");
                        battleTag.AddToClassList("cal-day-label");
                        dayCell.Add(battleTag);
                    }

                    if (isMilestone && !isPast)
                    {
                        dayCell.AddToClassList("cal-day-milestone");
                        var msTag = new Label("BOSS");
                        msTag.AddToClassList("cal-day-boss-label");
                        msTag.AddToClassList("cal-day-label");
                        dayCell.Add(msTag);
                    }

                    dayContainer.Add(dayCell);
                }

                _calGrid.Add(row);
            }

            // Status text
            int totalDay = cal.TotalDayInMonth;
            int totalDays = GameCalendar.WeeksPerMonth * GameCalendar.DaysPerWeek;
            _calStatus.text = $"Day {totalDay} of {totalDays} | {cal.GetDayOfWeekLabel()} | Week {cal.CurrentWeek}";

            int daysUntilBattle = GameCalendar.DaysPerWeek - cal.CurrentDay;
            if (daysUntilBattle > 0)
                _calMilestone.text = $"Next battle in {daysUntilBattle} day(s)";
            else
                _calMilestone.text = "Battle day!";
        }
    }
}
