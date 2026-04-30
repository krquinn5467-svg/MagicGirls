using System.Collections.Generic;
using System;
using System.Linq;

namespace MagicGirls.Runtime.Screens
{
    public class ScreenRegistry
    {

        private readonly Dictionary<Type, ScreenData> _map;

        public ScreenRegistry(ScreenRegistryAsset asset)
        {
            _map = asset.Screens.ToDictionary(
                s => s.ScreenType.Type,
                s => s
            );
        }

        public ScreenData GetInfo<T>() where T : IScreen
            => _map[typeof(T)];

    }
}
