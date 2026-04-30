using UnityEngine.UIElements;
using System;
using UnityEngine;
using UnityEditor;
using MagicGirls.Utils;

namespace MagicGirls.Runtime.Screens
{
    [Serializable]
    public class ScreenData
    {
        public string Id;
        public bool IsOverlay;
        public VisualTreeAsset VisualTreeAsset;
        public SerializableType ScreenType;

        //public AssetReferenceGameObject AddressableRef;
    }
}
