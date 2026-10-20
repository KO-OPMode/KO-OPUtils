using UnityEngine;

namespace KOOP.Attributes
{
    [System.AttributeUsage(System.AttributeTargets.Field)]
    public class AnimationParameterAttribute : PropertyAttribute
    {
        public AnimationParameterAttribute() {}
        public AnimationParameterAttribute(bool findInChildren) 
        {
            this.findInChildren = findInChildren;
        }

        public bool findInChildren = true;
    }
}
