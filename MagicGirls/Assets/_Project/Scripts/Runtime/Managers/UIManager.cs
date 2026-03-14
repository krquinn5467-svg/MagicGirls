using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using MagicGirls.UI;

namespace MagicGirls.Runtime
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private UIDocument uiDocument;

        private readonly Dictionary<string, BaseScreen> _screens = new();
        private BaseScreen _activeScreen;

        public VisualElement Root => uiDocument.rootVisualElement;

        public void RegisterScreen(string name, BaseScreen screen)
        {
            _screens[name] = screen;
            screen.Root.style.display = DisplayStyle.None;
            Root.Add(screen.Root);
        }

        public void ShowScreen(string name)
        {
            if (_activeScreen != null)
            {
                _activeScreen.OnHide();
                _activeScreen.Root.style.display = DisplayStyle.None;
            }

            if (_screens.TryGetValue(name, out var screen))
            {
                _activeScreen = screen;
                screen.Root.style.display = DisplayStyle.Flex;
                screen.OnShow();
            }
            else
            {
                Debug.LogError($"Screen '{name}' not registered.");
            }
        }

        public T GetScreen<T>(string name) where T : BaseScreen
        {
            return _screens.TryGetValue(name, out var screen) ? screen as T : null;
        }

        public void ShowOverlay(BaseScreen overlay)
        {
            overlay.Root.style.display = DisplayStyle.Flex;
            Root.Add(overlay.Root);
            overlay.OnShow();
        }

        public void HideOverlay(BaseScreen overlay)
        {
            overlay.OnHide();
            overlay.Root.style.display = DisplayStyle.None;
            overlay.Root.RemoveFromHierarchy();
        }

        public void ShowOverlay(string name)
        {
            if (_screens.TryGetValue(name, out var screen))
                ShowOverlay(screen);
        }

        public void HideOverlay(string name)
        {
            if (_screens.TryGetValue(name, out var screen))
                HideOverlay(screen);
        }
    }
}
