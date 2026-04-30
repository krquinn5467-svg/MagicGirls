using System.Collections.Generic;
using UnityEngine;

namespace MagicGirls.Runtime.Screens
{
    [CreateAssetMenu(menuName = "UI/Screen Registry", fileName = "Screen Registry")]
    public class ScreenRegistryAsset:ScriptableObject
    {
        public List<ScreenData> Screens;
    }
}
