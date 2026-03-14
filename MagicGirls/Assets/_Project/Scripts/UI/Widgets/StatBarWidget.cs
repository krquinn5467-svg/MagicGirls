using UnityEngine.UIElements;

namespace MagicGirls.UI.Widgets
{
    public class StatBarWidget : VisualElement
    {
        private readonly Label _label;
        private readonly VisualElement _fill;
        private readonly Label _valueLabel;

        public StatBarWidget(string statName, int current, int max, string fillColor = "#FF69B4")
        {
            AddToClassList("stat-bar-container");

            _label = new Label(statName);
            _label.AddToClassList("stat-bar-label");
            Add(_label);

            var track = new VisualElement();
            track.AddToClassList("stat-bar-track");

            _fill = new VisualElement();
            _fill.AddToClassList("stat-bar-fill");
            _fill.style.backgroundColor = ColorFromHex(fillColor);
            track.Add(_fill);
            Add(track);

            _valueLabel = new Label($"{current}/{max}");
            _valueLabel.AddToClassList("stat-bar-value");
            Add(_valueLabel);

            UpdateValue(current, max);
        }

        public void UpdateValue(int current, int max)
        {
            float pct = max > 0 ? (float)current / max * 100f : 0;
            _fill.style.width = new Length(pct, LengthUnit.Percent);
            _valueLabel.text = $"{current}/{max}";
        }

        public void UpdateStat(string name, int value)
        {
            _label.text = name;
            _valueLabel.text = value.ToString();
            // For flat stats (no max), show as a bar from 0-30 range
            float pct = value / 30f * 100f;
            if (pct > 100f) pct = 100f;
            _fill.style.width = new Length(pct, LengthUnit.Percent);
        }

        private static UnityEngine.Color ColorFromHex(string hex)
        {
            if (UnityEngine.ColorUtility.TryParseHtmlString(hex, out var color))
                return color;
            return UnityEngine.Color.white;
        }
    }
}
