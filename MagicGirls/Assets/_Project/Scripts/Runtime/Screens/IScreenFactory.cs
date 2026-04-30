

using System.Threading.Tasks;

namespace MagicGirls.Runtime.Screens
{
    public interface IScreenFactory
    {
        public Task<IScreen> CreateAsync(ScreenData data);
    }
}
