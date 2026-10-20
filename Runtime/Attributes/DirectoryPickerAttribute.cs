using UnityEngine;

namespace KOOP.Attributes
{
    [System.AttributeUsage(System.AttributeTargets.Field)]
    public class DirectoryPickerAttribute : PropertyAttribute 
    { 
        public DirectoryPickerAttribute() {}
    }
}