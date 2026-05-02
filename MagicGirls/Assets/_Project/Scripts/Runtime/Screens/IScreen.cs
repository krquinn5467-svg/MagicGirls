using UnityEngine.UIElements;

namespace MagicGirls.Runtime.Screens
{
    public interface IScreen
    {
        VisualElement Root { get; }
        void Show();
        void Hide();
        void Initialize();
    }
}
