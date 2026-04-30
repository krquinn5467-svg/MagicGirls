using System;
using UnityEngine;

namespace MagicGirls.Utils
{
    [Serializable]
    public class SerializableType
    {
        [SerializeField] 
        private string _assemblyQualifiedName;

        public Type Type
        {
            get
            {
                if (string.IsNullOrEmpty(_assemblyQualifiedName))
                    return null;

                return System.Type.GetType(_assemblyQualifiedName);
            }
        }

        public SerializableType(Type type)
        {
            Set(type);
        }

        public void Set(Type type)
        {
            _assemblyQualifiedName = type != null 
                ? type.AssemblyQualifiedName 
                : null;
        }

        // Allow implicit conversion from SerializableType → Type
        public static implicit operator Type(SerializableType serializable)
        {
            return serializable?.Type;
        }

        // Allow implicit conversion from Type → SerializableType
        public static implicit operator SerializableType(Type type)
        {
            return new SerializableType(type);
        }

        public override string ToString()
        {
            return Type != null ? Type.FullName : "(null)";
        }
    }
}
