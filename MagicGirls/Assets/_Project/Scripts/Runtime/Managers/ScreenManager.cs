#nullable enable

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
        
        private readonly Dictionary<string, IScreen> _screens = new();
        private IScreen _activeScreen;
        private List<IScreen> _activeOverlays = new();

        private VisualElement _root;
        private ScreenRegistry _registry;
        private IScreenFactory _factory;

        public static ScreenManager Instance = null!;

        public ScreenManager(VisualElement root)
        {
            Instance = this;
            _root = root;
        }

        public IScreenFactory Factory
        {
            set
            {
                _factory = value;
            }
        }

        public ScreenRegistry Registry
        {
            set
            {
                _registry = value;
            }
        }

        public async Task<IScreen> GetScreen<T>() where T : IScreen
        {
            var info = _registry.GetInfo<T>();

            IScreen screen;
            if (_screens.ContainsKey(info.Id))
            {
                screen = _screens[info.Id];
            }
            else
            {
                screen = await _factory.CreateAsync(info); 
                if (info.PreserveScreenOnHide)
                {
                    _screens.Add(info.Id, screen);
                }
                screen.Initialize(); // does this need to happen after it's added to the root??
            }

            return screen;

        }

        public async Task<IScreen> ShowScreenAsync<T>() where T : IScreen
        {
            // var info = _registry.GetInfo<T>();

            // IScreen screen;
            // if (_screens.ContainsKey(info.Id))
            // {
            //     screen = _screens[info.Id];
            // }
            // else
            // {
            //     screen = await _factory.CreateAsync(info); 
            //     if (info.PreserveScreenOnHide)
            //     {
            //         _screens.Add(info.Id, screen);
            //     }
            //     screen.Initialize(); // does this need to happen after it's added to the root??
            // }

            var screen = await GetScreen<T>();

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

        
        // public void ShowScreen(string name)
        // {
        //     if (_activeScreen != null)
        //     {
        //         _activeScreen.Hide();
        //         _activeScreen.Root.style.display = DisplayStyle.None;
        //     }

        //     if (_screens.TryGetValue(name, out var screen))
        //     {
        //         _activeScreen = screen;
        //         screen.Root.style.display = DisplayStyle.Flex;
        //         screen.Show();
        //     }
        //     else
        //     {
        //         Debug.LogError($"Screen '{name}' not registered.");
        //     }
        // }

        // public T? GetScreen<T>(string name) where T : IScreen
        // {
        //     return _screens.TryGetValue(name, out var screen) ? (T)screen : null;
        // }

        public async Task<IScreen> ShowOverlayAsync<T>() where T : IScreen
        {
            // var info = _registry.GetInfo<T>();

            // IScreen screen;
            // if (_screens.ContainsKey(info.Id))
            // {
            //     screen = _screens[info.Id];
            // }
            // else
            // {
            //     screen = await _factory.CreateAsync(info); 
            //     if (info.PreserveScreenOnHide)
            //     {
            //         _screens.Add(info.Id, screen);
            //     }
            // }

            var screen = await GetScreen<T>();

            _activeOverlays.Add(screen);
            screen.Root.style.display = DisplayStyle.Flex;

            _root.Add(screen.Root);
            screen.Show();

            return screen;
        }

        public void ShowOverlay(IScreen overlay)
        {
            _activeOverlays.Add(overlay);
            overlay.Root.style.display = DisplayStyle.Flex;
            _root.Add(overlay.Root);
            overlay.Show();
        }

        public void HideOverlay(IScreen overlay)
        {
            overlay.Hide();
            overlay.Root.style.display = DisplayStyle.None;
            overlay.Root.RemoveFromHierarchy();
        }

        public void HideOverlay<T>() where T : IScreen
        {
            foreach(var overlay in _activeOverlays)
            {
                if(overlay is T)
                {
                    HideOverlay(overlay);
                    _activeOverlays.Remove(overlay);
                    return;
                }
            }
        }
    }
}
