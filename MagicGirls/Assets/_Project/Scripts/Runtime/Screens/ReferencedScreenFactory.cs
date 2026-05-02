using System.Threading.Tasks;
using Unity.UIElements;
using UnityEngine.UIElements;
using MagicGirls.UI;
using System;
using System.Diagnostics;
using System.Reflection;
using MagicGirls.Runtime.Controllers;
using MagicGirls.Runtime.Managers;

namespace MagicGirls.Runtime.Screens
{
    public class ReferencedScreenFactory : IScreenFactory
    {
        private ScreenManager _screenManager;
        private GameManager _gameManager;
        private BattleController _battleController;

        public ReferencedScreenFactory(ScreenManager screenManager, GameManager gameManager, BattleController battleController)
        {
            _screenManager = screenManager;
            _gameManager = gameManager;
            _battleController = battleController;
        }
        
        public Task<IScreen> CreateAsync(ScreenData info)
        {
            Type type = info.ScreenType;

            try
            {
                ConstructorInfo ctor = type.GetConstructor(new[] { typeof(VisualTreeAsset)});
                IScreen screen = (IScreen)ctor.Invoke(new object[] { info.VisualTreeAsset });

                if (screen is IUsesGameManager ugm)
                {
                    ugm.SetGameManager(_gameManager);    
                }
                if (screen is IUsesScreenManager usm)
                {
                    usm.SetScreenManager(_screenManager);
                }
                if (screen is IUsesBattleController ubc)
                {
                    ubc.SetBattleController(_battleController);
                }

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
