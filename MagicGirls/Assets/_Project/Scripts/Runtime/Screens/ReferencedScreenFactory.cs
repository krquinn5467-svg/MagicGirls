using System.Threading.Tasks;
using Unity.UIElements;
using UnityEngine.UIElements;
using MagicGirls.UI;
using System;
using System.Diagnostics;
using System.Reflection;

namespace MagicGirls.Runtime.Screens
{
    public class ReferencedScreenFactory : IScreenFactory
    {

        public Task<IScreen> CreateAsync(ScreenData info)
        {
            Type type = info.ScreenType;

            try
            {
                ConstructorInfo ctor = type.GetConstructor(new[] { typeof(VisualTreeAsset)});
                IScreen screen = (IScreen)ctor.Invoke(new object[] { info.VisualTreeAsset });
                return Task.FromResult(screen);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"Could not create screen of type {type}: {ex}");
                return null;
            }
        }
    }
}
