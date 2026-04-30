using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UIElements;
using MagicGirls.UI;
using MagicGirls.Runtime.Screens;
using System.Threading.Tasks;

namespace MagicGirls.Runtime
{
    public class ScreenManager
    {
        [SerializeField] private UIDocument uiDocument;
        
        private readonly Dictionary<string, BaseScreen> _screens = new();
        private IScreen _activeScreen;

        private VisualElement _root;
        private ScreenRegistry _registry;
        private IScreenFactory _factory;

        public static ScreenManager Instance;

        public ScreenManager(ScreenRegistry registry, IScreenFactory factory, VisualElement root)
        {
            Instance = this;
            _registry = registry;
            _factory = factory;
            _root = root;
        }

        public async Task<IScreen> ShowScreenAsync<T>() where T : IScreen
        {
            var info = _registry.GetInfo<T>();
            var screen = await _factory.CreateAsync(info);

            if (_activeScreen != null)
            {
                _activeScreen.Hide();
                _activeScreen.Root.style.display = DisplayStyle.None;
            }

            _activeScreen = screen;
            screen.Root.style.display = DisplayStyle.Flex;

            _root.Add(screen.Root);
            screen.Show();

            return screen;
        }

        public void ShowScreen(string name)
        {
            if (_activeScreen != null)
            {
                _activeScreen.Hide();
                _activeScreen.Root.style.display = DisplayStyle.None;
            }

            if (_screens.TryGetValue(name, out var screen))
            {
                _activeScreen = screen;
                screen.Root.style.display = DisplayStyle.Flex;
                screen.Show();
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
            _root.Add(overlay.Root);
            overlay.Show();
        }

        public void HideOverlay(BaseScreen overlay)
        {
            overlay.Hide();
            overlay.Root.style.display = DisplayStyle.None;
            overlay.Root.RemoveFromHierarchy();
        }

        public void ShowOverlay(string name)
        {
            if (_screens.TryGetValue(name, out var screen))
            {
                ShowOverlay(screen);
            }
        }

        public void HideOverlay(string name)
        {
            if (_screens.TryGetValue(name, out var screen))
            {
                HideOverlay(screen);
            }
        }
    }
}
