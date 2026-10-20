using UnityEngine;

namespace KOOP
{
    /// <summary>
    /// @owner: Max
    /// A serializable type class.<para/>
    /// Serializes the assembly-qualified name of the type, and pulls the System.Type when required.<para/>
    /// Use in conjunction with <see cref="SubclassesOfTypeAttribute"/>. Without attribute, it'll default to subclasses of <see cref="MonoBehaviour"/>.
    /// </summary>
    [System.Serializable]
    public class SerializableType
    {
        [SerializeField]
        private string _assemblyQualifiedName;
        public string AssemblyQualifiedName 
        { 
            get => _assemblyQualifiedName;
            set
            { 
                _assemblyQualifiedName = value;
                RefreshSystemType(); 
            } 
        }

        public bool IsValid => Type != null;
        
        private System.Type _type;	
        public System.Type Type
        {
            get 	
            {
    #if UNITY_EDITOR
                RefreshSystemType();
    #else
                if (_type == null)
                {
                    RefreshSystemType();
                }
    #endif

                return _type;
            }
            set
            {
                DoSetType(value);
            }
        }

        public SerializableType(string typeName)
        {
            TrySetWithAssemblyQualifiedName(typeName);
        }

        public SerializableType(System.Type newType)
        {
            Type = newType;
        }

        public bool TrySetWithAssemblyQualifiedName(string assemblyQualifiedName)
        {
            if (string.IsNullOrEmpty(assemblyQualifiedName))
            {
                return false;
            }

            System.Type newType = System.Type.GetType(assemblyQualifiedName);
            if (newType == null)
            {
                return false;
            }

            Type = newType;
            return true;
        }

        private void DoSetType(System.Type value)
        {
            _type = value;
            _assemblyQualifiedName = _type != null ? _type.AssemblyQualifiedName : string.Empty;
        }
        
        public void RefreshSystemType()
        {
            if (string.IsNullOrEmpty(_assemblyQualifiedName)) 
            { 
                _type = null; 
                return;
            }

            _type = System.Type.GetType(_assemblyQualifiedName);
        }
        
        public static implicit operator System.Type(SerializableType t) => t.Type;
        public static implicit operator SerializableType(System.Type t) => new SerializableType(t);
        
        public override bool Equals(System.Object obj)
        {
            SerializableType temp = obj as SerializableType;
            if (temp == null)
            {
                return false;
            }

            if (!temp.IsValid)
            {
                return IsValid;
            }

            return temp.Type.Equals(Type);
        }
        
        public static bool operator ==(SerializableType a, SerializableType b)
        {
            // If both are null, or both are same instance, return true.
            if (System.Object.ReferenceEquals(a, b))
            {
                return true;
            }
        
            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }
        
            return a.Type.Equals(b);
        }
        
        public static bool operator !=(SerializableType a, SerializableType b)
        {
            return !(a == b);
        }

        // converts System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089 -> String
        public static string AssemblyQualifiedToTypeName(string assemblyQualifiedName)
        {
            if (string.IsNullOrEmpty(assemblyQualifiedName)) 
            {
                return string.Empty;
            }

            string[] splitFromComma = assemblyQualifiedName.Split(',', 2);

            // System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089 -> System.String
            string[] splitFromFullStops = splitFromComma[0].Split('.'); 

            // System.String -> String
            return splitFromFullStops[^1].Trim(); 
        }
        
        // looks like pointless override but actually prevents warnings
        public override int GetHashCode() => base.GetHashCode();
    }
}