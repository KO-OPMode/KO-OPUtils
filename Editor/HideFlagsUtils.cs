#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

namespace KOOPEditor
{
    public static class HideFlagsUtils
    {
        private const string _hideFlagsContextMenu = "CONTEXT/Component/HideFlags/";
        private const string _hideFlagsGameObjectContextMenu = _hideFlagsContextMenu + "GameObject/";
        private const string _hideFlagsComponentContextMenu = _hideFlagsContextMenu + "Component/";

        [MenuItem(_hideFlagsGameObjectContextMenu + nameof(HideFlags.None))]
        public static void SetHideFlags_GameObject_None(MenuCommand cmd)
            => SetHideFlags_GameObject(cmd, HideFlags.None);

        [MenuItem(_hideFlagsGameObjectContextMenu + nameof(HideFlags.None), validate = true)]
        public static bool Validate_SetHideFlags_GameObject_None(MenuCommand cmd)
            => Validate_SetHideFlags_GameObject(cmd, HideFlags.None);

        [MenuItem(_hideFlagsGameObjectContextMenu + nameof(HideFlags.HideInHierarchy))]
        public static void SetHideFlags_GameObject_HideInHierarchy(MenuCommand cmd)
            => SetHideFlags_GameObject(cmd, HideFlags.HideInHierarchy);

        [MenuItem(_hideFlagsGameObjectContextMenu + nameof(HideFlags.HideInHierarchy), validate = true)]
        public static bool Validate_SetHideFlags_GameObject_HideInHierarchy(MenuCommand cmd)
            => Validate_SetHideFlags_GameObject(cmd, HideFlags.HideInHierarchy);

        private static void SetHideFlags_GameObject(MenuCommand cmd, HideFlags hideFlags)
        {
            if (cmd.context is Component component)
            {
                SetHideFlags_GameObject(component, hideFlags);
            }
        }

        private static void SetHideFlags_GameObject(Object obj, HideFlags hideFlags)
        {
            if (obj is Component component)
            {
                component.gameObject.hideFlags = hideFlags;
                SetHideFlags_GameObject(component.transform.parent, hideFlags);
            }
        }

        private static bool Validate_SetHideFlags_GameObject(MenuCommand cmd, HideFlags hideFlags)
        {
            bool output = false;
            bool isChecked = false;
            if (cmd.context is Component component)
            {
                output = true;
                isChecked = component.gameObject.hideFlags.HasFlag(hideFlags);
            }
            else
            {
                output = false;
            }
            
            Menu.SetChecked(_hideFlagsGameObjectContextMenu + nameof(HideFlags.None), isChecked);
            return output;
        }
    }
}

#endif