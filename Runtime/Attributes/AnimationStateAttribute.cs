using UnityEngine;

namespace KOOP.Attributes
{
    [System.AttributeUsage(System.AttributeTargets.Field)]
    public class AnimationStateAttribute : PropertyAttribute
    {
        public AnimationStateAttribute() {}
        public AnimationStateAttribute(bool findInChildren) 
        {
            this.findInChildren = findInChildren;
        }

        public bool findInChildren = true;
    }
}
