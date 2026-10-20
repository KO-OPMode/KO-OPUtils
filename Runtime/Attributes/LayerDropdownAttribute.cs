using UnityEngine;

namespace KOOP.Attributes
{
    [System.AttributeUsage(System.AttributeTargets.Field)]
    public class LayerDropdownAttribute : PropertyAttribute 
    {
        public bool mask = false;
    }
}
