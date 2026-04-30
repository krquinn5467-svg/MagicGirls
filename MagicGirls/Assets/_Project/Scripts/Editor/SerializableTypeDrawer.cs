#if UNITY_EDITOR
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using MagicGirls.Runtime.Screens;
using MagicGirls.Utils;

namespace MagicGirls.Editor
{
    [CustomPropertyDrawer(typeof(SerializableType))]
    public class SerializableTypeDrawer : PropertyDrawer
    {
        private static List<Type> _screenTypes;
        private static string[] _typeNames;

        private void EnsureTypeCache()
        {
            if (_screenTypes != null)
                return;

            _screenTypes = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(a =>
                {
                    try { return a.GetTypes(); }
                    catch { return Array.Empty<Type>(); }
                })
                .Where(t =>
                    typeof(IScreen).IsAssignableFrom(t) &&
                    t.IsClass &&
                    !t.IsAbstract)
                .ToList();

            _typeNames = _screenTypes
                .Select(t => t.FullName)
                .Prepend("(None)")
                .ToArray();
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EnsureTypeCache();

            SerializedProperty aqnProp = property.FindPropertyRelative("_assemblyQualifiedName");

            string currentAQN = aqnProp.stringValue;
            Type currentType = string.IsNullOrEmpty(currentAQN) ? null : Type.GetType(currentAQN);

            int currentIndex = 0;

            if (currentType != null)
            {
                int idx = _screenTypes.IndexOf(currentType);
                if (idx >= 0)
                    currentIndex = idx + 1; // +1 because "(None)" is index 0
            }

            EditorGUI.BeginProperty(position, label, property);

            int newIndex = EditorGUI.Popup(position, label.text, currentIndex, _typeNames);

            if (newIndex != currentIndex)
            {
                if (newIndex == 0)
                {
                    aqnProp.stringValue = null;
                }
                else
                {
                    Type selectedType = _screenTypes[newIndex - 1];
                    aqnProp.stringValue = selectedType.AssemblyQualifiedName;
                }
            }

            EditorGUI.EndProperty();
        }
    }
    #endif
}
