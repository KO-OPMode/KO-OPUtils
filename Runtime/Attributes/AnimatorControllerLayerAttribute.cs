using UnityEngine;

namespace KOOP.Attributes
{
    [System.AttributeUsage(System.AttributeTargets.Field)]
    public class AnimatorControllerLayerAttribute : PropertyAttribute
    {
        public AnimatorControllerLayerAttribute() {}
        public AnimatorControllerLayerAttribute(bool findInChildren) 
        {
            this.findInChildren = findInChildren;
        }

        public bool findInChildren = true;
    }
}
