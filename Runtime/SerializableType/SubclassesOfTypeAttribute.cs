using UnityEngine;

namespace KOOP
{
    [System.AttributeUsage(System.AttributeTargets.Field)]
    public class SubclassesOfTypeAttribute : PropertyAttribute
    {
        public SubclassesOfTypeAttribute(System.Type type, bool excludeBaseType = false) 
        {
            SubclassesOf = type;
            IncludeBaseType = !excludeBaseType;
        } 

        public System.Type SubclassesOf { get; private set; }
        public bool IncludeBaseType { get; private set; }
    }
}