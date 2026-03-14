using UnityEngine.UIElements;

namespace MagicGirls.UI
{
    public abstract class BaseScreen
    {
        public VisualElement Root { get; protected set; }

        protected BaseScreen()
        {
            Root = new VisualElement();
            Root.style.flexGrow = 1;
        }

        protected BaseScreen(VisualTreeAsset template)
        {
            Root = template.CloneTree();
            Root.style.flexGrow = 1;
        }

        public abstract void OnShow();
        public abstract void OnHide();
        public abstract void BindData();

        protected VisualElement Q(string name) => Root.Q(name);
        protected T Q<T>(string name) where T : VisualElement => Root.Q<T>(name);
        protected Label QL(string name) => Root.Q<Label>(name);
        protected Button QB(string name) => Root.Q<Button>(name);
    }
}
