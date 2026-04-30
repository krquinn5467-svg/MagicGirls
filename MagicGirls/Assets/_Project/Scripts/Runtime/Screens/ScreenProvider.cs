using System.Threading.Tasks;

namespace MagicGirls.Runtime.Screens
{
    public class ScreenProvider
    {
        private readonly ScreenRegistry _registry;
        private readonly IScreenFactory _factory;

        public ScreenProvider(ScreenRegistry registry, IScreenFactory factory)
        {
            _registry = registry;
            _factory = factory;
        }

        public async Task<T> GetScreenAsync<T>() where T : IScreen
        {
            var info = _registry.GetInfo<T>();
            var screen = await _factory.CreateAsync(info);
            return (T)screen;
        }

    }
}
