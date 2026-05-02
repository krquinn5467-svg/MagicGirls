using UnityEngine.UIElements;
using MagicGirls.Runtime.Screens;

namespace MagicGirls.UI
{
    public abstract class BaseScreen: IScreen
    {
        public VisualElement Root { get; protected set; }
        private bool IsInitialized;

        public BaseScreen()
        {
            Root = new VisualElement();
            Root.style.flexGrow = 1;
        }
  
        public BaseScreen(VisualTreeAsset template)
        {
            Root = template.Instantiate();
            Root.style.flexGrow = 1;
        }

        public abstract void Show();
        public abstract void Hide();
        public abstract void Initialize();
        public abstract void BindData();

        protected VisualElement Q(string name) => Root.Q(name);
        protected T Q<T>(string name) where T : VisualElement => Root.Q<T>(name);
        protected Label QL(string name) => Root.Q<Label>(name);
        protected Button QB(string name) => Root.Q<Button>(name);
    }
}
